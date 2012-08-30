using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using System.IO;
using IronSharePoint.Diagnostics;
using Microsoft.SharePoint.Administration;
using IronSharePoint.Administration;
using System.Web;

namespace IronSharePoint
{
    public class IronRuntime: IDisposable
    {
        // the key is the ID of the hive
        private static readonly Dictionary<Guid, IronRuntime> _runningRuntimes = new Dictionary<Guid, IronRuntime>();

        private Dictionary<String, IronEngine> _runningEngines = new Dictionary<String, IronEngine>();
        internal Dictionary<String, IronEngine> RunningEngines
        {
            get { return _runningEngines; }
        }   

        private Guid _hiveSiteId;

        private List<Guid> _sitesUsingThisRuntime = new List<Guid>();
        internal List<Guid> SitesUsingThisRuntime
        {
            get { return _sitesUsingThisRuntime; }
        }

        public ScriptRuntime ScriptRuntime { get; private set; }
        public IronHost IronHost { get; private set; }
        public Dictionary<string, Object> DynamicTypeRegistry { get; private set; }
        public Dictionary<string, Object> DynamicFunctionRegistry { get; private set; }

        public static IronRuntime GetIronRuntime(Guid siteId)
        {
            IronRuntime ironRuntime = _runningRuntimes.Values.Where(r => r.SitesUsingThisRuntime.Contains(siteId)).FirstOrDefault();

            if (ironRuntime == null)
            {
                var hiveSiteId = IronHiveRegistry.Local.GetHiveBySiteId(siteId);
                if (hiveSiteId == Guid.Empty)
                {
                    throw new InvalidOperationException(String.Format("There is no IronHive mapping for the site with id {0}", siteId));
                }

                if (_runningRuntimes.ContainsKey(hiveSiteId))
                {
                    ironRuntime = _runningRuntimes[hiveSiteId];
                }
                else
                {
                    ironRuntime = new IronRuntime();
                    // create new runtime
                    var setup = new ScriptRuntimeSetup();
                    setup.LanguageSetups.Add(new LanguageSetup(
                           "IronRuby.Runtime.RubyContext, IronRuby, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", IronConstants.IronRubyLanguageName,
                                 new List<String>() { "IronRuby", "Ruby", "rb" }, new List<String>() { ".rb" }));

                    setup.HostType = typeof(IronHost);

#if DEBUG
                    setup.DebugMode = true;
#endif
                    ironRuntime.ScriptRuntime = new ScriptRuntime(setup);
                    
                    ironRuntime._hiveSiteId = hiveSiteId;
                    ironRuntime.IronHost = ironRuntime.ScriptRuntime.Host as IronHost;
                    ironRuntime.DynamicTypeRegistry = new Dictionary<string, Object>();
                    ironRuntime.ScriptRuntime.Globals.SetVariable("ironRuntime", ironRuntime);

                    _runningRuntimes.Add(hiveSiteId, ironRuntime);
                }
            }
      

            if (HttpContext.Current != null)
            {     
                ironRuntime.IronHost.SetHiveSite(ironRuntime._hiveSiteId);
            }

            if (!ironRuntime.SitesUsingThisRuntime.Contains(siteId))
            {
                ironRuntime.SitesUsingThisRuntime.Add(siteId);
            }

            return ironRuntime;
        }

        public IronEngine GetEngineByExtension(string extension)
        {
            IronEngine ironEngine = null;

            try
            {
                if (RunningEngines.ContainsKey(extension))
                {
                    return RunningEngines[extension];
                }

                var scriptEngine = ScriptRuntime.GetEngineByFileExtension(extension);

                // ruby
                if (scriptEngine.Setup.DisplayName == IronConstants.IronRubyLanguageName)
                {
                    var ironRubyRootFolder = Path.Combine(IronHost.FeatureFolderPath, "IronSP_IronRuby10\\");

                    SPSecurity.RunWithElevatedPrivileges(() =>
                    {
                        System.Environment.SetEnvironmentVariable("IRONRUBY_10_20", ironRubyRootFolder);

                        scriptEngine.SetSearchPaths(new List<String>() {
                                Path.Combine(ironRubyRootFolder, @"Lib\IronRuby"),
                                Path.Combine(ironRubyRootFolder, @"Lib\ruby\site_ruby\1.8"),
                                Path.Combine(ironRubyRootFolder, @"Lib\ruby\site_ruby"),
                                Path.Combine(ironRubyRootFolder, @"Lib\ruby\1.8"),
                                IronConstants.IronHiveRootSymbol
                        });
                    });
                }

                ironEngine = new IronEngine(this, scriptEngine);

                RunningEngines.Add(extension, ironEngine);

            }
            catch (Exception ex)
            {
                LogError(String.Format("Error occured while getting engine for extension {0}", extension), ex);
                throw ex;
            }

            return ironEngine;
        }

        public void RegisterDynamicType(string name, object type)
        {
            if (!DynamicTypeRegistry.ContainsKey(name))
            {
                DynamicTypeRegistry.Add(name, type);
            }
        }

        public void RegisterDynamicType(object type)
        {
            var name = type.ToString();
            RegisterDynamicType(name, type);
        }

        public void RegisterDynamicFunction(string name, object type)
        {
            if (!DynamicFunctionRegistry.ContainsKey(name))
            {
                DynamicTypeRegistry.Add(name, type);
            }
        }

        public void RegisterDynamicFunction(object type)
        {
            var name = type.ToString();
            DynamicFunctionRegistry.Add(name, type);
        }

        private IronRuntime()
        {

        }

        public static void LogError(string msg, Exception ex)
        {
            IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Controls], TraceSeverity.Unexpected, String.Format("{0}. Error:{1}; Stack:{2}", msg, ex.Message, ex.StackTrace));
        }

        public static void LogVerbose(string msg)
        {
            IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Controls], TraceSeverity.Verbose, String.Format("{0}.", msg));
        }


        public void Dispose()
        {
            if (IronHost != null)
            {
                IronHost.Dispose();
            }
        }
    }
}
