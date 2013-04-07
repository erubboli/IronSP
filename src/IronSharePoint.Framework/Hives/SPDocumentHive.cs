﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using IronSharePoint.Util;
using Microsoft.SharePoint;

namespace IronSharePoint.Hives
{
    /// <summary>
    ///     Hive implementation where the files are retrieved from a SharePoint Document Library
    /// </summary>
    public class SPDocumentHive : IHive
    {
        private readonly ThreadLocal<SPDocumentLibrary> _documentLibrary;
        private readonly string _hiveLibraryPath;
        private readonly string _hiveLibraryUrl;

        private readonly ThreadLocal<SPSite> _site;
        private readonly Guid _siteId;
        private readonly ThreadLocal<SPWeb> _web;
        private string[] _allDirs;
        private Dictionary<string, int> _allFiles;

        public SPDocumentHive(Guid siteId)
            : this(siteId, IronConstant.IronHiveLibraryPath) {}

        public SPDocumentHive(Guid siteId, string hiveLibraryPath)
        {
            _siteId = siteId;
            _hiveLibraryPath = hiveLibraryPath;

            _site = new ThreadLocal<SPSite>(() => new SPSite(_siteId, SPUserToken.SystemAccount), true);
            _web = new ThreadLocal<SPWeb>(() => Site.RootWeb);
            _documentLibrary = new ThreadLocal<SPDocumentLibrary>(() => Web.GetFolder(_hiveLibraryPath).DocumentLibrary);
            _hiveLibraryUrl = CombinePath(Site.RootWeb.Url, _hiveLibraryPath);
            Reset();
        }

        public SPSite Site
        {
            get { return _site.Value; }
        }

        public SPWeb Web
        {
            get { return _web.Value; }
        }

        public SPDocumentLibrary DocumentLibrary
        {
            get { return _documentLibrary.Value; }
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
            foreach (SPSite spSite in _site.Values)
            {
                spSite.Dispose();
                _site.Dispose();
                _web.Dispose();
            }
            _documentLibrary.Dispose();
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

        public override string ToString()
        {
            return string.Format("{0}: {1}", GetType().Name, _hiveLibraryUrl);
        }

        public void Reset()
        {
            var allFilesQuery = new SPQuery
                {
                    Query = "<Where></Where>",
                    ViewFields = "<FieldRef Name='FileRef'/>" +
                                 "<FieldRef Name='ID'/>" +
                                 "<FieldRef Name='File_x0020_Size'/>" +
                                 "<FieldRef Name='FileLeafRef'/>",
                    ViewAttributes = "Scope='Recursive'",
                    IncludeMandatoryColumns = false
                };

            _allFiles = new Dictionary<string, int>();
            SPListItemCollection allItems = DocumentLibrary.GetItems(allFilesQuery);
            foreach (SPListItem item in allItems)
            {
                string fileRef = item["FileRef"].ToString();
                string siteRelative = fileRef.ReplaceFirst(Site.RootWeb.ServerRelativeUrl, string.Empty).TrimStart('/');
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