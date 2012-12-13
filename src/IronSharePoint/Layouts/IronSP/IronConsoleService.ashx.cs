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
            var response = new IronConsoleResult();
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

                //var ironRuntime = IronRuntime.GetIronRuntime(site, site.ID);
                var ironRuntime = IronRuntime.GetDefaultIronRuntime(site);
                var extension = HttpContext.Current.Request["ext"];
                var expression = HttpContext.Current.Request["expression"];

                if (expression == "_ = (kill);_.inspect")
                {
                    ironRuntime.Dispose();
                    response.Output = "Runtime disposed.";
                }
                else
                {
                    response = ironRuntime.IronConsole.Execute(expression, extension);
                }
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
                response.StackTrace = ex.StackTrace;
            }
            finally
            {
                if (response == null)
                {
                    response = new IronConsoleResult();
                    response.Error = "Request timed out";
                }

                jsonResponse = response.ToJson();
            }

            context.Response.Write(jsonResponse);
        }
    }
}
