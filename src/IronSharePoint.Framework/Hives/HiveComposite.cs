using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace IronSharePoint.Hives
{
    /// <summary>
    /// Hive implementation which delegates its operations to an ordered list of leaf-hives.
    /// Operations that return a single object, e.g. OpenInputFileStream(), the composite delegates the call
    /// to the first leaf-hive that is able to handle the operation (in that case, the file exists in the hive).
    /// For operations that return a collection of objects, the result of all leaf-hives are combined.
    /// For operations that return a bool, the result of all leaf-hives are combined with an OR.
    /// </summary>
    public class HiveComposite : IHive, IEnumerable<IHive>
    {
        private readonly List<IHive> _hives; 

        public HiveComposite(params IHive[] hives)
        {
            _hives = new List<IHive>(hives.Where(x => x != null));
        }

        public void Append(IHive hive)
        {
            Contract.Requires<ArgumentNullException>(hive != null, "hive");

            _hives.Add(hive);
        }

        public void Prepend(IHive hive)
        {
            Contract.Requires<ArgumentNullException>(hive != null, "hive");

            _hives.Insert(0, hive);
        }

        public bool FileExists(string path)
        {
            return _hives.Any(x => x.FileExists(path));
        }

        public bool DirectoryExists(string path)
        {
            return _hives.Any(x => x.DirectoryExists(path));
        }

        public Stream OpenInputFileStream(string path)
        {
            var handler = FindHandler(path);
            if (handler == null) throw new FileNotFoundException("", path);

            return handler.OpenInputFileStream(path);
        }

        public Stream OpenOutputFileStream(string path)
        {
            var handler = FindHandler(path);
            if (handler == null) throw new FileNotFoundException("", path);

            return handler.OpenOutputFileStream(path);
        }

        public string GetFullPath(string path)
        {
            var handler = FindHandler(path);
            if (handler == null) return Path.GetFullPath(path);

            return handler.GetFullPath(path);
        }

        public bool IsAbsolutePath(string path)
        {
            return _hives.Any(x => x.IsAbsolutePath(path));
        }

        public string CombinePath(string path1, string path2)
        {
            Contract.Requires<ArgumentNullException>(path1 != null);
            Contract.Requires<ArgumentNullException>(path2 != null); 

            // TODO better way? Use hives but which to choose?
            var delim = path1.Contains("/") || path2.Contains("/") ? '/' : '\\';
            path1 = path1.TrimEnd(delim);
            path2 = path2.TrimStart(delim);

            return string.Format("{0}{1}{2}", path1, delim, path2);
        }

        public IEnumerable<string> GetFiles(string path, string searchPattern, bool absolutePaths = false)
        {
            return _hives.SelectMany(x => x.GetFiles(path, searchPattern, absolutePaths)).Distinct();
        }

        public IEnumerable<string> GetDirectories(string path, string searchPattern, bool absolutePaths = false)
        {
            return _hives.SelectMany(x => x.GetDirectories(path, searchPattern, absolutePaths)).Distinct();
        }

        IHive FindHandler(string path)
        {
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(path), "path cannot be null or blank");

            return _hives.FirstOrDefault(x => x.FileExists(path));
        }

        public IEnumerator<IHive> GetEnumerator()
        {
            return _hives.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            foreach (var hive in _hives)
            {
                hive.Dispose();
            }
        }
    }
}
