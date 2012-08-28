﻿using System;
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

        private IronRuntime _runtime;

        public IronRuntime Runtime
        {
            get { return _runtime; }
        }

        public ScriptRuntime ScriptRuntime
        {
            get { return _scriptEngine.Runtime; }
        }

        private ScriptEngine _scriptEngine;

        public ScriptEngine ScriptEngine
        {
            get { return _scriptEngine; }
        }

        public string Language 
        { 
            get
            {
                return _scriptEngine.Setup.DisplayName;
            }
        }

        public static IronEngine GetEngineByExtension(SPSite hiveSite, string extension)
        {     

            var engine = new IronEngine();

            try
            {
                engine._runtime = IronRuntime.GetRuntime(hiveSite);

                engine._scriptEngine = engine._runtime.ScriptRuntime.GetEngineByFileExtension(extension);

                // if ruby
                if (engine._scriptEngine.Setup.DisplayName == IronConstants.IronRubyLanguageName)
                {
                    var ironRubyRootFolder = Path.Combine(engine._runtime.Host.FeatureFolderPath, "IronSP_IronRuby10\\");

                    SPSecurity.RunWithElevatedPrivileges(() =>
                    {
                        System.Environment.SetEnvironmentVariable("IRONRUBY_10_20", ironRubyRootFolder);

                        engine._scriptEngine.SetSearchPaths(new List<String>() {
                                Path.Combine(ironRubyRootFolder, @"Lib\IronRuby"),
                                Path.Combine(ironRubyRootFolder, @"Lib\ruby\site_ruby\1.8"),
                                Path.Combine(ironRubyRootFolder, @"Lib\ruby\site_ruby"),
                                Path.Combine(ironRubyRootFolder, @"Lib\ruby\1.8"),
                                "@IronHive"
                        });
                    });
                }
            }
            catch (Exception ex)
            { 
                LogError(String.Format("Error occured while getting engine for extension {0}", extension) , ex);
                throw ex;
            }

            return engine;         
        }

        public object CreateDynamicInstance(string className, string scriptName)
        {
            object obj = null;

            if (Language == IronConstants.IronRubyLanguageName)
            {
                var rubyClassName = className.Replace(".", "::").Trim();

                obj = _scriptEngine.Execute(String.Format("defined?({0})?({0}.new):nil", rubyClassName));

                // load script
                if (obj == null)
                {
                    var scriptFile = GetFile(scriptName);
                    ExcecuteScriptFile(scriptFile);
                    obj = _scriptEngine.Execute(String.Format("defined?({0})?({0}.new):nil", rubyClassName));
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
                    if (!_runtime.ScriptRuntime.Globals.TryGetVariable(functionName, out rubyFunc))
                    {
                        var file = GetFile(scriptName);
                        ExcecuteScriptFile(file);

                        rubyFunc = _runtime.ScriptRuntime.Globals.GetVariable(functionName);
                    }
                    
                    if (args != null && args.Length > 0)
                    {
                        obj = _runtime.ScriptRuntime.Operations.Invoke(rubyFunc, args);
                    }
                    else
                    {
                        obj = _runtime.ScriptRuntime.Operations.Invoke(rubyFunc);
                    }
                }
                else
                {
                    var rubyModule = _scriptEngine.Execute(String.Format("defined?({0})?({0}):nil", ns.Replace(".", "::").Trim()));
                    if (rubyModule == null)
                    {
                        var file = GetFile(scriptName);
                        ExcecuteScriptFile(file);
                        rubyModule = _scriptEngine.Execute(ns.Replace(".", "::"));
                    }
                    if (args != null && args.Length > 0)
                    {
                        obj = _runtime.ScriptRuntime.Operations.InvokeMember(rubyModule, functionName, args);
                    }
                    else
                    {
                        obj = _runtime.ScriptRuntime.Operations.InvokeMember(rubyModule, functionName);
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


            script = scriptFile.Web.GetFileAsString(scriptFile.Url);

         
            output = _scriptEngine.CreateScriptSourceFromString(script, SourceCodeKind.File).Execute(_scriptEngine.Runtime.Globals);
            

            return output;
        }

        public SPFile GetFile(string fileName)
        {
           

            SPFile file = null;
            var hiveWeb = _runtime.Host.HiveWeb;

            /*
            if (_initialScriptFolder != null)
            {
                file = rootWeb.GetFile(String.Format("{0}/{1}", _initialScriptFolder.ServerRelativeUrl, fileName));
                if (file.Exists)
                {
                    ctx.Cache[fileName] = file;
                    return file;
                }
            }
             * */

            file = hiveWeb.GetFile(String.Format("{0}/{1}", _runtime.Host.HiveFolder.ServerRelativeUrl, fileName));
            if (!file.Exists)
            {
                throw new FileNotFoundException();
            }
            //ctx.Cache[fileName] = file;
            return file;
        }

    
        public string LoadText(string fileName)
        {         
            var file = GetFile(fileName);
            var str = _runtime.Host.HiveWeb.GetFileAsString(file.Url);

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
        
    }

    
}
