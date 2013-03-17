using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using IronSharePoint.Util;
using Microsoft.SharePoint;

namespace IronSharePoint.Hives
{
    /// <summary>
    /// Hive implementation where the files are retrieved from a SharePoint Document Library
    /// </summary>
    public class SPDocumentHive : IHive
    {
        private readonly Guid _siteId;
        private readonly string _hiveLibraryPath;
        private readonly string _hiveLibraryUrl;

        private readonly ThreadLocal<SPSite> _site;
        private readonly ThreadLocal<SPDocumentLibrary> _documentLibrary;

        public SPSite Site
        {
            get { return _site.Value; }
        }

        public SPDocumentLibrary DocumentLibrary
        {
            get { return _documentLibrary.Value; }
        }

        private Dictionary<string, int> _allFiles;
        private string[] _allDirs;

        public IEnumerable<string> Files
        {
            get { return _allFiles.Keys.AsEnumerable(); }
        } 

        public IEnumerable<string> Directories
        {
            get { return _allDirs.AsEnumerable(); }
        }

        public SPDocumentHive(Guid siteId)
            : this(siteId, IronConstant.IronHiveLibraryPath)
        {
        }

        public SPDocumentHive(Guid siteId, string hiveLibraryPath)
        {
            _siteId = siteId;
            _hiveLibraryPath = hiveLibraryPath;

            _site = new ThreadLocal<SPSite>(() => new SPSite(_siteId, SPUserToken.SystemAccount), true);
            _documentLibrary = new ThreadLocal<SPDocumentLibrary>(() => Site.RootWeb.GetFolder(_hiveLibraryPath).DocumentLibrary);
            _hiveLibraryUrl = CombinePath(Site.RootWeb.Url, _hiveLibraryPath);

            Reset();
        }

        public bool FileExists(string path)
        {
            path = IsAbsolutePath(path) ? GetPartialPath(path) : path;

            return Files.Contains(path);
        }

        public bool DirectoryExists(string path)
        {
            path = IsAbsolutePath(path) ? GetPartialPath(path) : path;

            return Directories.Any(x => x.StartsWith(path));
        }

        public Stream OpenInputFileStream(string path)
        {
            var item = GetSPListItem(path);
            if (item == null) throw new FileNotFoundException("", path);

            return item.File.OpenBinaryStream();
        }

        public SPListItem GetSPListItem(string path)
        {
            path = GetPartialPath(path);
            int id;
            if (_allFiles.TryGetValue(path, out id))
            {
                try
                {
                    return DocumentLibrary.GetItemById(id);
                }
                catch
                {
                    return null;
                }
            }
            return null;
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

        public IEnumerable<string> GetFiles(string path, string searchPattern, bool absolutePaths = false)
        {
            searchPattern = searchPattern.Replace("*", "[^/]+").Replace(".", "\\.")+"$";
            path = "^" + (path.Replace(".", "") + "/").TrimStart('/');
            var regexPattern = string.Format("{0}{1}", path, searchPattern);
            var regex = new Regex(regexPattern);
            var files = Files.Select(file =>
                {
                    var match = regex.Match(file);
                    return match.Success ? file : null;
                }).Where(x => x != null).Distinct().ToArray();
            return absolutePaths ? files.Select(GetFullPath) : files;
        }

        public IEnumerable<string> GetDirectories(string path, string searchPattern, bool absolutePaths = false)
        {
            path = (path.Replace(".", "") + "/").TrimStart('/');
            var regexPattern = searchPattern.Replace("*", string.Format("^({0}[^/]+)(/.*)?", path)) + "$";
            var regex = new Regex(regexPattern);
            var dirs = Directories.Select(dir =>
            {
                var match = regex.Match(dir);
                return match.Success ? match.Groups[1].Value : null;
            }).Compact().Distinct().ToArray();
            return absolutePaths ? dirs.Select(GetFullPath) : dirs;
        }

        public void Reset()
        {
            var allFilesQuery = new SPQuery();
            allFilesQuery.Query = "<Where></Where>";
            allFilesQuery.ViewFields = "<FieldRef Name='FileRef'/><FieldRef Name='ID'/><FieldRef Name='FileLeafRef'/>";
            allFilesQuery.ViewAttributes = "Scope='Recursive'";
            allFilesQuery.IncludeMandatoryColumns = false;

            _allFiles = new Dictionary<string, int>();
            var allItems = DocumentLibrary.GetItems(allFilesQuery);
            foreach (SPListItem item in allItems)
            {
                var fileRef = item["FileRef"].ToString();
                var siteRelative = fileRef.ReplaceFirst(Site.RootWeb.ServerRelativeUrl, string.Empty).TrimStart('/');
                var hiveRelative = siteRelative.ReplaceFirst(_hiveLibraryPath, string.Empty).TrimStart('/');
                var id = Convert.ToInt32(item["ID"]);

                _allFiles.Add(hiveRelative, id);
            }

            // ReSharper disable PossibleNullReferenceException
            _allDirs = _allFiles.Keys.Select(x => Path.GetDirectoryName(x).Replace('\\', '/'))
                                      .Distinct()
                                      .Where(x => !String.IsNullOrWhiteSpace(x))
                                      .ToArray();
            // ReSharper restore PossibleNullReferenceException
        }

        private string GetPartialPath(string path)
        {
            Contract.Requires<ArgumentNullException>(path != null);

            return path.Replace(_hiveLibraryUrl, "").TrimStart('/');
        }

        public void Dispose()
        {
            foreach (var spSite in _site.Values)
            {
                spSite.Dispose();
                _site.Dispose();
            }
            _documentLibrary.Dispose();
        }
    }
}
