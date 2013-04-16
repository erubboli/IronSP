using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using IronSharePoint.Administration;
using IronSharePoint.Diagnostics;
using IronSharePoint.Util;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace IronSharePoint
{
    public abstract class IronRuntimeBase : IDisposable
    {
        private Lazy<ScriptRuntime> _scriptRuntime;
        private Lazy<IronConsole> _console;
        public const string RubyEngineName = "IronRuby";

        protected IronRuntimeBase()
            : this(Guid.NewGuid()) {}

        protected IronRuntimeBase(Guid id)
        {
            Id = id;
            ULSLogger = new IronULSLogger();

            _console = new Lazy<IronConsole>(() => new IronConsole(ScriptRuntime));
            _scriptRuntime = new Lazy<ScriptRuntime>(CreateScriptRuntime);
        }

        protected void Initialize()
        {
            CreateScriptRuntime();
           
        }

        public IronULSLogger ULSLogger { get; private set; }
        public Exception InitializationException { get; protected set; }
        public IronConsole Console
        {
            get { return _console.Value; }
        }

        public ScriptRuntime ScriptRuntime
        {
            get { return _scriptRuntime.Value; }
        }

        public IronScriptHost ScriptHost
        {
            get { return (IronScriptHost) ScriptRuntime.Host; }
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

        public Guid Id { get; private set; }

        public abstract string Name { get; }
        public abstract IronEnvironment Environment { get; }

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

        private ScriptRuntime CreateScriptRuntime()
        {
            var setup = new ScriptRuntimeSetup();
            var languageSetup = new LanguageSetup(
                "IronRuby.Runtime.RubyContext, IronRuby, Version=1.1.3.0, Culture=neutral, PublicKeyToken=7f709c5b713576e1",
                RubyEngineName,
                new[] {"IronRuby", "Ruby", "rb"},
                new[] {".rb"});
            setup.LanguageSetups.Add(languageSetup);
            setup.HostType = typeof (IronScriptHost);
            setup.HostArguments = GetHiveSetups().ToArray();
            setup.DebugMode = Environment == IronEnvironment.Debug;

            var scriptRuntime = new ScriptRuntime(setup);
            scriptRuntime.LoadAssembly(typeof (IronRuntime).Assembly); // IronSharePoint
            scriptRuntime.LoadAssembly(typeof (SPSite).Assembly); // Microsoft.SharePoint
            scriptRuntime.LoadAssembly(typeof (IHttpHandler).Assembly); // System.Web

            var scriptEngine = CreateScriptEngine(scriptRuntime);

            try
            {
                InitializeScriptEngine(scriptEngine);
            }
            catch (Exception ex)
            {
                InitializationException = ex;
                IronULSLogger.Local.Error(
                    string.Format("Could not initialize ruby framework for runtime '{0}'", Id),
                    ex, IronCategoryDiagnosticsId.Core);
            }
            return scriptRuntime;
        }

        protected abstract IEnumerable<HiveSetup> GetHiveSetups();
        protected abstract IEnumerable<string> GetGemPaths();

        private ScriptEngine CreateScriptEngine(ScriptRuntime scriptRuntime)
        {
            using (new SPMonitoredScope("Initializing IronEngine(s)"))
            {
                var scriptEngine = scriptRuntime.GetEngine(RubyEngineName);

                scriptEngine.SetSearchPaths(new List<String>
                    {
                        Path.Combine(IronConstant.IronRubyDirectory, @"ironruby"),
                        Path.Combine(IronConstant.IronRubyDirectory, @"ruby\1.9.1"),
                        Path.Combine(IronConstant.IronRubyDirectory, @"ruby\site_ruby\1.9.1"),
                        IronConstant.HiveWorkingDirectory
                    });

                ScriptScope scope = scriptEngine.CreateScope();
                scope.SetVariable("iron_runtime", this);
                scriptEngine.Execute("$RUNTIME = iron_runtime", scope);

                return scriptEngine;
            }
        }

        protected virtual void InitializeScriptEngine(ScriptEngine scriptEngine)
        {
            var joinedPaths = GetGemPaths().Select(x => string.Format("'{0}'", x)).StringJoin(",");
            var script = new StringBuilder()
                .AppendLine("require 'rubygems'")
                .AppendLine("Encoding.default_internal = Encoding.UTF8")
                .AppendLine("Encoding.default_external = Encoding.UTF8")
                .AppendLine("Gem.clear_paths")
                .AppendFormat("Gem.use_paths '', [{0}]", joinedPaths);

            scriptEngine.Execute(script.ToString());
        }
    }
}