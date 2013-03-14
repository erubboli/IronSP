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
            var entries = new List<string>();
            if (includeFiles)
            {
                //entries.AddRange(Directory.GetFiles(path, searchPattern));
                entries.AddRange(_hive.GetFiles(path, searchPattern));
            }
            if (includeDirectories)
            {
                //entries.AddRange(Directory.GetDirectories(path, searchPattern));
                entries.AddRange(_hive.GetDirectories(path, searchPattern));
            }
            string[] result = entries.Distinct().Select(x => Regex.IsMatch(x, @"(^\w:|^\./)") ? x : "./" + x).ToArray();
            return result;
        }

        public override bool DirectoryExists(string path)
        {
            return DelegateToHiveOrBase(path,
                                        _hive.DirectoryExists,
                                        x => base.DirectoryExists(x) || _hive.DirectoryExists(x));
        }

        public override bool FileExists(string file)
        {
            return DelegateToHiveOrBase(file,
                                        _hive.FileExists,
                                        x => base.FileExists(x) || _hive.FileExists(x));
        }

        public override string GetFullPath(string file)
        {
            return DelegateToHiveOrBase(file,
                                        _hive.GetFullPath,
                                        x => base.FileExists(file) ? base.GetFullPath(file) : _hive.GetFullPath(file));
        }

        public override Stream OpenOutputFileStream(string path)
        {
            return DelegateToHiveOrBase(path,
                                        _hive.OpenOutputFileStream,
                                        x =>
                                        base.FileExists(x) ? base.OpenOutputFileStream(x) : _hive.OpenOutputFileStream(x));
        }

        public override Stream OpenInputFileStream(string path)
        {
            return DelegateToHiveOrBase(path,
                                        _hive.OpenInputFileStream,
                                        x =>
                                        base.FileExists(x) ? base.OpenInputFileStream(x) : _hive.OpenOutputFileStream(x));
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return DelegateToHiveOrBase(path,
                                        _hive.OpenInputFileStream,
                                        x =>
                                        base.FileExists(x)
                                            ? base.OpenInputFileStream(x, mode, access, share)
                                            : _hive.OpenOutputFileStream(x));
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share,
                                                   int bufferSize)
        {
            return DelegateToHiveOrBase(path,
                                        _hive.OpenInputFileStream,
                                        x =>
                                        base.FileExists(x)
                                            ? base.OpenInputFileStream(x, mode, access, share, bufferSize)
                                            : _hive.OpenOutputFileStream(x));
        }

        private T DelegateToHiveOrBase<T>(string path, Func<string, T> hive, Func<string, T> @base)
        {
            T result;
            if (path.StartsWith(IronConstant.FakeHiveDirectory))
            {
                result = hive(path.ReplaceFirst(IronConstant.FakeHiveDirectory, string.Empty));
            }
            else if (path.StartsWith(IronConstant.FakeHiveDirectory.Replace('\\', '/')))
            {
                result = hive(path.ReplaceFirst(IronConstant.FakeHiveDirectory.Replace('\\', '/'), string.Empty));
            } 
            else
            {
                result = @base(path);
            }
            return result;
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