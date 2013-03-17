using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using IronSharePoint.Util;
using Microsoft.Scripting;

namespace IronSharePoint
{
    public class IronPlatformAdaptationLayer : PlatformAdaptationLayer
    {
        private readonly IHive _hive;

        public IronPlatformAdaptationLayer(IHive hive)
        {
            _hive = hive;
        }

        public override string[] GetFileSystemEntries(string path, string searchPattern, bool includeFiles,
                                                      bool includeDirectories)
        {
            path = TrimPath(path);

            var entries = new List<string>();
            if (includeFiles)
            {
                entries.AddRange(_hive.GetFiles(path, searchPattern));
            }
            if (includeDirectories)
            {
                entries.AddRange(_hive.GetDirectories(path, searchPattern));
            }
            string[] result = entries.Distinct().Select(x => Regex.IsMatch(x, @"(^\w:|^\./)") ? x : "./" + x).ToArray();
            return result;
        }

        public override bool DirectoryExists(string path)
        {
            path = TrimPath(path);

            return _hive.DirectoryExists(path);
        }

        public override bool FileExists(string file)
        {
            file = TrimPath(file);

            return _hive.FileExists(file);
        }

        public override string GetFullPath(string file)
        {
            file = TrimPath(file);

            return _hive.GetFullPath(file);
        }

        public override Stream OpenOutputFileStream(string path)
        {
            path = TrimPath(path);

            return _hive.OpenOutputFileStream(path);
        }

        public override Stream OpenInputFileStream(string path)
        {
            path = TrimPath(path);

            return _hive.OpenInputFileStream(path);
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return OpenInputFileStream(path);
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share,
                                                   int bufferSize)
        {
            return OpenInputFileStream(path);
        }

        private string TrimPath(string path)
        {
            var invalidPrefixes = new[] {".\\", IronConstant.FakeHiveDirectory};
            foreach (string prefix in invalidPrefixes)
            {
                path = path.ReplaceStart(prefix, string.Empty);
                path = path.ReplaceStart(prefix.Replace('\\', '/'), string.Empty);
            }

            return path;
        }

        private T TrimPath<T>(string path, Func<string, T> func)
        {
            return func(TrimPath(path));
        }

        public override void CreateDirectory(string path)
        {
            throw new NotSupportedException();
        }

        public override void DeleteDirectory(string path, bool recursive)
        {
            throw new NotSupportedException();
        }

        public override void DeleteFile(string path, bool deleteReadOnly)
        {
            throw new NotSupportedException();
        }

        public override void MoveFileSystemEntry(string sourcePath, string destinationPath)
        {
            throw new NotSupportedException();
        }
    }
}