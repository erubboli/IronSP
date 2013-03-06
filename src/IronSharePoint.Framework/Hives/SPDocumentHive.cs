﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        private string _hiveLibraryUrl;

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
            path = IsAbsolutePath(path) ? GetPartialPath(path) : path;

            return _cachedFiles.Contains(path);
        }

        public bool DirectoryExists(string path)
        {
            path = IsAbsolutePath(path) ? GetPartialPath(path) : path;

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
            return IsAbsolutePath(path) ? path : CombinePath(_hiveLibraryUrl, path);
        }

        public bool IsAbsolutePath(string path)
        {
            Contract.Requires<ArgumentNullException>(path != null);

            Uri uri;
            Uri.TryCreate(path, UriKind.Absolute, out uri);
            return uri != null && 
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        public string CombinePath(string path1, string path2)
        {
            Contract.Requires<ArgumentNullException>(path1 != null);
            Contract.Requires<ArgumentNullException>(path2 != null);

            path1 = path1.TrimEnd('/');
            path2 = path2.TrimStart('/');

            return string.Format("{0}/{1}", path1, path2);
        }

        public string[] GetFiles(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetDirectories(string path, string searchPattern)
        {
            throw new NotImplementedException();
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
                    _hiveLibraryUrl = CombinePath(_webUrl, _hiveLibraryPath);

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

        private string GetPartialPath(string path)
        {
            Contract.Requires<ArgumentNullException>(path != null);

            return path.Replace(_hiveLibraryUrl, "").TrimStart('/');
        }
    }
}
