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

        private ScriptRuntime _scriptRuntime;

        public ScriptRuntime ScriptRuntime
        {
            get { return _scriptRuntime; }
        }

        private IronHost _host;

        public IronHost Host
        {
            get { return _host; }
        }

        private Dictionary<String, IronEngine> _runningEngines = new Dictionary<String, IronEngine>();

        internal Dictionary<String, IronEngine> RunningEngines
        {
            get { return _runningEngines; }
            set { _runningEngines = value; }
        }

        private List<Guid> _sitesUsingThisRuntime = new List<Guid>();

        public List<Guid> SitesUsingThisRuntime
        {
            get { return _sitesUsingThisRuntime; }
        }

        private Guid _hiveSiteId;

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
                    ironRuntime._scriptRuntime = new ScriptRuntime(setup);
                    
                    ironRuntime._hiveSiteId = hiveSiteId;
                    ironRuntime._host = ironRuntime._scriptRuntime.Host as IronHost;

                    _runningRuntimes.Add(hiveSiteId, ironRuntime);
                }
            }
      

            if (HttpContext.Current != null)
            {     
                ironRuntime._host.SetHiveSite(ironRuntime._hiveSiteId);
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
                    var ironRubyRootFolder = Path.Combine(Host.FeatureFolderPath, "IronSP_IronRuby10\\");

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
            if (_host != null)
            {
                _host.Dispose();
            }
        }
    }
}
