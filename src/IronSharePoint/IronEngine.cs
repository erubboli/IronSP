using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronRuby;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using System.IO;
using IronSharePoint.Diagnostics;
using Microsoft.Scripting.Runtime;
using System.Web;
using Microsoft.SharePoint.Administration;
using Microsoft.Scripting;
using System.Dynamic;

namespace IronSharePoint
{
    
    public class IronEngine
    {
        private static Dictionary<Guid, ScriptRuntime> _scripRuntimes = new Dictionary<Guid, ScriptRuntime>();

        public IronRuntime Runtime { get; private set; }
        public ScriptRuntime ScriptRuntime { get; private set; }
        public ScriptEngine ScriptEngine { get; private set; }

        public string Language 
        { 
            get
            {
                return ScriptEngine.Setup.DisplayName;
            }
        }

        private IronPlatformAdaptationLayer _platformAdaptationLayer;

        public IronPlatformAdaptationLayer PlatformAdaptationLayer
        {
            get
            {
                return _platformAdaptationLayer ??
                       (_platformAdaptationLayer = Runtime.Host.PlatformAdaptationLayer as IronPlatformAdaptationLayer);
            }
        }

        public static IronEngine GetEngineByExtension(SPSite hiveSite, string extension)
        {

            IronEngine engine;

            try
            {
                var runtime = IronRuntime.GetRuntime(hiveSite);

                if (runtime.RunningEngines.ContainsKey(extension))
                {
                    return runtime.RunningEngines[extension];
                }

                engine = new IronEngine();
                engine.Runtime = runtime;
                engine.ScriptRuntime = engine.Runtime.ScriptRuntime;
                engine.ScriptEngine = engine.Runtime.ScriptRuntime.GetEngineByFileExtension(extension);

                // if ruby
                if (engine.ScriptEngine.Setup.DisplayName == IronConstants.IronRubyLanguageName)
                {
                    var ironRubyRootFolder = Path.Combine(engine.Runtime.Host.FeatureFolderPath, "IronSP_IronRuby10\\");

                    SPSecurity.RunWithElevatedPrivileges(() =>
                    {
                        System.Environment.SetEnvironmentVariable("IRONRUBY_10_20", ironRubyRootFolder);

                        engine.ScriptEngine.SetSearchPaths(new List<String>() {
                                Path.Combine(ironRubyRootFolder, @"Lib\IronRuby"),
                                Path.Combine(ironRubyRootFolder, @"Lib\ruby\site_ruby\1.8"),
                                Path.Combine(ironRubyRootFolder, @"Lib\ruby\site_ruby"),
                                Path.Combine(ironRubyRootFolder, @"Lib\ruby\1.8"),
                                IronConstants.IronHiveRootSymbol
                        });
                    });
                }

                runtime.RunningEngines.Add(extension, engine);

            }
            catch (Exception ex)
            { 
                LogError(String.Format("Error occured while getting engine for extension {0}", extension) , ex);
                throw;
            }

            return engine;         
        }

        public object CreateDynamicInstance(string className, string scriptName)
        {
            object obj = null;

            if (Language == IronConstants.IronRubyLanguageName)
            {
                var rubyClassName = className.Replace(".", "::").Trim();

                obj = ScriptEngine.Execute(String.Format("defined?({0})?({0}.new):nil", rubyClassName));

                // load script
                if (obj == null)
                {
                    var scriptFile = GetHiveFile(scriptName);
                    ExcecuteScriptFile(scriptFile);
                    obj = ScriptEngine.Execute(String.Format("defined?({0})?({0}.new):nil", rubyClassName));
                }
            }

            return obj;
        }

        public object InvokeDynamicFunction(string functionName, string scriptName, params object[] args)
        {
            object obj = null;

            string ns = null;
            if (functionName.Contains("."))
            {
                ns = functionName;
                var split = functionName.Split('.');
                functionName = split.Last();
                ns = ns.Replace("." + functionName, String.Empty);
            }

            if (Language == IronConstants.IronRubyLanguageName)
            {
                if (ns == null)
                {
                
                    object rubyFunc = null;
                    if (!Runtime.ScriptRuntime.Globals.TryGetVariable(functionName, out rubyFunc))
                    {
                        var file = GetHiveFile(scriptName);
                        ExcecuteScriptFile(file);

                        rubyFunc = Runtime.ScriptRuntime.Globals.GetVariable(functionName);
                    }
                    
                    if (args != null && args.Length > 0)
                    {
                        obj = Runtime.ScriptRuntime.Operations.Invoke(rubyFunc, args);
                    }
                    else
                    {
                        obj = Runtime.ScriptRuntime.Operations.Invoke(rubyFunc);
                    }
                }
                else
                {
                    var rubyModule = ScriptEngine.Execute(String.Format("defined?({0})?({0}):nil", ns.Replace(".", "::").Trim()));
                    if (rubyModule == null)
                    {
                        var file = GetHiveFile(scriptName);
                        ExcecuteScriptFile(file);
                        rubyModule = ScriptEngine.Execute(ns.Replace(".", "::"));
                    }
                    if (args != null && args.Length > 0)
                    {
                        obj = Runtime.ScriptRuntime.Operations.InvokeMember(rubyModule, functionName, args);
                    }
                    else
                    {
                        obj = Runtime.ScriptRuntime.Operations.InvokeMember(rubyModule, functionName);
                    }
                }
              
            }

            return obj;
        }


        //public object InvokeDyamicMethodIfExists(string methodName, params object[] args)
        //{
        //    try
        //    {
        //        object dynMethod = null;
        //        if (ScriptScope.TryGetVariable(methodName, out dynMethod))
        //        {
        //            if (args.Length == 0)
        //            {
        //                return _scriptEngine.Operations.Invoke(dynMethod);
        //            }
        //            else
        //            {
        //                return _scriptEngine.Operations.Invoke(dynMethod, args);
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        LogError(String.Format("Error invoking method {0}", methodName), ex);
        //        throw;
        //    }

        //    return null;
        //}

        public object ExcecuteScriptFile(SPFile scriptFile)
        {
            object output = null;
          
            string script = String.Empty;

           // _runtime.Host.PlatformAdaptationLayer.CurrentDirectory = scriptFile.ParentFolder.ServerRelativeUrl;
            
            script = scriptFile.Web.GetFileAsString(scriptFile.Url);


            output = ScriptEngine.CreateScriptSourceFromString(script, SourceCodeKind.File).Execute(ScriptEngine.Runtime.Globals);


            return output;
        }

        public SPFile GetHiveFile(string fileName)
        {
            if (!fileName.Contains(IronConstants.IronHiveRootSymbol))
            {
                fileName = (IronConstants.IronHiveRootSymbol + "/" + fileName).Replace("//","/");  
            }

            if (!PlatformAdaptationLayer.FileExists(fileName))
            {
                throw new FileNotFoundException();
            }

            return PlatformAdaptationLayer.GetIronHiveFile(fileName);
        }

    
        public string LoadText(string fileName)
        {         
            var file = GetHiveFile(fileName);
            var str = Runtime.Host.HiveWeb.GetFileAsString(file.Url);

            return str;
        }
        

        public static void LogError(string msg, Exception ex)
        {
            IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Controls], TraceSeverity.Unexpected, String.Format("{0}. Error:{1}; Stack:{2}", msg, ex.Message, ex.StackTrace));
        }

        public static void LogVerbose(string msg)
        {
            IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Controls], TraceSeverity.Verbose, String.Format("{0}.", msg));
        }

        private IronEngine()
        {

        }
        
    }

    
}
