using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronSharePoint.Hives
{
    public class SystemHive : IHive
    {
        public void Dispose()
        {
            // nothing to do
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public Stream OpenInputFileStream(string path)
        {
            return File.OpenRead(path);
        }

        public Stream OpenOutputFileStream(string path)
        {
            return File.OpenWrite(path);
        }

        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
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
            return new string[0];
        }

        public IEnumerable<string> GetDirectories(string path, string searchPattern, bool absolutePaths = false)
        {
            return new string[0];
        }

        public string Name { get; set; }

        public string Description { get { return string.Format("{0}: {1}", GetType().Name, Environment.MachineName); } }
    }
}
