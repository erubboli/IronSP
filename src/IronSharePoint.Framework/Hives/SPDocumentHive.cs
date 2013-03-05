using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.SharePoint;

namespace IronSharePoint.Framework.Hives
{
    /// <summary>
    /// Hive implementation where the files are retrieved from a SharePoint Document Library
    /// </summary>
    public class SPDocumentHive : IHive
    {
        private Guid _siteId;
        private string _hiveLibraryPath;
        private string _webUrl;

        private string[] _cachedFiles;
        private string[] _cachedDirs;

        public IEnumerable<string> CachedFiles
        {
            get { return _cachedFiles.AsEnumerable(); }
        } 

        public IEnumerable<string> CachedDirs
        {
            get { return _cachedDirs.AsEnumerable(); }
        } 

        public SPDocumentHive(Guid siteId, string hiveLibraryPath = IronConstant.HiveLibraryPath)
        {
            _siteId = siteId;
            _hiveLibraryPath = hiveLibraryPath;
            Reset();
        }

        public bool FileExists(string path)
        {
            return _cachedFiles.Contains(path);
        }

        public bool DirectoryExists(string path)
        {
            return _cachedDirs.Contains(path);
        }

        public Stream OpenInputFileStream(string path)
        {
            return OpenLibrary(lib =>
                {
                    var spFile = lib.ParentWeb.GetFile(path);
                    if (!spFile.Exists) throw new FileNotFoundException("", path);

                    return spFile.OpenBinaryStream();
                });
        }

        public Stream OpenOutputFileStream(string path)
        {
            return OpenInputFileStream(path);
        }

        public string GetFullPath(string path)
        {
            return string.Format("{0}/{1}/{2}", _webUrl, _hiveLibraryPath, path);
        }

        public void Reset()
        {
            var allFilesQuery = new SPQuery();
            allFilesQuery.Query = "<Where></Where>";
            allFilesQuery.ViewFields = "<FieldRef Name='FileRef'/><FieldRef Name='FileLeafRef'/>";
            allFilesQuery.ViewAttributes = "Scope='Recursive'";
            allFilesQuery.IncludeMandatoryColumns = false;

            var allFiles = new List<string>();

            OpenLibrary(lib =>
                {
                    _webUrl = lib.ParentWebUrl;

                     var allItems = lib.GetItems(allFilesQuery);
                     foreach (SPListItem item in allItems)
                     {
                         var fileRef = item["FileRef"].ToString();
                         var siteRelative = fileRef.Replace(lib.ParentWeb.ServerRelativeUrl, string.Empty).TrimStart('/');
                         var hiveRelative = siteRelative.Replace(_hiveLibraryPath,string.Empty).TrimStart('/');

                         allFiles.Add(hiveRelative);
                     }
                });

            _cachedFiles = allFiles.ToArray();
// ReSharper disable PossibleNullReferenceException
            _cachedDirs = _cachedFiles.Select(x => Path.GetDirectoryName(x).Replace('\\', '/'))
                .Distinct()
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .ToArray();
// ReSharper restore PossibleNullReferenceException
        }


        private T OpenLibrary<T>(Func<SPDocumentLibrary, T> func)
        {
            var siteKey = IronHelper.GetPrefixedKey("Hive_" + _siteId);
            var context = HttpContext.Current;
            T result;

            if (context != null)
            {
                if (!context.Items.Contains(siteKey))
                {
                    // Site gets automatically disposed after http request ended
                    context.Items[siteKey] = new SPSite(_siteId, SPUserToken.SystemAccount);
                }
                var site = context.Items[siteKey] as SPSite;
                var library = GetHiveLibrary(site);
                result = func(library);
            }
            else
            {
                using (var site = new SPSite(_siteId))
                {
                    var library = GetHiveLibrary(site);
                    result = func(library);
                }
            }

            return result;
        }

        private void OpenLibrary(Action<SPDocumentLibrary> action)
        {
            var func = new Func<SPDocumentLibrary, bool>(lib =>
                {
                    action(lib);
                    return true;
                });
            OpenLibrary(func);
        }

        private SPDocumentLibrary GetHiveLibrary(SPSite site)
        {
            var web = site.RootWeb;
            var folder = web.GetFolder(_hiveLibraryPath);
            return folder.DocumentLibrary;
        }
    }
}
