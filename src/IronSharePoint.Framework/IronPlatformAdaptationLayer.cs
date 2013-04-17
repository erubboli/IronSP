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
            return _hive.DirectoryExists(TrimPath(path)) || Directory.Exists(path);
        }

        public override bool FileExists(string file)
        {
            return _hive.FileExists(TrimPath(file)) || File.Exists(file);
        }

        public override string GetFullPath(string file)
        {
            file = TrimPath(file);
            var fullPath = _hive.GetFullPath(file);
            if (fullPath == null)
            {
                if (!Path.IsPathRooted(file))
                {
                    file = file.ReplaceStart("./", string.Empty);
                    fullPath = Path.Combine(IronConstant.HiveWorkingDirectory, file);
                }
                else
                {
                    fullPath = Path.GetFullPath(file);
                }
            }
            return fullPath;
        }

        public override Stream OpenOutputFileStream(string path)
        {
            Stream stream;
            string trimmed = TrimPath(path);
            try
            {
                stream = _hive.OpenInputFileStream(trimmed);
            }
            catch (FileNotFoundException)
            {
                stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            }
            return stream;
        }

        public override Stream OpenInputFileStream(string path)
        {
            return OpenInputFileStream(path, FileMode.Open, FileAccess.Read,FileShare.None);
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            Stream stream;
            string trimmed = TrimPath(path);
            try
            {
                stream = _hive.OpenInputFileStream(trimmed);
            }
            catch (FileNotFoundException)
            {
                stream = new FileStream(path, mode, access);
            }
            return stream;
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share,
                                                   int bufferSize)
        {
            return OpenInputFileStream(path, mode, access, share);
        }

        public string TrimPath(string path)
        {
            var invalidPrefixes = new[] {".\\", IronConstant.HiveWorkingDirectory, Directory.GetCurrentDirectory()};
            foreach (string prefix in invalidPrefixes)
            {
                path = path.ReplaceStart(prefix, string.Empty);
                path = path.ReplaceStart(prefix.Replace('\\', '/'), string.Empty);
            }

            return path.TrimStart('/', '\\');
        }

        public T TrimPath<T>(string path, Func<string, T> func)
        {
            return func(TrimPath(path));
        }
    }
}