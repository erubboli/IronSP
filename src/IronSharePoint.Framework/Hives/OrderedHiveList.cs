using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronSharePoint.Framework.Hives
{
    /// <summary>
    /// Hive implementation which operates an ordered list of hives. First come, first serve.
    /// </summary>
    public class OrderedHiveList : IHive, IEnumerable<IHive>
    {
        private List<IHive> _hives; 

        public OrderedHiveList(params IHive[] hives)
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
            if (handler == null) throw new FileNotFoundException("", path);

            return handler.GetFullPath(path);
        }

        public bool IsAbsolutePath(string path)
        {
            throw new NotImplementedException();
        }

        public string CombinePath(string path1, string path2)
        {
            throw new NotImplementedException();
        }

        public string[] GetFiles(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetDirectories(string path, string searchPattern)
        {
            throw new NotImplementedException();
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
    }
}
