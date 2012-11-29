using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Scripting;
using Microsoft.SharePoint;
using System.IO;
using System.Web;
using Microsoft.SharePoint.Utilities;

namespace IronSharePoint
{
    public class IronPlatformAdaptationLayer : PlatformAdaptationLayer
    {
        private readonly IronHive _ironHive;

        public IronPlatformAdaptationLayer(IronHive host)
        {
            _ironHive=host;
        }
       
        public override string[] GetFileSystemEntries(string path, string searchPattern, bool includeFiles, bool includeDirectories)
        {
            using (new SPMonitoredScope(string.Format("AdaptionLayer Query - {0}", path)))
            {
                var entries = base.GetFileSystemEntries(path, searchPattern, includeFiles, includeDirectories);
                /*
                 * HACK: if you just return 'app/foo/bar.rb', IronRuby removes the first two characters for whatever reason. Therefore, appending to random chars fixes this
                 */
                return entries.Select(x => Regex.IsMatch(x, @"^\w:") ? x : "@@" + x).ToArray();
            }

        }
        public override string[] GetFiles(string path, string searchPattern)
        {
            string[] files;
            try
            {
                files =  base.GetFiles(path, searchPattern);
            }
            catch (Exception)
            {
                files = _ironHive.GetFiles(path, searchPattern);
            }
            return files;
        }

        public override string[] GetDirectories(string path, string searchPattern)
        {
            string[] dirs;
            try
            {
               dirs = base.GetDirectories(path, searchPattern);
            }
            catch (Exception)
            {
                dirs = _ironHive.GetDirectories(path, searchPattern);
            }
            return dirs;
        }

        public override bool DirectoryExists(string path)
        {
            return base.DirectoryExists(path) || _ironHive.ContainsDirectory(path);
        }

        public override bool FileExists(string file)
        {
            bool fileExists = !file.StartsWith(IronConstant.IronHiveRoot) && base.FileExists(file);

            if (!fileExists)
            {
                fileExists = _ironHive.ContainsFile(file);
            }
         
            return fileExists;
        }

        public override System.IO.Stream OpenOutputFileStream(string path)
        {
            Stream fileStream = null;
            if (!path.StartsWith(IronConstant.IronHiveRoot) && base.FileExists(path))
            {
                fileStream = base.OpenOutputFileStream(path);
            }
            else
            {
                var spFile = _ironHive.LoadFile(path);

                if (spFile != null)
                {
                    fileStream = spFile.OpenBinaryStream();
                }
            }

            return fileStream;
        }

        public override string GetFullPath(string file)
        {
            if (_ironHive.ContainsFile(file))
            {
                return _ironHive.GetFullPath(file);
            }
            return base.GetFullPath(file);
        }

        public override Stream OpenInputFileStream(string path)
        {
            Stream fileStream = null;
            if (!path.StartsWith(IronConstant.IronHiveRoot) && base.FileExists(path))
            {
                fileStream = base.OpenInputFileStream(path);
            }
            else
            {
                var spFile = _ironHive.LoadFile(path);

                if (spFile != null)
                {
                    fileStream = spFile.OpenBinaryStream();
                }
            }

            return fileStream;
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            Stream fileStream = null;
            if (!path.StartsWith(IronConstant.IronHiveRoot) && base.FileExists(path))
            {
                fileStream = base.OpenInputFileStream(path, mode, access, share);
            }
            else
            {
                var spFile = _ironHive.LoadFile(path);

                if (spFile != null)
                {
                    fileStream = spFile.OpenBinaryStream();
                }
            }

            return fileStream;
        }
    }
}
