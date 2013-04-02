using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IronSharePoint.Util;
using Microsoft.SharePoint;

namespace IronSharePoint.AssetPipeline
{
    public class Asset
    {
        private readonly SPListItem _item;
        private readonly AssetRuntime _runtime;

        public Asset(SPListItem item, AssetRuntime runtime)
        {
            _item = item;
            _runtime = runtime;
        }

        public string SourceName
        {
            get { return Path.GetFileName(_item.Url); }
        }

        public string FolderName
        {
            get { return Path.GetDirectoryName(_item.Url).Replace("\\", "/"); }
        }

        public string Name
        {
            get { return SourceName.Split('.').Take(2).StringJoin("."); }
        }

        public string Compile()
        {
            var compiled = _runtime.RubyEngine.Execute(string.Format("$ASSET_ENV['{0}/{1}']", FolderName, Name));
            if (compiled == null)
            {
                throw new FileNotFoundException("Asset not found", Name);
            }

            return Convert.ToString(compiled.source);
        }

        public SPFile Save(SPWeb web)
        {
            SPFolder folder = web.GetFolder(FolderName);
            byte[] bytes = Encoding.UTF8.GetBytes(Compile());
            return folder.Files.Add(string.Format("{0}/{1}", FolderName, Name), bytes, true);
        }
    }
}