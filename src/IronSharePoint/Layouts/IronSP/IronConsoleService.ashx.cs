using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using IronSharePoint.IronConsole;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web;
using IronSharePoint.Administration;
using System.Linq;

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
            var response = new Response();
            var jsonResponse = string.Empty;

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
                var scriptEngine = engine.ScriptEngine;
                if (extension == ".rb")
                {
                    // TODO: fix load path
                    scriptEngine.Execute(@"
                    unless defined?(IronConsole::Utils)
                      begin
                        require 'C:\Program Files\Common Files\Microsoft Shared\Web Server Extensions\14\TEMPLATE\FEATURES\IronSP_Hive_Site\IronConsole\iron_console_utils.rb'
                        include IronConsole::Utils 
                        puts 'IronConsole Utils loaded'
                      rescue
                        raise 'Could not load IronConsole Utils'
                      end
                    end");
                }

                var obj = engine.ScriptEngine.Execute(expression, engine.IronRuntime.ScriptRuntime.Globals);
                response.Result = (obj != null ? obj.ToString() : "nil");

                if (extension == ".rb")
                {
                    var consoleOut = scriptEngine.Execute(String.Format("console_out '{0}'", Environment.NewLine));
                    response.Output = Convert.ToString(consoleOut);
                }
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
                response.StackTrace = ex.StackTrace;
            }
            finally
            {
                jsonResponse = response.ToJson();
            }

            context.Response.Write(jsonResponse);
        }
    }
}
