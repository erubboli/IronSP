using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace IronSharePoint.Framework.Hives
{
    /// <summary>
    /// Hive implementation for folders on a physical hard disk
    /// </summary>
    public class PhysicalHive : IHive
    {
        private string _root;

        public string Root { get { return _root; } }

        /// <summary>
        /// Creates a new Hive for the folder <paramref name="root"/>.
        /// </summary>
        /// <param name="root"></param>
        public PhysicalHive(string root)
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

            Console.WriteLine(fullPath);
            return Directory.Exists(fullPath);
        }

        public Stream OpenInputFileStream(string path)
        {
            var fullPath = GetFullPath(path);

            return File.OpenWrite(fullPath);
        }

        public Stream OpenOutputFileStream(string path)
        {
            var fullPath = GetFullPath(path);

            return File.OpenRead(fullPath);
        }

        public string GetFullPath(string path)
        {
            return Path.IsPathRooted(path) ? path : Path.Combine(Root, path);
        }
    }
}