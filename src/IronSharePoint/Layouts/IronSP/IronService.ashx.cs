using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web;
using IronSharePoint.Administration;
using System.IO;

namespace IronSharePoint
{
    public partial class IronService : IHttpHandler
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

                var ironRuntime = IronRuntime.GetDefaultIronRuntime(site);

                var scriptName = HttpContext.Current.Request["scriptName"];
                var scriptClass = HttpContext.Current.Request["scriptClass"];
                var extension = Path.GetExtension(scriptName);

                var engine = ironRuntime.GetEngineByExtension(extension);

                var dynamicHandler = engine.CreateDynamicInstance(scriptClass, scriptName) as IHttpHandler;
                dynamicHandler.ProcessRequest(context);


            }
            catch (Exception ex)
            {
                output = ex.Message;
            }

            context.Response.Write(output);
        }
    }
}
