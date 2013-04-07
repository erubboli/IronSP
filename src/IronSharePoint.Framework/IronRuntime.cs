using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using IronSharePoint.Administration;
using IronSharePoint.Diagnostics;
using IronSharePoint.Exceptions;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace IronSharePoint
{
    public class IronRuntime : IronRuntimeBase
    {
        private static readonly object Lock = new Object();
        private readonly string _name;
        private readonly HiveSetup[] _hiveSetups;
        private readonly string[] _gemPaths;

        static IronRuntime()
        {
            LivingRuntimes = new List<IronRuntime>();
        }

        public IronRuntime(RuntimeSetup runtimeSetup)
            : base(runtimeSetup.Id)
        {
            _name = runtimeSetup.DisplayName;
            _hiveSetups = runtimeSetup.Hives.ToArray();
            _gemPaths = runtimeSetup.GemPaths.ToArray();
        }

        internal static List<IronRuntime> LivingRuntimes { get; private set; }

        public static IronRuntime GetDefaultIronRuntime(SPSite targetSite)
        {
            using (new SPMonitoredScope("Retrieving IronRuntime"))
            {
                Guid targetId = targetSite.ID;
                IronRuntime runtime;
                if (!TryGetLivingRuntime(targetSite, out runtime))
                {
                    using (new SPMonitoredScope("Creating IronRuntime"))
                    {
                        try
                        {
                            var runtimeSetup = IronRegistry.Local.ResolveRuntime(targetSite);
                            runtime = new IronRuntime(runtimeSetup);
                            LivingRuntimes.Add(runtime);
                            runtime.Initialize();
                        }
                        catch (Exception ex)
                        {
                            var message = string.Format("Could not create IronRuntime for SPSite '{0}'", targetId);
                            IronULSLogger.Local.Error(message, ex,IronCategoryDiagnosticsId.Core);
                            throw new IronRuntimeAccesssException(message, ex) {SiteId = targetId};
                        }
                    }
                }
                if (HttpContext.Current != null) HttpContext.Current.Items[IronConstant.IronRuntimeKey] = runtime;

                return runtime;
            }
        }

        private static bool TryGetLivingRuntime(SPSite site, out IronRuntime runtime)
        {
            runtime = null;
            RuntimeSetup runtimeSetup;
            if (!IronRegistry.Local.TryResolveRuntime(site, out runtimeSetup)) return false;

            var runtimeId = runtimeSetup.Id;
            if ((runtime = LivingRuntimes.SingleOrDefault(x => x.Id == runtimeId)) == null)
            {
                lock (Lock)
                {
                    if ((runtime = LivingRuntimes.SingleOrDefault(x => x.Id == runtimeId)) == null)
                    {
                        if (HttpContext.Current != null)
                        {
                            runtime = HttpContext.Current.Items[IronConstant.IronRuntimeKey] as IronRuntime;
                        }
                    }
                }
            }

            return runtime != null;
        }

        public override string Name
        {
            get { return _name; }
        }

        public override void Dispose()
        {
            base.Dispose();
            LivingRuntimes.Remove(this);
        }

        protected override IEnumerable<HiveSetup> GetHiveSetups()
        {
            return _hiveSetups;
        }

        protected override IEnumerable<string> GetGemPaths()
        {
            return _gemPaths;
        }

        protected override void InitializeScriptEngine(ScriptEngine scriptEngine)
        {
            base.InitializeScriptEngine(scriptEngine);
            var script = new StringBuilder()
                .AppendLine("require 'iron_sharepoint'")
                .AppendLine("require 'application' if File.exists? 'application.rb'");

            scriptEngine.Execute(script.ToString());
        }
    }
}