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

namespace IronSharePoint
{
    public class IronExpressionBuilder: ExpressionBuilder
    {

        public override System.CodeDom.CodeExpression GetCodeExpression(System.Web.UI.BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {

            throw new NotSupportedException();

            //CodeExpression[] inputParams = new CodeExpression[] { new CodePrimitiveExpression(entry.Expression.Trim()), 
            //                                           new CodeTypeOfExpression(entry.DeclaringType), 
            //                                           new CodePrimitiveExpression(entry.PropertyInfo.Name) };

            //// Return a CodeMethodInvokeExpression that will invoke the GetRequestedValue method using the specified input parameters 
            //return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(this.GetType()), "GetValue", inputParams);

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
            string scriptName = entry.Expression;
            string functionName = null;

            if (scriptName.Contains("@"))
            {
                var tmp = scriptName.Split('@');
                functionName = tmp[0];
                scriptName = tmp[1];  
            }

            var ironEngine = IronRuntime.GetIronRuntime(SPContext.Current.Site.ID).GetEngineByExtension(Path.GetExtension(scriptName));
            var value = ironEngine.InvokeDynamicFunction(functionName, scriptName, target, entry);
           
            return value.ToString();
        }

        //public static object GetValue(string key, Type targetType, string propertyName)
        //{
        //    string scriptName = key;
        //    string functionName = null;
        //    if(scriptName.Contains("@"))
        //    {
        //        var tmp = scriptName.Split('@');
        //        functionName = tmp[0];
        //        scriptName = tmp[1];
        //    }

        //    var ironEngine = IronEngine.GetEngine(Path.GetExtension(scriptName), SPContext.Current.Web);

        //    object value = ironEngine.LoadScript(scriptName);

        //    if (!String.IsNullOrEmpty(functionName))
        //    {
        //        value = ironEngine.InvokeDyamicMethodIfExists(functionName);
        //    }


        //    //// Convert the Session variable if its type does not match up with the Web control property type 
        //    if (targetType != null)
        //    {
        //        PropertyDescriptor propDesc = TypeDescriptor.GetProperties(targetType)[propertyName];
        //        if (propDesc != null && propDesc.PropertyType != value.GetType())
        //        {
        //            // Type mismatch - make sure that the Session variable value can be converted to the Web control property type 
        //            if (propDesc.Converter.CanConvertFrom(value.GetType()) == false)
        //                throw new InvalidOperationException(string.Format("Output cannot be converted to type {0}.", propDesc.PropertyType.ToString()));
        //            else
        //                return propDesc.Converter.ConvertFrom(value);
        //        }
        //    }

        //    // If we reach here, no type mismatch - return the value 
        //    return value;
        //} 

    }
}
