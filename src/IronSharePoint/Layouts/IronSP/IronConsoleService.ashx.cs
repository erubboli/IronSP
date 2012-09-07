using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web;
using IronSharePoint.Administration;

namespace IronSharePoint
{
    public partial class IronConsoleService : IHttpHandler
    {
        readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var responseData = new Dictionary<string, string>();

            try
            {
                var site = SPContext.Current.Site;
                var web = SPContext.Current.Web;
                if (!web.CurrentUser.IsSiteAdmin)
                {
                    context.Response.Write("Only Site Admins are allowed to use the Console");
                    return;
                }
                

                IronHiveRegistry.Local.EnsureTrustedHive(site.ID);

                var ironRuntime = IronRuntime.GetIronRuntime(site, site.ID);

                var extension = HttpContext.Current.Request["ext"];
                var expression = HttpContext.Current.Request["expression"];

                var engine = ironRuntime.GetEngineByExtension(extension);

                if (extension == ".rb")
                {
                    engine.ScriptEngine.Execute(String.Format(@"
$rb_console_out=''
def puts(o)
    if o.respond_to? :GetEnumerator
        $rb_console_out += '['
        o.each_with_index do |x,i|
            $rb_console_out += x.inspect
            $rb_console_out += ', '
        end
        $rb_console_out = $rb_console_out[0...-2] + ']'
    else
        $rb_console_out +=o.inspect + '{0}' unless o.nil?
    end
    return nil
end
", System.Environment.NewLine));
                }
                
                var obj = engine.ScriptEngine.Execute(expression, engine.IronRuntime.ScriptRuntime.Globals);
                responseData["result"] = (obj != null ? obj.ToString() : "nil");

                if (extension == ".rb")
                {
                    responseData["output"] = engine.ScriptEngine.Execute("$rb_console_out").ToString();
                }
            }
            catch (Exception ex)
            {
                responseData["error"] = ex.Message + Environment.NewLine + ex.StackTrace;
            }

            var json = _serializer.Serialize(responseData);
            context.Response.Write(json);
        }
    }
}
