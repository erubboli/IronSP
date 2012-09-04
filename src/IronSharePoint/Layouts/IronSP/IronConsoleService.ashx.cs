﻿using System;
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
                var script ="_=(" + HttpContext.Current.Request["script"] +");_";

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
                
                var obj = engine.ScriptEngine.Execute(script, engine.IronRuntime.ScriptRuntime.Globals);

                output = "=> " + (obj != null ? obj.ToString() : "nil");

                if (extension == ".rb")
                {
                    var consoleOut = engine.ScriptEngine.Execute("$rb_console_out.ToString()").ToString();
                    if(consoleOut!=String.Empty)
                    {
                        output = consoleOut + System.Environment.NewLine + output;
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
