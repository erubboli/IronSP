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
        private static AssetConfiguration _configuration;

        public Asset(SPListItem item, AssetRuntime runtime, AssetConfiguration configuration = null)
        {
            _item = item;
            _runtime = runtime;
            _configuration = configuration;
        }

        public string Name
        {
            get
            {
                string pathPart = string.Empty;
                int nameGroupIndex = 1;
                if (Configuration.Paths.Any())
                {
                    pathPart = String.Format("({0})/", Configuration.Paths.StringJoin("|"));
                    nameGroupIndex = 2;
                }
                var nameRegex = new Regex(String.Format(@"Style Library/{0}(.+\.\w+)\..+$", pathPart), RegexOptions.IgnoreCase);
                var match = nameRegex.Match(_item.Url);
                return match.Success ? match.Groups[nameGroupIndex].Value : string.Empty;
            }
        }

        public string FolderName
        {
            get
            {
                string pathPart = string.Empty;
                if (Configuration.Paths.Any())
                {
                    pathPart = String.Format("/({0})", Configuration.Paths.StringJoin("|"));
                }
                var nameRegex = new Regex(String.Format(@"Style Library{0}", pathPart), RegexOptions.IgnoreCase);
                var match = nameRegex.Match(_item.Url);
                return match.Success ? match.Value : string.Empty;
            }
        }

        public string Compile()
        {
            var compiled = _runtime.RubyEngine.Execute(string.Format("$ASSET_ENV['{0}']", Name));
            if (compiled == null)
            {
                throw new FileNotFoundException("Asset not found", Name);
            }

            return Convert.ToString(compiled.source);
        }

        public AssetConfiguration Configuration
        {
            get { return _configuration ?? AssetConfiguration.Local; }
        }
    }
}