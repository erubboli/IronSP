using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace IronSharePoint.Hives
{
    /// <summary>
    /// Hive implementation which acts on a directory on a hard disk
    /// </summary>
    public class DirectoryHive : IHive
    {
        private string _root;

        public string Root { get { return _root; } }

        /// <summary>
        /// Creates a new Hive for the folder <paramref name="root"/>.
        /// </summary>
        /// <param name="root"></param>
        public DirectoryHive(string root)
        {
            Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(root), "Root directory cannot be null or blank");
            Contract.Requires<ArgumentException>(Directory.Exists(root), "Root directory can not be found");

            _root = root;
        }

        public bool FileExists(string path)
        {
            var fullPath = GetFullPath(path);

            return File.Exists(fullPath);
        }

        public bool DirectoryExists(string path)
        {
            var fullPath = GetFullPath(path);

            return Directory.Exists(fullPath);
        }

        public Stream OpenInputFileStream(string path)
        {
            path = GetFullPath(path);

            return File.OpenRead(path);
        }

        public Stream OpenOutputFileStream(string path)
        {
            path = GetFullPath(path);

            if(!File.Exists(path)) throw new FileNotFoundException("", path);
            return File.OpenWrite(path);
        }

        public string GetFullPath(string path)
        {
            return IsAbsolutePath(path) ? path : Path.Combine(Root, path);
        }

        public bool IsAbsolutePath(string path)
        {
            return Path.IsPathRooted(path);
        }

        public string CombinePath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public IEnumerable<string> GetFiles(string path, string searchPattern, bool absolutePaths = false)
        {
            path = GetFullPath(path);

            var files = DirectoryExists(path)
                            ? Directory.GetFiles(path, searchPattern).Select(Path.GetFullPath)
                            : new string[0];
            return absolutePaths ? files : files.Select(GetPartialPath);
        }

        public IEnumerable<string> GetDirectories(string path, string searchPattern, bool absolutePaths = false)
        {
            path = GetFullPath(path);

            var directories = DirectoryExists(path)
                                  ? Directory.GetDirectories(path, searchPattern).Select(Path.GetFullPath)
                                  : new string[0];
            return absolutePaths ? directories : directories.Select(GetPartialPath);
        }

        public string Name { get; set; }

        public string Description { get { return string.Format("{0}: {1}", GetType().Name, Root); } }

        private string GetPartialPath(string path)
        {
            Contract.Requires<ArgumentNullException>(path != null);

            return path.Replace(Root, "").TrimStart('\\');
        }

        public void Dispose()
        {
            // nothing to do
        }
    }
}