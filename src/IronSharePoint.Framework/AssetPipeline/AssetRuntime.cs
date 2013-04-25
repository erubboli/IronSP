using System;
using System.Collections.Generic;
using System.Text;
using IronSharePoint.Administration;
using IronSharePoint.Hives;
using Microsoft.Scripting.Hosting;

namespace IronSharePoint.AssetPipeline
{
    public class AssetRuntime : IronRuntimeBase
    {
        public AssetRuntime(Guid siteId)
        {
            SiteId = siteId;
        }

        public Guid SiteId { get; private set; }

        public override string Name
        {
            get { return "Asset Pipeline Runtime"; }
        }

        public override IronEnvironment Environment
        {
            get { return IronEnvironment.Production; }
        }

        protected override IEnumerable<HiveSetup> GetHiveSetups()
        {
            yield return new HiveSetup
                {
                    DisplayName = "Style Library",
                    HiveArguments = new object[] {SiteId, "Style%20Library"},
                    HiveType = typeof (SPDocumentHive),
                    Priority = 1
                };
        }

        protected override IEnumerable<string> GetGemPaths()
        {
            yield return IronConstant.GemsDirectory;
        }

        protected override void InitializeRubyEngine(ScriptEngine scriptEngine)
        {
            base.InitializeRubyEngine(scriptEngine);
            StringBuilder script = new StringBuilder()
                .AppendLine("require 'sprockets'")
                .AppendLine("require 'iron_sharepoint/patches/sprockets'")
                .AppendFormat("$ASSET_ENV = Sprockets::Environment.new '{0}'",
                              IronConstant.HiveWorkingDirectory.Replace('\\', '/')).AppendLine()
                .AppendLine("$ASSET_ENV.append_path '.'");

            scriptEngine.Execute(script.ToString());
        }
    }
}