using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting;
using Microsoft.SharePoint;
using System.IO;

namespace IronSharePoint
{
    public class IronPlatformAdaptationLayer : PlatformAdaptationLayer
    {
        private IronHost _host;
        private string _currentDir;
        private Stack<String> _folderHistory;
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
            _currentDir = IronConstants.IronHiveRootSymbol;
        }

        public override bool FileExists(string path)
        {
            bool fileExists = base.FileExists(path);

            if (!fileExists && path.StartsWith(IronConstants.IronHiveRootSymbol))
            {
                fileExists = HiveFileDictionary.Values.Where(name => name == Path.GetFileName(path)).Any();
            }
         
            return fileExists;
        }

        public override bool DirectoryExists(string path)
        {
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
            if (path.StartsWith(IronConstants.IronHiveRootSymbol))
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
                return _currentDir;
            }
            set
            {
                _currentDir = value.Replace(IronConstants.IronHiveRootSymbol, _host.HiveFolder.ServerRelativeUrl);
                _folderHistory.Push(_currentDir);
                
            }
        }

        public override string GetFileName(string path)
        {
            return base.GetFileName(path);
        }


        public override string GetFullPath(string path)
        {
            if (path.StartsWith(IronConstants.IronHiveRootSymbol))
            {
                return path.Replace(IronConstants.IronHiveRootSymbol, _host.HiveFolder.ServerRelativeUrl);
            }
            return base.GetFullPath(path);
        }

        public override bool IsAbsolutePath(string path)
        {
            return base.IsAbsolutePath(path);
        }

        public override System.IO.Stream OpenInputFileStream(string path)
        {
            if (path.StartsWith(IronConstants.IronHiveRootSymbol))
            {
                var file = GetIronHiveFile(path);

                _folderHistory.Push(file.ParentFolder.ServerRelativeUrl);

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
            path = path.Replace(IronConstants.IronHiveRootSymbol, _host.HiveFolder.ServerRelativeUrl).ToLower();
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
                matchingPath = matchingFilePaths.Where(x => x == searchPath).FirstOrDefault();

                if (!String.IsNullOrEmpty(matchingPath))
                {
                    break;
                }
            }
            
            return _host.HiveWeb.GetFile(matchingPath);
        }

    }
}
