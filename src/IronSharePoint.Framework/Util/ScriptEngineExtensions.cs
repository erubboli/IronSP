using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;

namespace IronSharePoint.Util
{
    public static class ScriptEngineExtensions
    {
        public static dynamic ExecuteSPFile(this ScriptEngine scriptEngine, SPFile file)
        {
            Contract.Requires<FileNotFoundException>(file.Exists);

            var script = file.Web.GetFileAsString(file.Url);
            var source = scriptEngine.CreateScriptSourceFromString(script, SourceCodeKind.File);
            return source.Execute(scriptEngine.Runtime.Globals);
        }

        public static dynamic CreateInstance(this ScriptEngine scriptEngine, string typeName, params object[] parameters)
        {
            Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(typeName));

            if (scriptEngine.Setup.DisplayName == IronConstant.RubyLanguageName)
            {
                // TODO better way?
                try
                {
                    var type = scriptEngine.Execute(typeName);
                    return scriptEngine.Operations.CreateInstance(type, parameters);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(string.Format("{0} not found", typeName), "typeName", ex);
                }
            }
            return null;
        }
    }
}
