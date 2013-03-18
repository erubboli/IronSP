using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using IronSharePoint.Diagnostics;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using System.Linq;

namespace IronSharePoint
{
    public class IronRuntime : IDisposable
    {
        // SiteId -> Runtime
        private static readonly Dictionary<Guid, IronRuntime> _staticLivingRuntimes =
            new Dictionary<Guid, IronRuntime>();

        private static readonly object _sync = new Object();

        private readonly Guid _siteId;
        private readonly Guid _id;
        private IronConsole.IronConsole _console;
        private ScriptRuntime _scriptRuntime;

        private IronRuntime(Guid siteId)
        {
            _siteId = siteId;
            _id = Guid.NewGuid();

            DynamicTypeRegistry = new Dictionary<string, Object>();
            DynamicFunctionRegistry = new Dictionary<string, Object>();
            Engines = new Dictionary<string, IronEngine>();

            Initialize();
        }

        internal static Dictionary<Guid, IronRuntime> LivingRuntimes
        {
            get { return _staticLivingRuntimes; }
        }

        internal Dictionary<String, IronEngine> Engines { get; private set; }

        public IronConsole.IronConsole IronConsole
        {
            get
            {
                return _console ??
                       (_console = IronSharePoint.IronConsole.IronConsole.GetConsoleForRuntime(this));
            }
        }

        public ScriptRuntime ScriptRuntime
        {
            get { return _scriptRuntime; }
        }

        public IronScriptHost ScripHost
        {
            get { return (IronScriptHost) ScriptRuntime.Host; }
        }

        public IHive Hive
        {
            get { return ScripHost.Hive; }
        }

        public Dictionary<string, Object> DynamicTypeRegistry { get; private set; }
        public Dictionary<string, Object> DynamicFunctionRegistry { get; private set; }

        public string HttpHandlerClass { get; set; }
        public bool IsDisposed { get; private set; }

        public Guid SiteId
        {
            get { return _siteId; }
        }

        public Guid Id
        {
            get { return _id; }
        }

        public SPSite Site
        {
            get
            {
                var key = IronConstant.GetPrefixed("Site_" + Id);
                var httpContext = HttpContext.Current;
                SPSite site;

                if (httpContext != null)
                {
                    if (!httpContext.Items.Contains(key))
                    {
                        httpContext.Items[key] = new SPSite(Id, SPUserToken.SystemAccount);
                    }
                    site = httpContext.Items[key] as SPSite;
                }
                else
                {
                    site = new SPSite(Id, SPUserToken.SystemAccount);
                }

                return site;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                if (_console != null)
                {
                    _console.Dispose();
                    _console = null;
                }
                ScripHost.Dispose();

                LivingRuntimes.Remove(_siteId);
            }
        }

        #endregion

