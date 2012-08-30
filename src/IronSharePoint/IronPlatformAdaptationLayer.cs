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
        private readonly IronHost _host;
        private readonly Stack<String> _folderHistory;
        private Dictionary<String, String> _hiveFileDictionary;

        public Dictionary<String, String> HiveFileDictionary
        {
            get 
            {
                if (_hiveFileDictionary == null)
                {       
                    _hiveFileDictionary = new Dictionary<string, string>();

                    var query = new SPQuery();
                    query.Query = "<Where></Where>";
                    query.ViewFields = "<FieldRef Name='FileRef'/><FieldRef Name='FileLeafRef'/>";
                    query.ViewAttributes = "Scope='Recursive'";
                    query.IncludeMandatoryColumns = false;

                    var allItems = _host.HiveList.GetItems(query);

                    foreach (SPListItem item in allItems)
                    {
                        _hiveFileDictionary.Add(item["FileRef"].ToString().ToLower(), item["FileLeafRef"].ToString().ToLower());
                    }                    
                }
                
                return _hiveFileDictionary; 
            }
        }

        public IronPlatformAdaptationLayer(IronHost host)
        {
            _folderHistory = new Stack<string>();
            _folderHistory.Push(host.HiveFolder.ServerRelativeUrl);
            _host=host;
        }

        public override bool FileExists(string path)
        {
            bool fileExists = base.FileExists(path);

            if (!fileExists && path.StartsWith(IronConstants.IronHiveDefaultRoot))
            {
                path = path.ToLower();
                fileExists = HiveFileDictionary.Values.Any(name => name == Path.GetFileName(path));
            }
         
            return fileExists;
        }
       
        public override System.IO.Stream OpenOutputFileStream(string path)
        {
            if (path.StartsWith(IronConstants.IronHiveDefaultRoot))
            {
                var file = GetIronHiveFile(path);

                return file.OpenBinaryStream();
            }

            return base.OpenOutputFileStream(path);
        }
     

        public override string GetFullPath(string path)
        {
            if (path.StartsWith(IronConstants.IronHiveDefaultRoot))
            {
                return path.Replace(IronConstants.IronHiveDefaultRoot, _host.HiveFolder.ServerRelativeUrl);
            }
            return base.GetFullPath(path);
        }

        public override System.IO.Stream OpenInputFileStream(string path)
        {
            if (path.StartsWith(IronConstants.IronHiveDefaultRoot))
            {
                var file = GetIronHiveFile(path);

                return file.OpenBinaryStream();
            }

            return base.OpenInputFileStream(path);
        }

        public SPFile GetIronHiveFile(string path)
        {
            path = path.Replace(IronConstants.IronHiveDefaultRoot, _host.HiveFolder.ServerRelativeUrl + "/").ToLower().Replace("//","/");

            var fileName = Path.GetFileName(path);

            var matchingFilePaths = _hiveFileDictionary.Where(x => x.Value == fileName).Select(x => x.Key);

            if (matchingFilePaths.Count() == 1)
            {
                return _host.HiveWeb.GetFile(matchingFilePaths.First());
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

            var file = _host.HiveWeb.GetFile(matchingPath);
            _folderHistory.Push(file.ParentFolder.ServerRelativeUrl);
            
            return file;
        }

    }
}
