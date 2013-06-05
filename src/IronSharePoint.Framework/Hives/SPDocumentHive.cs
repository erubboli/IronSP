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
    ///     Hive implementation where the files are retrieved from a SharePoint Document Library
    /// </summary>
    public class SPDocumentHive : IHive
    {
        class Context : IDisposable
        {
            public SPSite Site { get; set; }
            public SPWeb Web { get; set; }
            public SPDocumentLibrary DocumentLibrary { get; set; }

            public Context(Guid siteId, string hiveLibraryPath)
            {
                Site = new SPSite(siteId, SPUserToken.SystemAccount);
                Web = Site.RootWeb;
                DocumentLibrary = Web.GetFolder(hiveLibraryPath).DocumentLibrary;
            }

            public void Dispose()
            {
                Site.Dispose();
                Web.Dispose();

                Site = null;
                Web = null;
                DocumentLibrary = null;
            }
        }

        private static readonly Object Lock = new Object();
        private readonly Guid _siteId;
        private readonly string _hiveLibraryPath;
        private string _hiveLibraryUrl;

        private string[] _allDirs;
        private Dictionary<string, int> _allFiles;

        public SPDocumentHive(Guid siteId)
            : this(siteId, IronConstant.IronHiveLibraryPath) {}

        public SPDocumentHive(Guid siteId, string hiveLibraryPath)
        {
            _siteId = siteId;
            _hiveLibraryPath = hiveLibraryPath;

            Reset();
        }

        private ThreadLocal<Context> _contexts;

        Context CurrentContext
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    if (_contexts == null)
                    {
                        lock (Lock)
                        {
                            if (_contexts == null)
                            {
                                _contexts = new ThreadLocal<Context>(() => new Context(_siteId, _hiveLibraryPath), true);
                            }
                        }
                    }
                    return _contexts.Value;
                }

                var key = "IronSP_HiveCtx_" + _siteId;
                var context = HttpContext.Current.Items[key] as Context;
                if (context == null)
                {
                    lock (Lock)
                    {
                        context = HttpContext.Current.Items[key] as Context;
                        if (context == null)
                        {
                            context = new Context(_siteId, _hiveLibraryPath);
                            HttpContext.Current.Items[key] = context;
                        }
                    }
                }
                return context;
            }
        }

        private void RecycleContexts()
        {
            foreach (var context in _contexts.Values)
            {
                context.Dispose();
            }
            _contexts.Dispose();
            _contexts = null;
        }

        public IEnumerable<string> Files
        {
            get { return _allFiles.Keys.AsEnumerable(); }
        }

        public IEnumerable<string> Directories
        {
            get { return _allDirs.AsEnumerable(); }
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
            SPListItem item = GetSPListItem(path);
            if (item == null) throw new FileNotFoundException("", path);

            return item.File.OpenBinaryStream();
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
            searchPattern = searchPattern.Replace("*", "[^/]+").Replace(".", "\\.") + "$";
            path = "^" + (path.Replace(".", "") + "/").TrimStart('/');
            string regexPattern = string.Format("{0}{1}", path, searchPattern);
            var regex = new Regex(regexPattern);
            string[] files = Files.Select(file =>
            {
                Match match = regex.Match(file);
                return match.Success ? file : null;
            }).Where(x => x != null).Distinct().ToArray();
            return absolutePaths ? files.Select(GetFullPath) : files;
        }

        public IEnumerable<string> GetDirectories(string path, string searchPattern, bool absolutePaths = false)
        {
            path = (path.Replace(".", "") + "/").TrimStart('/');
            string regexPattern = searchPattern.Replace("*", string.Format("^({0}[^/]+)(/.*)?", path)) + "$";
            var regex = new Regex(regexPattern);
            string[] dirs = Directories.Select(dir =>
            {
                Match match = regex.Match(dir);
                return match.Success ? match.Groups[1].Value : null;
            }).Compact().Distinct().ToArray();
            return absolutePaths ? dirs.Select(GetFullPath) : dirs;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }

        public void Dispose()
        {
            RecycleContexts();
        }

        public SPListItem GetSPListItem(string path)
        {
            path = GetPartialPath(path);
            int id;
            if (_allFiles.TryGetValue(path, out id))
            {
                try
                {
                    return CurrentContext.DocumentLibrary.GetItemById(id);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", GetType().Name, _hiveLibraryUrl);
        }

        public void Reset()
        {
            _hiveLibraryUrl = CombinePath(CurrentContext.Site.RootWeb.Url, _hiveLibraryPath);
            var allFilesQuery = new SPQuery
                {
                    Query = "<Where></Where>",
                    ViewFields = "<FieldRef Name='FileRef'/>" +
                                 "<FieldRef Name='ID'/>" +
                                 "<FieldRef Name='FileLeafRef'/>",
                    ViewAttributes = "Scope='Recursive'",
                    IncludeMandatoryColumns = false
                };

            _allFiles = new Dictionary<string, int>();
            SPListItemCollection allItems = CurrentContext.DocumentLibrary.GetItems(allFilesQuery);
            foreach (SPListItem item in allItems)
            {
                string fileRef = item["FileRef"].ToString();
                string siteRelative =
                    fileRef.ReplaceFirst(CurrentContext.Site.RootWeb.ServerRelativeUrl, string.Empty).TrimStart('/');
                string hiveRelative = siteRelative.ReplaceFirst(_hiveLibraryPath, string.Empty).TrimStart('/');
                int id = Convert.ToInt32(item["ID"]);

                _allFiles.Add(hiveRelative, id);
            }

            // ReSharper disable PossibleNullReferenceException
            _allDirs = _allFiles.Keys.Select(x => Path.GetDirectoryName(x).Replace('\\', '/'))
                                .Distinct()
                                .Where(x => !String.IsNullOrWhiteSpace(x))
                                .ToArray();
            // ReSharper restore PossibleNullReferenceException
        }

        public string GetPartialPath(string path)
        {
            Contract.Requires<ArgumentNullException>(path != null);

            return path.Replace(_hiveLibraryUrl, "").TrimStart('/');
        }
    }
}