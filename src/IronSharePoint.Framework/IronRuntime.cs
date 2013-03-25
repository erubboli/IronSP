using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using IronSharePoint.Diagnostics;
using IronSharePoint.Exceptions;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace IronSharePoint
{
    public class IronRuntime : IDisposable
    {
        public const string RubyEngineName = "IronRuby";

        private static readonly object _sync = new Object();

        private readonly Guid _id;
        private readonly Guid _siteId;

        static IronRuntime()
        {
            LivingRuntimes = new Dictionary<Guid, IronRuntime>();
        }

        private IronRuntime(Guid siteId)
        {
            _siteId = siteId;
            _id = Guid.NewGuid();
            ULSLogger = new IronULSLogger();

            CreateScriptRuntime();
            Console = new IronConsole(ScriptRuntime);
        }

        public IronULSLogger ULSLogger { get; private set; }

        public Exception InitializationException { get; private set; }

        internal static Dictionary<Guid, IronRuntime> LivingRuntimes { get; private set; }

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
            get { return ScriptRuntime.GetEngine(RubyEngineName); }
        }

        public IHive Hive
        {
            get { return ScripHost.Hive; }
        }

        public bool IsDisposed { get; private set; }

        public Guid SiteId
        {
            get { return _siteId; }
        }

        public Guid Id
        {
            get { return _id; }
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
                RubyEngineName,
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
                SPSecurity.RunWithElevatedPrivileges(EnsureGemPath);

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
            }
        }

        private void EnsureGemPath()
        {
            string gemDir = Path.Combine(IronConstant.IronRubyRootDirectory, "ruby", "gems", "1.9.1");

            List<string> gemPath =
                (Environment.GetEnvironmentVariable("GEM_PATH") ?? "").Split(new[] {';'},
                                                                             StringSplitOptions.RemoveEmptyEntries)
                                                                      .ToList();
            if (!gemPath.Contains(gemDir))
            {
                gemPath.Add(gemDir);
            }
            Environment.SetEnvironmentVariable("GEM_PATH", String.Join(";", gemPath));
        }

        private void InitializeRubyFramework()
        {
            try
            {
                RubyEngine.Execute(@"require 'rubygems'; require 'iron_sharepoint'; require 'application'",
                                   ScriptRuntime.Globals);
            }
            catch (Exception ex)
            {
                InitializationException = ex;
                throw new RubyFrameworkInitializationException("Error loading ruby framework", ex);
            }
        }

        public static IronRuntime Create(SPSite targetSite)
        {
            Guid targetId = targetSite.ID;
            var runtime = new IronRuntime(targetId);
            runtime.InitializeRubyFramework();
            return runtime;
        }

        public static IronRuntime GetDefaultIronRuntime(SPSite targetSite)
        {
            using (new SPMonitoredScope("Retrieving IronRuntime"))
            {
                Guid targetId = targetSite.ID;
                IronRuntime runtime;
                if (!TryGetExistingRuntime(targetId, out runtime))
                {
                    using (new SPMonitoredScope("Creating IronRuntime"))
                    {
                        try
                        {
                            runtime = Create(targetSite);
                        }
                        catch (RubyFrameworkInitializationException ex)
                        {
                            IronULSLogger.Local.Error(
                                string.Format("Could not initialize ruby framework for SPSite '{0}'", targetId),
                                ex.InnerException, IronCategoryDiagnosticsId.Core);
                        }
                        catch (Exception ex)
                        {
                            IronULSLogger.Local.Error(
                                string.Format("Could not create IronRuntime for SPSite '{0}'", targetId), ex,
                                IronCategoryDiagnosticsId.Core);
                            throw new IronRuntimeAccesssException("Cannot access IronRuntime", ex) {SiteId = targetId};
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
                lock (_sync)
                {
                    if (!LivingRuntimes.TryGetValue(targetId, out runtime) && HttpContext.Current != null)
                    {
                        runtime = HttpContext.Current.Items[IronConstant.IronRuntimeKey] as IronRuntime;
                    }
                }
            }
            return runtime != null;
        }
    }
}