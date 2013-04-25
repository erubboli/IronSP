using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private ScriptRuntime _scriptRuntime;
        private Lazy<IronConsole> _console;
        private ScriptEngine _rubyEngine;
        private Task _initializationTask;
        public bool IsInitialized { get; private set; }
        public const string RubyEngineName = "IronRuby";

        protected IronRuntimeBase()
            : this(Guid.NewGuid()) {}

        public Task InitializationTask
        {
            get { return _initializationTask; }
        }

        protected IronRuntimeBase(Guid id)
        {
            Id = id;
            ULSLogger = new IronULSLogger(IronDiagnosticsService.Local, this);

            _console = new Lazy<IronConsole>(() => new IronConsole(ScriptRuntime));
        }

        protected void Initialize()
        {
            if (_initializationTask == null)
            {
                _initializationTask = Task.Run(() =>
                {
                    _scriptRuntime = CreateScriptRuntime();
                    _rubyEngine = CreateRubyEngine(_scriptRuntime);
                    try
                    {
                        InitializeRubyEngine(RubyEngine);
                    }
                    catch (Exception ex)
                    {
                        InitializationException = ex;
                        ULSLogger.Error(
                            string.Format("Could not initialize ruby framework for runtime '{0}'", Id),
                            ex, IronCategoryDiagnosticsId.Core);
                    }
                    finally
                    {
                        IsInitialized = true;
                        _initializationTask = null;
                    }
                });
            }
        }

        public IronULSLogger ULSLogger { get; private set; }
        public Exception InitializationException { get; protected set; }
        public IronConsole Console
        {
            get { return _console.Value; }
        }

        public ScriptRuntime ScriptRuntime
        {
            get { return _scriptRuntime; }
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
            get { return _rubyEngine; }
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
                "IronRuby.Runtime.RubyContext, IronRuby, Version=1.1.4.0, Culture=neutral, PublicKeyToken=7f709c5b713576e1",
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

            return scriptRuntime;
        }

        protected abstract IEnumerable<HiveSetup> GetHiveSetups();
        protected abstract IEnumerable<string> GetGemPaths();

        private ScriptEngine CreateRubyEngine(ScriptRuntime scriptRuntime)
        {
            using (new SPMonitoredScope("Initializing IronEngine(s)"))
            {
                var scriptEngine = scriptRuntime.GetEngine(RubyEngineName);

                scriptEngine.SetSearchPaths(new List<String>
                    {
                        Path.Combine(IronConstant.IronRubyDirectory, @"ironruby"),
                        Path.Combine(IronConstant.IronRubyDirectory, @"ruby\1.9.1"),
                        Path.Combine(IronConstant.IronRubyDirectory, @"ruby\site_ruby\1.9.1"),
                        IronConstant.HiveWorkingDirectory,
                        IronConstant.IronSPRootDirectory
                    });

                ScriptScope scope = scriptEngine.CreateScope();
                scope.SetVariable("iron_runtime", this);
                scriptEngine.Execute("$RUNTIME = iron_runtime", scope);

                return scriptEngine;
            }
        }

        protected virtual void InitializeRubyEngine(ScriptEngine scriptEngine)
        {
            var joinedPaths = GetGemPaths().Select(x => string.Format("'{0}'", x)).StringJoin(",");
            var script = new StringBuilder()
                .AppendLine("require 'rubygems'")
                .AppendLine("begin")
                .AppendLine("  load_assembly 'Microsoft.SharePoint.Publishing, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c'")
                .AppendLine("rescue; end")
                .AppendLine("Encoding.default_internal = Encoding.UTF8")
                .AppendLine("Encoding.default_external = Encoding.UTF8")
                .AppendLine("Gem.clear_paths")
                .AppendFormat("Gem.use_paths '', [{0}]", joinedPaths);

            scriptEngine.Execute(script.ToString());
        }
    }
}