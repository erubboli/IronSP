using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting;
using Microsoft.SharePoint;
using System.IO;
using System.Web;

namespace IronSharePoint
{
    public class IronPlatformAdaptationLayer : PlatformAdaptationLayer
    {
        private readonly IronHive _ironHive;
        private readonly Stack<String> _folderHistory;
       

        public IronPlatformAdaptationLayer(IronHive host)
        {
            _folderHistory = new Stack<string>();
            _folderHistory.Push(host.Folder.ServerRelativeUrl);
            _ironHive=host;
        }

        public override bool FileExists(string path)
        {
            bool fileExists = base.FileExists(path);

            if (!fileExists && path.StartsWith(IronConstant.IronHiveDefaultRoot))
            {
                path = path.ToLower();
                fileExists = _ironHive.HiveFileDictionary.Values.Any(name => name == Path.GetFileName(path));
            }
         
            return fileExists;
        }
       
        public override System.IO.Stream OpenOutputFileStream(string path)
        {
            if (path.StartsWith(IronConstant.IronHiveDefaultRoot))
            {
                var file = GetIronHiveFile(path);

                return file.OpenBinaryStream();
            }

            return base.OpenOutputFileStream(path);
        }
     

        public override string GetFullPath(string path)
        {
            if (path.StartsWith(IronConstant.IronHiveDefaultRoot))
            {
                return path.Replace(IronConstant.IronHiveDefaultRoot, _ironHive.Folder.ServerRelativeUrl);
            }
            return base.GetFullPath(path);
        }

        public override System.IO.Stream OpenInputFileStream(string path)
        {
            if (path.StartsWith(IronConstant.IronHiveDefaultRoot))
            {
                var file = GetIronHiveFile(path);

                return file.OpenBinaryStream();
            }

            return base.OpenInputFileStream(path);
        }

        public SPFile GetIronHiveFile(string path)
        {
            path = path.Replace(IronConstant.IronHiveDefaultRoot, _ironHive.Folder.ServerRelativeUrl + "/").ToLower().Replace("//","/");

            var fileName = Path.GetFileName(path);

            var matchingFilePaths = _ironHive.HiveFileDictionary.Where(x => x.Value == fileName).Select(x => x.Key);

            if (matchingFilePaths.Count() == 1)
            {
                return _ironHive.Web.GetFile(matchingFilePaths.First());
            }

            var matchingPath=String.Empty;

            foreach (var folder in _folderHistory)
            {
                var searchPath = (folder + "/" + fileName).Replace("//","/").ToLower();
                matchingPath = matchingFilePaths.FirstOrDefault(x => x == searchPath);

                if (!String.IsNullOrEmpty(matchingPath))
                {
                    break;
                }
            }

            var file = _ironHive.Web.GetFile(matchingPath);
            _folderHistory.Push(file.ParentFolder.ServerRelativeUrl);

            return file;
        }

    }
}
