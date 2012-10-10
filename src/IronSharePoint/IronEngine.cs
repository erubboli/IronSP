using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public IronRuntime IronRuntime { get; private set; }
        public ScriptEngine ScriptEngine { get; private set; }

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
                return IronRuntime.IronHive.PlatformAdaptationLayer as IronPlatformAdaptationLayer;
            }
        }

        public object CreateDynamicInstance(string className, string scriptName, params object[] args)
        {
            object obj = null;

            if (!IronRuntime.DynamicTypeRegistry.ContainsKey(className))
            {
                var scriptFile = IronRuntime.IronHive.LoadFile(scriptName);
                ExcecuteScriptFile(scriptFile);

                if (!IronRuntime.DynamicTypeRegistry.ContainsKey(className))
                {
                    throw new NullReferenceException(String.Format("The class {0} in script file {1} is not regsitered in the DynamicTypeRegistry", className, scriptName));
                }
            }

            var dynamicType = IronRuntime.DynamicTypeRegistry[className];

            if (args != null && args.Length > 0)
            {
                obj = IronRuntime.ScriptRuntime.Operations.CreateInstance(dynamicType, args);
            }
            else
            {
                obj = IronRuntime.ScriptRuntime.Operations.CreateInstance(dynamicType);
            }

            return obj;
        }

        public object InvokeDynamicFunction(string functionName, string scriptName, params object[] args)
        {
            object obj = null;

            if (!IronRuntime.DynamicFunctionRegistry.ContainsKey(functionName))
            {
                var scriptFile = IronRuntime.IronHive.LoadFile(scriptName);
                ExcecuteScriptFile(scriptFile);

                if (!IronRuntime.DynamicFunctionRegistry.ContainsKey(functionName))
                {
                    throw new NullReferenceException(String.Format("The function {0} in script file {1} is not regsitered in the DynamicFunctionRegistry", functionName, scriptName));
                }
            }

            var dynamicType = IronRuntime.DynamicFunctionRegistry[functionName];

            if (args != null && args.Length > 0)
            {
                obj = IronRuntime.ScriptRuntime.Operations.Invoke(dynamicType, args);
            }
            else
            {
                obj = IronRuntime.ScriptRuntime.Operations.Invoke(dynamicType);
            }

            return obj;
        }


        public object ExcecuteScriptFile(SPFile scriptFile)
        {
            if (!scriptFile.Exists)
            {
                throw new FileNotFoundException();
            }

            object output = null;
          
            string script = String.Empty;

            script = scriptFile.Web.GetFileAsString(scriptFile.Url);

            output = ScriptEngine.CreateScriptSourceFromString(script, SourceCodeKind.File).Execute(ScriptEngine.Runtime.Globals);

            return output;
        }

        

        internal IronEngine(IronRuntime ironRuntime, ScriptEngine scriptEngine)
        {
            this.IronRuntime = ironRuntime;
            this.ScriptEngine = scriptEngine;
        }
        
    }

    
}
