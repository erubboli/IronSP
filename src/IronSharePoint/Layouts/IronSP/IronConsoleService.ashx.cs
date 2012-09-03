using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web;
using IronSharePoint.Administration;

namespace IronSharePoint
{
    public partial class IronConsoleService : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string output = null;

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
                var script = HttpContext.Current.Request["script"];

                var engine = ironRuntime.GetEngineByExtension(extension);


                if (extension == ".rb")
                {
                    engine.ScriptEngine.Execute(String.Format(@"
$rb_console_out=''
def puts(o)
    $rb_console_out +=o.ToString() + '{0}'
    return nil
end
", System.Environment.NewLine));
                }
                
                var obj = engine.ScriptEngine.Execute(script, engine.IronRuntime.ScriptRuntime.Globals);

                output = obj == null ? String.Empty: "=> " + obj.ToString();

                if (extension == ".rb")
                {
                    var consoleOut = engine.ScriptEngine.Execute("$rb_console_out.ToString()").ToString();
                    if(consoleOut!=String.Empty)
                    {
                        output = consoleOut + output;
                    }
                }
            }
            catch (Exception ex)
            {
                output = ex.Message;
            }

            context.Response.Write(output);
        }
    }
}
