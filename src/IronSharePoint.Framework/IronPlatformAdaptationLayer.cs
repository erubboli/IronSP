using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Scripting;
using System.IO;

namespace IronSharePoint
{
    public class IronPlatformAdaptationLayer : PlatformAdaptationLayer
    {
        private readonly IHive _hive;

        public IronPlatformAdaptationLayer(IHive hive)
        {
            _hive = hive;
        }
       
        public override string[] GetFileSystemEntries(string path, string searchPattern, bool includeFiles, bool includeDirectories)
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
            var result = entries.Distinct().Select(x => Regex.IsMatch(x, @"^\w:") ? x : "@@" + x).ToArray();
            return result;
        }

        public override bool DirectoryExists(string path)
        {
            return base.DirectoryExists(path) || _hive.DirectoryExists(path);
        }

        public override bool FileExists(string file)
        {
            return base.FileExists(file) || _hive.FileExists(file);
        }

        public override string GetFullPath(string file)
        {
            return base.FileExists(file) ? base.GetFullPath(file) : _hive.GetFullPath(file);
        }

        public override Stream OpenOutputFileStream(string path)
        {
            return base.FileExists(path) ? base.OpenInputFileStream(path) : _hive.OpenOutputFileStream(path);
        }

        public override Stream OpenInputFileStream(string path)
        {
            return base.FileExists(path) ? base.OpenInputFileStream(path) : _hive.OpenOutputFileStream(path);
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return base.FileExists(path) ? base.OpenInputFileStream(path, mode, access, share) : _hive.OpenOutputFileStream(path);
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            return base.FileExists(path) ? 
                base.OpenInputFileStream(path, mode, access, share, bufferSize) :
                _hive.OpenOutputFileStream(path);
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
