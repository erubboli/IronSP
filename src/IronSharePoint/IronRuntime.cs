﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using IronSharePoint.Administration;
using IronSharePoint.Diagnostics;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace IronSharePoint
{
    public class IronRuntime : IDisposable
    {
        // the key is the ID of the hive
        private static readonly Dictionary<Guid, IronRuntime> _staticLivingRuntimes =
            new Dictionary<Guid, IronRuntime>();

        private static readonly object _sync = new Object();

        private readonly Guid _hiveId;
        private IronConsole.IronConsole _console;
        private bool _isInitialized;
        private ScriptRuntime _scriptRuntime;

        private IronRuntime(Guid hiveId)
        {
            _hiveId = hiveId;

            DynamicTypeRegistry = new Dictionary<string, Object>();
            DynamicFunctionRegistry = new Dictionary<string, Object>();
            Engines = new Dictionary<string, IronEngine>();
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
            get
            {
                if (_scriptRuntime == null)
                {
                    Initialize();
                }

                return _scriptRuntime;
            }
        }

        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        public IronHive IronHive
        {
            get { return (IronHive) ScriptRuntime.Host; }
        }

        public Dictionary<string, Object> DynamicTypeRegistry { get; private set; }
        public Dictionary<string, Object> DynamicFunctionRegistry { get; private set; }
        public string HttpHandlerClass { get; set; }
        public bool IsDisposed { get; private set; }

        public Guid Id
        {
            get { return _hiveId; }
        }

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

        private void Initialize()
        {
            if (!_isInitialized)
            {
                lock (_sync)
                {
                    if (!_isInitialized)
                    {
                        var setup = new ScriptRuntimeSetup();
                        var languageSetup = new LanguageSetup(
                            "IronRuby.Runtime.RubyContext, IronRuby, Version=1.0.0.1, Culture=neutral, PublicKeyToken=baeaf26a6e0611a7",
                            IronConstant.IronRubyLanguageName,
                            new[] {"IronRuby", "Ruby", "rb"},
                            new[] {".rb"});
                        setup.LanguageSetups.Add(languageSetup);
                        setup.HostType = typeof (IronHive);
                        setup.DebugMode = IronConstant.IronEnv == IronEnvironment.Staging;

                        _scriptRuntime = new ScriptRuntime(setup);
                        (_scriptRuntime.Host as IronHive).Id = _hiveId;

                        _scriptRuntime.LoadAssembly(typeof (IronRuntime).Assembly); // IronSharePoint
                        _scriptRuntime.LoadAssembly(typeof (SPSite).Assembly); // Microsoft.SharePoint
                        _scriptRuntime.LoadAssembly(typeof (IHttpHandler).Assembly); // System.Web

                        using (new SPMonitoredScope("Creating IronEngine(s)"))
                        {
                            string ironRubyRoot = Path.Combine(IronHive.FeatureFolderPath, "IronSP_IronRuby10\\");
                            ScriptEngine rubyEngine = _scriptRuntime.GetEngineByFileExtension(".rb");

                            rubyEngine.SetSearchPaths(new List<String>
                                                          {
                                                              Path.Combine(ironRubyRoot, @"Lib\IronRuby"),
                                                              Path.Combine(ironRubyRoot, @"Lib\ruby\site_ruby\1.8"),
                                                              Path.Combine(ironRubyRoot, @"Lib\ruby\1.8"),
                                                              IronConstant.IronHiveRoot
                                                          });

                            ScriptScope scope = rubyEngine.CreateScope();
                            scope.SetVariable("iron_runtime", this);
                            rubyEngine.Execute("$RUNTIME = iron_runtime", scope);
                            rubyEngine.Execute(
                                @"
require 'rubygems'
require 'iron_sharepoint'
require 'iron_templates'

begin
    require 'application'
rescue Exception => ex
    IRON_DEFAULT_LOGGER.error ex
end");

                            Engines[".rb"] = new IronEngine(this, rubyEngine);
                            SPSecurity.RunWithElevatedPrivileges(() => PrivilegedInitialize(ironRubyRoot));
                        }
                    }
                }
            }
        }

        private void PrivilegedInitialize(string rubyRoot)
        {
            string gemDir = Path.Combine(rubyRoot, "lib/ironruby/gems/1.8").Replace("\\", "/");

            Environment.SetEnvironmentVariable("IRONRUBY_10_20", rubyRoot);
            Environment.SetEnvironmentVariable("GEM_PATH", gemDir);
            Environment.SetEnvironmentVariable("GEM_HOME", gemDir);
        }

        public static IronRuntime GetDefaultIronRuntime(SPSite targetSite)
        {
            using (new SPMonitoredScope("Retrieving IronRuntime"))
            {
                Guid hiveId = IronHiveRegistry.Local.GetHiveForSite(targetSite.ID);
                if (hiveId == Guid.Empty)
                {
                    throw new InvalidOperationException(
                        String.Format("There is no IronHive mapping for the site with id {0}", targetSite.ID));
                }

                if (!LivingRuntimes.ContainsKey(hiveId))
                {
                    lock (_sync)
                    {
                        if (!LivingRuntimes.ContainsKey(hiveId))
                        {
                            using (new SPMonitoredScope("Creating IronRuntime"))
                            {
                                var runtime = new IronRuntime(hiveId);
                                runtime.Initialize();
                                LivingRuntimes[hiveId] = runtime;
                            }
                        }
                    }
                }

                return LivingRuntimes[hiveId];
            }
        }

        public IronEngine GetEngineByExtension(string extension)
        {
            IronEngine ironEngine = null;

            if (!Engines.TryGetValue(extension, out ironEngine))
            {
                string error = String.Format("Error occured while getting engine for extension {0}", extension);
                var ex = new ArgumentException(error, "extension ");
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
                                                    String.Format("{0}. Error:{1}; Stack:{2}", msg, ex.Message,
                                                                  ex.StackTrace));
        }

        public static void LogVerbose(string msg)
        {
            IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Core],
                                                    TraceSeverity.Verbose, String.Format("{0}.", msg));
        }
    }
}