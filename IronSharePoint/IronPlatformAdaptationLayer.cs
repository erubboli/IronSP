using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting;
using Microsoft.SharePoint;

namespace IronSharePoint
{
    public class IronPlatformAdaptationLayer : PlatformAdaptationLayer
    {
        private IronHost _host;

        public IronPlatformAdaptationLayer(IronHost host)
        {
            _host=host;
        }

        public override bool FileExists(string path)
        {
            if (path.StartsWith(IronConstants.IronHivePath))
            {
                var file = GetIronHiveFile(path);

                return file.Exists;
            }
         
            return base.FileExists(path);
        }

        public override bool DirectoryExists(string path)
        {
            if (path.StartsWith(IronConstants.IronHivePath))
            {
                var file = GetIronHiveFile(path);

                return true;
            }

            return base.DirectoryExists(path);
        }

        public override string CombinePaths(string path1, string path2)
        {
            return base.CombinePaths(path1, path2);
        }

        public override string GetDirectoryName(string path)
        {
            return base.GetDirectoryName(path);
        }
       
        public override System.IO.Stream OpenOutputFileStream(string path)
        {
            if (path.StartsWith(IronConstants.IronHivePath))
            {
                var file = GetIronHiveFile(path);

                return file.OpenBinaryStream();
            }

            return base.OpenOutputFileStream(path);
        }

        public override string GetFileNameWithoutExtension(string path)
        {
            return base.GetFileNameWithoutExtension(path);
        }

        public override string GetExtension(string path)
        {
            return base.GetExtension(path);
        }

        public override string[] GetFileSystemEntries(string path, string searchPattern, bool includeFiles, bool includeDirectories)
        {
            return base.GetFileSystemEntries(path, searchPattern, includeFiles, includeDirectories);
        }

        public override string CurrentDirectory
        {
            get
            {
                return base.CurrentDirectory;
            }
            set
            {
                base.CurrentDirectory = value;
            }
        }

        public override string GetFileName(string path)
        {
            return base.GetFileName(path);
        }


        public override string GetFullPath(string path)
        {
            if (path.StartsWith(IronConstants.IronHivePath))
            {
                return path.Replace(IronConstants.IronHivePath, _host.HiveFolder.ServerRelativeUrl);
            }
            return base.GetFullPath(path);
        }

        public override bool IsAbsolutePath(string path)
        {
            return base.IsAbsolutePath(path);
        }

        public override System.IO.Stream OpenInputFileStream(string path)
        {
            if (path.StartsWith(IronConstants.IronHivePath))
            {
                var file = GetIronHiveFile(path);

                return file.OpenBinaryStream();
            }

            return base.OpenInputFileStream(path);
        }

        public override System.IO.Stream OpenInputFileStream(string path, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share)
        {
            return base.OpenInputFileStream(path, mode, access, share);
        }

        public override System.IO.Stream OpenInputFileStream(string path, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share, int bufferSize)
        {
            return base.OpenInputFileStream(path, mode, access, share, bufferSize);
        }

        private SPFile GetIronHiveFile(string path)
        {
            path = path.Replace(IronConstants.IronHivePath, _host.HiveFolder.ServerRelativeUrl);

            var file = _host.HiveWeb.GetFile(path);
            return file;
        }

    }
}
