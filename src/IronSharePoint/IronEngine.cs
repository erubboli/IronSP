using System;
using System.IO;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;

namespace IronSharePoint
{
    public class IronEngine
    {
        internal IronEngine(IronRuntime ironRuntime, ScriptEngine scriptEngine)
        {
            IronRuntime = ironRuntime;
            ScriptEngine = scriptEngine;
        }

        public IronRuntime IronRuntime { get; private set; }
        public ScriptEngine ScriptEngine { get; private set; }

        public string Language
        {
            get { return ScriptEngine.Setup.DisplayName; }
        }

        public object CreateDynamicInstance(string className, string scriptName, params object[] args)
        {
#if DEBUG
            ScriptEngine.Execute("load '" + scriptName + "'", IronRuntime.ScriptRuntime.Globals);
#else

            if (!IronRuntime.DynamicTypeRegistry.ContainsKey(className))
            {
                SPFile scriptFile = IronRuntime.IronHive.LoadFile(scriptName);

                if (scriptFile == null)
                {
                    throw new NullReferenceException(String.Format("Script file {0} not found!", scriptName));
                }

                ExcecuteScriptFile(scriptFile);

                if (!IronRuntime.DynamicTypeRegistry.ContainsKey(className))
                {
                    throw new NullReferenceException(
                        String.Format("The class {0} in script file {1} is not registered in the DynamicTypeRegistry",
                                      className, scriptName));
                }
            }
#endif


            return IronRuntime.CreateDynamicInstance(className, args);
        }

        public object InvokeDynamicFunction(string functionName, string scriptName, params object[] args)
        {
            object obj = null;

            if (!IronRuntime.DynamicFunctionRegistry.ContainsKey(functionName))
            {
                SPFile scriptFile = IronRuntime.IronHive.LoadFile(scriptName);
                ExcecuteScriptFile(scriptFile);

                if (!IronRuntime.DynamicFunctionRegistry.ContainsKey(functionName))
                {
                    throw new NullReferenceException(
                        String.Format(
                            "The function {0} in script file {1} is not registered in the DynamicFunctionRegistry",
                            functionName, scriptName));
                }
            }

            object dynamicType = IronRuntime.DynamicFunctionRegistry[functionName];

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

            output =
                ScriptEngine.CreateScriptSourceFromString(script, SourceCodeKind.File).Execute(
                    ScriptEngine.Runtime.Globals);

            return output;
        }
    }
}