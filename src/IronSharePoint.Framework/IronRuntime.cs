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
    public class IronRuntime : IronRuntime<IronScriptHost>
    {
        private static readonly object Lock = new Object();

        static IronRuntime()
        {
            LivingRuntimes = new Dictionary<Guid, IronRuntime>();
        }

        public IronRuntime(Guid siteId)
            : base(siteId, new object[] {siteId})
        {
        }

        internal static Dictionary<Guid, IronRuntime> LivingRuntimes { get; private set; }

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
                            runtime = new IronRuntime(targetId);
                        }
                        catch (Exception ex)
                        {
                            IronULSLogger.Local.Error(
                                string.Format("Could not create IronRuntime for SPSite '{0}'", targetId), ex,
                                IronCategoryDiagnosticsId.Core);
                            throw new IronRuntimeAccesssException("Cannot access IronRuntime", ex) {SiteId = targetId};
                        }
                        finally
                        {
                            LivingRuntimes[targetId] = runtime;
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
                lock (Lock)
                {
                    if (!LivingRuntimes.TryGetValue(targetId, out runtime) && HttpContext.Current != null)
                    {
                        runtime = HttpContext.Current.Items[IronConstant.IronRuntimeKey] as IronRuntime;
                    }
                }
            }
            return runtime != null;
        }

        public override void Dispose()
        {
            base.Dispose();
            LivingRuntimes.Remove(SiteId);
        }

        protected override void Initialize()
        {
            base.Initialize();
            RubyEngine.Execute(@"require 'rubygems'; require 'iron_sharepoint'; require 'application'",
                               ScriptRuntime.Globals);
        }
    }

    public class IronRuntime<TScriptHost> : IDisposable
        where TScriptHost : IronScriptHostBase
    {
        public const string RubyEngineName = "IronRuby";

        private readonly object[] _hostArguments;
        private readonly Guid _id;
        private readonly Guid _siteId;

        protected IronRuntime(Guid siteId, object[] hostArguments)
        {
            _siteId = siteId;
            _id = Guid.NewGuid();
            _hostArguments = hostArguments;

            ULSLogger = new IronULSLogger();

            CreateScriptRuntime();
            Console = new IronConsole(ScriptRuntime);
            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                InitializationException = ex;
                IronULSLogger.Local.Error(
                    string.Format("Could not initialize ruby framework for SPSite '{0}'", SiteId),
                    ex, IronCategoryDiagnosticsId.Core);
            }
        }

        public IronULSLogger ULSLogger { get; private set; }
        public Exception InitializationException { get; protected set; }
        public IronConsole Console { get; private set; }
        public ScriptRuntime ScriptRuntime { get; private set; }

        public TScriptHost ScriptHost
        {
            get { return (TScriptHost) ScriptRuntime.Host; }
        }

        public IronPlatformAdaptationLayer PlatformAdaptationLayer
        {
            get { return ScriptHost.IronPlatformAdaptationLayer; }
        }

        public ScriptEngine RubyEngine
        {
            get { return ScriptRuntime.GetEngine(RubyEngineName); }
        }

        public IHive Hive
        {
            get { return ScriptHost.Hive; }
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

        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                if (ScriptHost != null) ScriptHost.Dispose();
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
            setup.HostType = typeof (TScriptHost);
            setup.HostArguments = _hostArguments;
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
                        IronConstant.HiveWorkingDirectory,
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

        protected virtual void Initialize()
        {
            RubyEngine.Execute(@"require 'rubygems'");
        }
    }
}