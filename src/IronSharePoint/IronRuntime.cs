using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
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
        static readonly Dictionary<Guid, IronRuntime> _staticLivingRuntimes;

        static IronRuntime()
        {
            _staticLivingRuntimes = new Dictionary<Guid, IronRuntime>();
        }

        IronConsole.IronConsole _console;
        private Guid _hiveId;

        internal static Dictionary<Guid, IronRuntime> LivingRuntimes
        {
            get {

                if (HttpContext.Current != null)
                {
                    var key = IronHelper.GetPrefixedKey("LivingRuntimes");
                    var inHttpAppLivingRuntimes = HttpContext.Current.Application[key];
                    if (inHttpAppLivingRuntimes == null)
                    {
                        inHttpAppLivingRuntimes = new Dictionary<Guid, IronRuntime>();
                        HttpContext.Current.Application[key] = inHttpAppLivingRuntimes;
                    }

                    return inHttpAppLivingRuntimes as Dictionary<Guid, IronRuntime>;
                }

                return _staticLivingRuntimes; 
            }
        }

        internal Dictionary<String, IronEngine> LivingEngines { get; private set; }
        internal List<Guid> DefaultSiteIds { get; private set; }

        public IronConsole.IronConsole IronConsole
        {
            get
            {
                return _console ??
                       (_console = IronSharePoint.IronConsole.IronConsole.GetConsoleForRuntime(this));
            }
        }

        public ScriptRuntime ScriptRuntime { get; private set; }
        public IronHive IronHive { get; private set; }
        public Dictionary<string, Object> DynamicTypeRegistry { get; private set; }
        public Dictionary<string, Object> DynamicFunctionRegistry { get; private set; }

        private IronRuntime()
        {
            LivingEngines = new Dictionary<string, IronEngine>();
            DefaultSiteIds = new List<Guid>();
        }

        public bool IsDefaultRuntime(SPSite site)
        {
            return DefaultSiteIds.Contains(site.ID);
        }

        public static IronRuntime GetDefaultIronRuntime(SPSite targetSite)
        {
            IronRuntime ironRuntime = LivingRuntimes.Values
                .Where(runtime => runtime != null && !runtime.IsDisposed) 
                .FirstOrDefault(runtime => runtime.IsDefaultRuntime(targetSite));
 
            if (ironRuntime == null)
            {
                var hiveId = IronHiveRegistry.Local.GetHiveForSite(targetSite.ID);
                if (hiveId == Guid.Empty)
                {
                    throw new InvalidOperationException(String.Format("There is no IronHive mapping for the site with id {0}", targetSite.ID));
                }

                ironRuntime = GetIronRuntime(targetSite, hiveId);

                if (!ironRuntime.DefaultSiteIds.Contains(targetSite.ID))
                {
                    ironRuntime.DefaultSiteIds.Add(targetSite.ID);
                }
            }
      
            return ironRuntime;
        }

        public static IronRuntime GetIronRuntime(SPSite targetSite, Guid hiveId)
        {
            IronRuntime ironRuntime = null;

            if (hiveId == Guid.Empty)
            {
                ironRuntime = GetDefaultIronRuntime(targetSite);
            }
            else
            {
                ironRuntime = LivingRuntimes.Values
                    .Where(runtime => runtime != null && !runtime.IsDisposed)
                    .FirstOrDefault(runtime => runtime._hiveId == hiveId)
                    ?? CreateIronRuntime(hiveId);
            }

            if (HttpContext.Current != null)
            {
                ironRuntime.IronHive.Init(ironRuntime._hiveId);

                // flag for dispoal in the IronHttpModule
                HttpContext.Current.Items[IronHelper.GetPrefixedKey("IronRuntime_" + Guid.NewGuid().ToString()) + "_ToDispose"] = ironRuntime;
            }

            return ironRuntime;
        }

        private static IronRuntime CreateIronRuntime(Guid hiveSiteId)
        {
            IronHiveRegistry.Local.EnsureTrustedHive(hiveSiteId);

            var ironRuntime = new IronRuntime();

            if (LivingRuntimes.ContainsKey(hiveSiteId))
            {
                ironRuntime = LivingRuntimes[hiveSiteId];
            }
            else
            {
                ironRuntime = new IronRuntime();
                // create new runtime
                var setup = new ScriptRuntimeSetup();
                var languageSetup = new LanguageSetup(
                        "IronRuby.Runtime.RubyContext, IronRuby, Version=1.0.0.1, Culture=neutral, PublicKeyToken=baeaf26a6e0611a7",
                        IronConstant.IronRubyLanguageName,
                        new[] {"IronRuby", "Ruby", "rb"},
                        new[] {".rb"});
                setup.LanguageSetups.Add(languageSetup);

                setup.HostType = typeof(IronHive);
#if DEBUG
                setup.DebugMode = true;
#endif
                ironRuntime.ScriptRuntime = new ScriptRuntime(setup);

                ironRuntime._hiveId = hiveSiteId;
                ironRuntime.IronHive = (IronHive) ironRuntime.ScriptRuntime.Host;
                ironRuntime.DynamicTypeRegistry = new Dictionary<string, Object>();
                ironRuntime.DynamicFunctionRegistry = new Dictionary<string, Object>();
                ironRuntime.ScriptRuntime.Globals.SetVariable("ironRuntime", ironRuntime);
                ironRuntime.ScriptRuntime.LoadAssembly(typeof(IronRuntime).Assembly);
                ironRuntime.ScriptRuntime.LoadAssembly(typeof(SPSite).Assembly);
                ironRuntime.IronHive.Init(ironRuntime._hiveId);

                LivingRuntimes.Add(hiveSiteId, ironRuntime);

            }
            return ironRuntime;
        }

        public IronEngine GetEngineByExtension(string extension)
        {
            IronEngine ironEngine = null;

            try
            {
                if (LivingEngines.ContainsKey(extension))
                {
                    return LivingEngines[extension];
                }

                var scriptEngine = ScriptRuntime.GetEngineByFileExtension(extension);

                // ruby
                if (scriptEngine.Setup.DisplayName == IronConstant.IronRubyLanguageName)
                {
                    var ironRubyRootFolder = Path.Combine(IronHive.FeatureFolderPath, "IronSP_IronRuby10\\");
                    var gemDirs = new[]
                                      {
                                          Path.Combine(ironRubyRootFolder, "lib/ironruby/gems/1.8").Replace("\\", "/"),
                                          "IronHive://vendor/rubygems"
                                      };

                    SPSecurity.RunWithElevatedPrivileges(() =>
                    {
                        System.Environment.SetEnvironmentVariable("IRONRUBY_10_20", ironRubyRootFolder);
                        System.Environment.SetEnvironmentVariable("GEM_PATH", String.Join(";", gemDirs));
                        System.Environment.SetEnvironmentVariable("GEM_HOME", gemDirs[0]);

                        scriptEngine.SetSearchPaths(new List<String>() {
                                Path.Combine(ironRubyRootFolder, @"Lib\IronRuby"),
                                Path.Combine(ironRubyRootFolder, @"Lib\ruby\site_ruby\1.8"),
                                Path.Combine(ironRubyRootFolder, @"Lib\ruby\site_ruby"),
                                Path.Combine(ironRubyRootFolder, @"Lib\ruby\1.8"),
                                IronConstant.IronHiveRoot
                        });
                        var scope = scriptEngine.CreateScope();
                        scope.SetVariable("iron_runtime", this);
                        scriptEngine.Execute("$RUNTIME = iron_runtime", scope);
                        scriptEngine.Execute("require 'rubygems'");
                        scriptEngine.Execute("begin; require 'application'; rescue LoadError => ex; end");
                    });
                }

                ironEngine = new IronEngine(this, scriptEngine);

                LivingEngines.Add(extension, ironEngine);

            }
            catch (Exception ex)
            {
                LogError(String.Format("Error occured while getting engine for extension {0}", extension), ex);
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


        public void RegisterDynamicFunction(string name, object type)
        {
            DynamicFunctionRegistry[name] = type;        
        }


        public void Reset()
        {
            LivingRuntimes.Remove(this._hiveId);
            _console.Dispose();
            _console = null;
            this.Dispose();
        }

        public void Dispose()
        {
            IsDisposed = true;
            if (IronHive != null)
            {
                IronHive.Close();
            }
        }

        protected bool IsDisposed { get; private set; }

        public static void LogError(string msg, Exception ex)
        {
            IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Controls], TraceSeverity.Unexpected, String.Format("{0}. Error:{1}; Stack:{2}", msg, ex.Message, ex.StackTrace));
        }

        public static void LogVerbose(string msg)
        {
            IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Controls], TraceSeverity.Verbose, String.Format("{0}.", msg));
        }

    }
}
