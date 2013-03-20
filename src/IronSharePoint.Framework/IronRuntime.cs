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

        private IronRuntime(Guid siteId)
        {
            _siteId = siteId;
            _id = Guid.NewGuid();

            DynamicTypeRegistry = new Dictionary<string, Object>();
            DynamicFunctionRegistry = new Dictionary<string, Object>();

            CreateScriptRuntime();
            Console = new IronConsole(ScriptRuntime);
        }

        internal static Dictionary<Guid, IronRuntime> LivingRuntimes
        {
            get { return _staticLivingRuntimes; }
        }

        public IronConsole Console { get; private set; }
        public ScriptRuntime ScriptRuntime { get; private set; }

        public IronScriptHost ScripHost
        {
            get { return (IronScriptHost) ScriptRuntime.Host; }
        }

        public IronPlatformAdaptationLayer PlatformAdaptationLayer
        {
            get { return ScripHost.IronPlatformAdaptationLayer; }
        }

        public ScriptEngine RubyEngine
        {
            get { return ScriptRuntime.GetEngine(IronConstant.RubyLanguageName); }
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
                ScripHost.Dispose();
                LivingRuntimes.Remove(_siteId);
            }
        }

        #endregion

        private void CreateScriptRuntime()
        {
            var setup = new ScriptRuntimeSetup();
            var languageSetup = new LanguageSetup(
                "IronRuby.Runtime.RubyContext, IronRuby, Version=1.1.3.0, Culture=neutral, PublicKeyToken=7f709c5b713576e1",
                IronConstant.RubyLanguageName,
                new[] {"IronRuby", "Ruby", "rb"},
                new[] {".rb"});
            setup.LanguageSetups.Add(languageSetup);
            setup.HostType = typeof (IronScriptHost);
            setup.HostArguments = new object[] {_siteId};
            setup.DebugMode = IronConstant.IronEnv == IronEnvironment.Debug;

            ScriptRuntime = new ScriptRuntime(setup);
            ScriptRuntime.LoadAssembly(typeof (IronRuntime).Assembly); // IronSharePoint
            ScriptRuntime.LoadAssembly(typeof (SPSite).Assembly); // Microsoft.SharePoint
            ScriptRuntime.LoadAssembly(typeof (IHttpHandler).Assembly); // System.Web

            InitializeScriptEngines();
        }

        private void InitializeScriptEngines()
        {
            using (new SPMonitoredScope("Initializing IronEngine(s)"))
            {
                SPSecurity.RunWithElevatedPrivileges(PrivilegedInitialize);

                RubyEngine.SetSearchPaths(new List<String>
                    {
                        Path.Combine(IronConstant.IronRubyRootDirectory, @"ironruby"),
                        Path.Combine(IronConstant.IronRubyRootDirectory, @"ruby\1.9.1"),
                        IronConstant.FakeHiveDirectory,
                        Path.Combine(IronConstant.IronRubyRootDirectory, @"ruby\site_ruby\1.9.1")
                    });


                ScriptScope scope = RubyEngine.CreateScope();
                scope.SetVariable("iron_runtime", this);
                RubyEngine.Execute("$RUNTIME = iron_runtime", scope);
                RubyEngine.Execute(@"require 'rubygems'; #require 'application'");
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