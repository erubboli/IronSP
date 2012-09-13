using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Compilation;
using System.CodeDom;
using System.Web;
using System.ComponentModel;
using Microsoft.SharePoint;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Scripting.Hosting;

namespace IronSharePoint
{
    public class IronExpressionBuilder: ExpressionBuilder
    {
        private IronEngine engine;

        public override System.CodeDom.CodeExpression GetCodeExpression(System.Web.UI.BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {

            throw new NotSupportedException();
        }

        public override bool SupportsEvaluate
        {
            get
            {
                return true;
            }
        }

        public override object EvaluateExpression(object target, System.Web.UI.BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            string value = String.Empty;
            string scriptName = entry.Expression;

            try
            {
                string functionName = null;

                if (scriptName.Contains("@"))
                {
                    var tmp = scriptName.Split('@');
                    functionName = tmp[0].Trim();
                    scriptName = tmp[1].Trim();
                }
                else
                {
                    throw new ArgumentException("Invalid expression! Use <%$Iron:My.sayHello@my/functions.rb");
                }

                engine = IronRuntime.GetDefaultIronRuntime(SPContext.Current.Site).GetEngineByExtension(Path.GetExtension(scriptName));
                value = engine.InvokeDynamicFunction(functionName, scriptName, target, entry).ToString();     
            }
            catch (Exception ex)
            {
                IronRuntime.LogError("Error", ex);

                if (SPContext.Current.Web.UserIsSiteAdmin && engine.IronRuntime.IronHive.Web.CurrentUser.IsSiteAdmin)
                {
                    var eo = engine.ScriptEngine.GetService<ExceptionOperations>();
                    string error = eo.FormatException(ex);

                    IronRuntime.LogError(String.Format("Error executing script {0}: {1}", scriptName, error), ex);

                    value = error;
                }
                else
                {
                    value = "Error occured";
                }
            }

            return value;
        }

    }
}
