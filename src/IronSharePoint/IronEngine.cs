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
       // private static Dictionary<Guid, ScriptRuntime> _scripRuntimes = new Dictionary<Guid, ScriptRuntime>();

        public IronRuntime IronRuntime
        {
            get;
            private set;
        }

        public ScriptEngine ScriptEngine
        {
            get;
            private set;
        }

        public string Language 
        { 
            get
            {
                return ScriptEngine.Setup.DisplayName;
            }
        }       

        public IronPlatformAdaptationLayer PlatformAdaptationLayer
        {
            get 
            {
                return IronRuntime.Host.PlatformAdaptationLayer as IronPlatformAdaptationLayer;
            }
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
                    if (!IronRuntime.ScriptRuntime.Globals.TryGetVariable(functionName, out rubyFunc))
                    {
                        var file = GetHiveFile(scriptName);
                        ExcecuteScriptFile(file);

                        rubyFunc = IronRuntime.ScriptRuntime.Globals.GetVariable(functionName);
                    }
                    
                    if (args != null && args.Length > 0)
                    {
                        obj = IronRuntime.ScriptRuntime.Operations.Invoke(rubyFunc, args);
                    }
                    else
                    {
                        obj = IronRuntime.ScriptRuntime.Operations.Invoke(rubyFunc);
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
                        obj = IronRuntime.ScriptRuntime.Operations.InvokeMember(rubyModule, functionName, args);
                    }
                    else
                    {
                        obj = IronRuntime.ScriptRuntime.Operations.InvokeMember(rubyModule, functionName);
                    }
                }
              
            }

            return obj;
        }


        public object ExcecuteScriptFile(SPFile scriptFile)
        {
            object output = null;
          
            string script = String.Empty; 
            
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
            var str = IronRuntime.Host.HiveWeb.GetFileAsString(file.Url);

            return str;
        }

        internal IronEngine(IronRuntime ironRuntime, ScriptEngine scriptEngine)
        {
            this.IronRuntime = ironRuntime;
            this.ScriptEngine = scriptEngine;
        }
        
    }

    
}