        private void Initialize()
        {
            var setup = new ScriptRuntimeSetup();
            var languageSetup = new LanguageSetup(
                "IronRuby.Runtime.RubyContext, IronRuby, Version=1.1.3.0, Culture=neutral, PublicKeyToken=7f709c5b713576e1",
                IronConstant.IronRubyLanguageName,
                new[] {"IronRuby", "Ruby", "rb"},
                new[] {".rb"});
            setup.LanguageSetups.Add(languageSetup);
            setup.HostType = typeof (IronScriptHost);
            setup.HostArguments = new object[] {_siteId};
            setup.DebugMode = IronConstant.IronEnv == IronEnvironment.Debug;

            _scriptRuntime = new ScriptRuntime(setup);

            _scriptRuntime.LoadAssembly(typeof (IronRuntime).Assembly); // IronSharePoint
            _scriptRuntime.LoadAssembly(typeof (SPSite).Assembly); // Microsoft.SharePoint
            _scriptRuntime.LoadAssembly(typeof (IHttpHandler).Assembly); // System.Web

            using (new SPMonitoredScope("Creating IronEngine(s)"))
            {
                SPSecurity.RunWithElevatedPrivileges(PrivilegedInitialize);

                ScriptEngine rubyEngine = _scriptRuntime.GetEngineByFileExtension(".rb");
                rubyEngine.SetSearchPaths(new List<String>
                    {
                        Path.Combine(IronConstant.IronRubyRootDirectory, @"ironruby"),
                        Path.Combine(IronConstant.IronRubyRootDirectory, @"ruby\1.9.1"),
                        IronConstant.FakeHiveDirectory,
                        Path.Combine(IronConstant.IronRubyRootDirectory, @"ruby\site_ruby\1.9.1")
                    });

                var ironRubyEngine = new IronEngine(this, rubyEngine);
                Engines[".rb"] = ironRubyEngine;

                ScriptScope scope = rubyEngine.CreateScope();
                scope.SetVariable("iron_runtime", this);
                scope.SetVariable("ruby_engine", ironRubyEngine);
                scope.SetVariable("iron_root", IronConstant.IronRubyRootDirectory);
                scope.SetVariable("iron_env",
                                  IronConstant.IronEnv == IronEnvironment.Debug
                                      ? "development"
                                      : IronConstant.IronEnv.ToString().ToLower());
                rubyEngine.Execute(
                    "$RUNTIME = iron_runtime; " +
                    "RUBY_ENGINE = ruby_engine;" +
                    "IRON_ROOT = RAILS_ROOT = iron_root;" +
                    "IRON_ENV = RAILS_ENV = iron_env",
                    scope);
                rubyEngine.Execute(@"
require 'rubygems'

begin
    require 'iron_sharepoint'
    require 'application'
rescue Exception => ex
    IRON_DEFAULT_LOGGER.error ex
end");
            }
        }

        private void PrivilegedInitialize()
        {
            string gemDir = Path.Combine(IronConstant.IronRubyRootDirectory, "ruby", "gems", "1.9.1");

            var gemPath = (Environment.GetEnvironmentVariable("GEM_PATH") ?? "").Split(new[]{';'}, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!gemPath.Contains(gemDir))
            {
                gemPath.Add(gemDir);
            }
            Environment.SetEnvironmentVariable("GEM_PATH", String.Join(";", gemPath));

            Directory.SetCurrentDirectory(IronConstant.IronRubyRootDirectory);
        }

        public static IronRuntime GetDefaultIronRuntime(SPSite targetSite)
        {
            using (new SPMonitoredScope("Retrieving IronRuntime"))
            {
                Guid targetId = targetSite.ID;
                IronRuntime runtime;
                if (!TryGetExistingRuntime(targetId, out runtime))
                {
                    lock (_sync)
                    {
                        if (!TryGetExistingRuntime(targetId, out runtime))
                        {
                            using (new SPMonitoredScope("Creating IronRuntime"))
                            {
                                runtime = new IronRuntime(targetId);
                                LivingRuntimes[targetId] = runtime;
                            }
                        }
                    }
                }
                if (HttpContext.Current != null) HttpContext.Current.Items[IronConstant.IronRuntimeKey] = runtime;

                return runtime;
            }
        }

        private static bool TryGetExistingRuntime(Guid targetId, out IronRuntime runtime)
        {
            if (!LivingRuntimes.TryGetValue(targetId, out runtime) && HttpContext.Current != null)
            {
                runtime = HttpContext.Current.Items[IronConstant.IronRuntimeKey] as IronRuntime;
            }
            return runtime != null;
        }

        public IronEngine GetEngineByExtension(string extension)
        {
            IronEngine ironEngine = null;

            if (!Engines.TryGetValue(extension, out ironEngine))
            {
                string error = String.Format("Error occured while getting engine for extension {0}", extension);
                var ex = new ArgumentException(error, "extension");
                LogError(error, ex);
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
            IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Core],
                                                    TraceSeverity.Unexpected,
                                                    String.Format("{0}\nError:{1}\nStack:{2}", msg, ex.Message,
                                                                  ex.StackTrace));
        }

        public static void LogVerbose(string msg)
        {
            IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Core],
                                                    TraceSeverity.Verbose, String.Format("{0}.", msg));
        }

        private static void ShowUnavailable()
        {
            HttpContext.Current.Response.StatusCode = 503;
            HttpContext.Current.Response.WriteFile(SPUtility.GetGenericSetupPath(@"TEMPLATE\LAYOUTS\IronSP\503.html"));
            HttpContext.Current.Response.End();
        }
    }
}