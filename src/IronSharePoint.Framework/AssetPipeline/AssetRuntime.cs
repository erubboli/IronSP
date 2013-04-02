using System;
using System.Text;
using System.Threading.Tasks;

namespace IronSharePoint.AssetPipeline
{
    public class AssetRuntime : IronRuntime<AssetScriptHost>
    {
        public AssetRuntime(Guid siteId)
            : base(siteId, new object[]{siteId}) {}

        protected override void Initialize()
        {
            base.Initialize();
            var script = new StringBuilder()
                .AppendLine("require 'iron_sharepoint'")
                .AppendLine("require 'sprockets'")
                .AppendFormat("$ASSET_ENV = Sprockets::Environment.new '{0}'",
                              IronConstant.HiveWorkingDirectory.Replace('\\', '/')).AppendLine()
                .AppendLine("$ASSET_ENV.append_path '.'");
            
            RubyEngine.Execute(script.ToString());
        }
    }
}