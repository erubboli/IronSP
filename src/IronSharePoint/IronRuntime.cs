using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using IronSharePoint.Administration;
using IronSharePoint.Diagnostics;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace IronSharePoint
{
    public class IronRuntime : IDisposable
    {
        // the key is the ID of the hive
        private static readonly Dictionary<Guid, IronRuntime> _staticLivingRuntimes =
            new Dictionary<Guid, IronRuntime>();

        private static readonly object _lock = new Object();

        private IronConsole.IronConsole _console;
        private Guid _hiveId;

        private IronRuntime(Guid hiveId)
        {
            _hiveId = hiveId;

            var setup = new ScriptRuntimeSetup();
            var languageSetup = new LanguageSetup(
                "IronRuby.Runtime.RubyContext, IronRuby, Version=1.0.0.1, Culture=neutral, PublicKeyToken=baeaf26a6e0611a7",
                IronConstant.IronRubyLanguageName,
                new[] {"IronRuby", "Ruby", "rb"},
                new[] {".rb"});
            setup.LanguageSetups.Add(languageSetup);

            setup.HostType = typeof (IronHive);
#if DEBUG
            setup.DebugMode = true;
#endif
            ScriptRuntime = new ScriptRuntime(setup);
            IronHive = (IronHive) ScriptRuntime.Host;
            IronHive.Id = _hiveId;
            DynamicTypeRegistry = new Dictionary<string, Object>();
            DynamicFunctionRegistry = new Dictionary<string, Object>();
            ScriptRuntime.LoadAssembly(typeof (IronRuntime).Assembly);
            ScriptRuntime.LoadAssembly(typeof (SPSite).Assembly);
            ScriptRuntime.LoadAssembly(typeof (IHttpHandler).Assembly);

            //load engines
            LivingEngines = new Dictionary<string, IronEngine>();
            //load Ruby Engine
            GetEngineByExtension(".rb");
        }

        internal static Dictionary<Guid, IronRuntime> LivingRuntimes
        {
            get { return _staticLivingRuntimes; }
        }

        internal Dictionary<String, IronEngine> LivingEngines { get; private set; }

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
        public string HttpHandlerClass { get; set; }
        public bool IsDisposed { get; private set; }
        public Guid Id { get { return _hiveId; } }

        #region IDisposable Members

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                if (IronHive != null)
                {
                    IronHive.Close();
                }
                if (_console != null)
                {
                    _console.Dispose();
                    _console = null;
                }
                LivingRuntimes.Remove(_hiveId);
            }
        }

        #endregion

        public static IronRuntime GetDefaultIronRuntime(SPSite targetSite)
        {
            Guid hiveId = IronHiveRegistry.Local.GetHiveForSite(targetSite.ID);
            if (hiveId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    String.Format("There is no IronHive mapping for the site with id {0}", targetSite.ID));
            }

            IronRuntime ironRuntime;

            lock (_lock)
            {
                if (!LivingRuntimes.TryGetValue(hiveId, out ironRuntime))
                {
                    ironRuntime = new IronRuntime(hiveId);
                    LivingRuntimes[hiveId] = ironRuntime;
                }
            }

            return ironRuntime;
        }

        //public static IronRuntime GetIronRuntime(SPSite targetSite, Guid hiveId)
        //{
        //    IronRuntime ironRuntime;

        //    if (hiveId == Guid.Empty)
        //    {
        //        ironRuntime = GetDefaultIronRuntime(targetSite);
        //    }
        //    else
        //    {
        //        if (!LivingRuntimes.TryGetValue(hiveId, out ironRuntime))
        //        {
        //            ironRuntime = CreateIronRuntime(hiveId);
        //        }
        //    }

        //    if (HttpContext.Current != null)
        //    {
        //        ironRuntime.IronHive.Init(hiveId);

        //        // flag for dispoal in the IronHttpModule
        //        HttpContext.Current.Items[
        //            IronHelper.GetPrefixedKey("IronRuntime_" + Guid.NewGuid().ToString()) + "_ToDispose"] = ironRuntime;
        //    }

        //    return ironRuntime;
        //}

        public IronEngine GetEngineByExtension(string extension)
        {
            IronEngine ironEngine = null;

            try
            {
                if (LivingEngines.ContainsKey(extension))
                {
                    return LivingEngines[extension];
                }

                ScriptEngine scriptEngine = ScriptRuntime.GetEngineByFileExtension(extension);

                // ruby
                if (scriptEngine.Setup.DisplayName == IronConstant.IronRubyLanguageName)
                {
                    string ironRubyRootFolder = Path.Combine(IronHive.FeatureFolderPath, "IronSP_IronRuby10\\");
                    var gemDirs = new[]
                                      {
                                          Path.Combine(ironRubyRootFolder, "lib/ironruby/gems/1.8").Replace("\\", "/"),
                                          //"IronHive://vendor/rubygems"
                                      };

                    SPSecurity.RunWithElevatedPrivileges(() =>
                    {
                        Environment.SetEnvironmentVariable("IRONRUBY_10_20",
                                                        ironRubyRootFolder);
                        Environment.SetEnvironmentVariable("GEM_PATH",
                                                        String.Join(";",
                                                                    gemDirs));
                        Environment.SetEnvironmentVariable("GEM_HOME",
                                                        gemDirs[0]);

                        scriptEngine.SetSearchPaths(new List<String>
                                                        {
                                                            Path.Combine(
                                                                ironRubyRootFolder,
                                                                @"Lib\IronRuby"),
                                                            Path.Combine(
                                                                ironRubyRootFolder,
                                                                @"Lib\ruby\site_ruby\1.8"),
                                                            Path.Combine(
                                                                ironRubyRootFolder,
                                                                @"Lib\ruby\1.8"),
#if DEBUG
IronDebug.IronDevHivePah                       
#else
                                                            IronConstant.
                                                                IronHiveRoot
#endif
                                                        });
                        ScriptScope scope = scriptEngine.CreateScope();
                        scope.SetVariable("iron_runtime", this);
                        scriptEngine.Execute("$RUNTIME = iron_runtime", scope);
                        scriptEngine.Execute(
                            @"
require 'rubygems'
require 'iron_sharepoint'
require 'iron_templates'

begin
    require 'application'
rescue Exception => ex
    IRON_DEFAULT_LOGGER.error ex
end"
                            );
                    });
                }

                ironEngine = new IronEngine(this, scriptEngine);

                LivingEngines.Add(extension, ironEngine);
            }
            catch (Exception ex)
            {
                LogError(String.Format("Error occured while getting engine for extension {0}", extension), ex);
                throw;
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

        public object CreateDynamicInstance(string className, params object[] args)
        {
            object obj = null;

            object dynamicType = DynamicTypeRegistry[className];

            if (args != null && args.Length > 0)
            {
                obj = ScriptRuntime.Operations.CreateInstance(dynamicType, args);
            }
            else
            {
                obj = ScriptRuntime.Operations.CreateInstance(dynamicType);
            }

            return obj;
        }

        public static void LogError(string msg, Exception ex)
        {
            IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Controls],
                                                    TraceSeverity.Unexpected,
                                                    String.Format("{0}. Error:{1}; Stack:{2}", msg, ex.Message,
                                                                  ex.StackTrace));
        }

        public static void LogVerbose(string msg)
        {
            IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Controls],
                                                    TraceSeverity.Verbose, String.Format("{0}.", msg));
        }
    }
}