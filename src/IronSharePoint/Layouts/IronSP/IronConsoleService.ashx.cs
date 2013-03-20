using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using IronSharePoint.Administration;
using IronSharePoint.Console;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web;
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
            var result = new ScriptResult();
            string jsonResponse;

            try
            {
                var site = SPContext.Current.Site;
                var web = SPContext.Current.Web;

                if (!web.CurrentUser.IsSiteAdmin)
                {
                    result.Error = "Only Site Admins are allowed to use the Console";
                }
                else
                {

                }
                var ironRuntime = IronRuntime.GetDefaultIronRuntime(site);
                var languageName = HttpContext.Current.Request["lang"];
                var script = HttpContext.Current.Request["script"];

                if (script == "kill")
                {
                    ironRuntime.Dispose();
                    result.Output = "Runtime disposed.";
                }
                else
                {
                    result = ironRuntime.Console.Execute(script, languageName).Result;
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                result.StackTrace = ex.StackTrace;
            }
            finally
            {
                if (result == null)
                {
                    result = new ScriptResult {Error = "Request timed out"};
                }

                jsonResponse = result.ToJson();
            }

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            context.Response.Write(jsonResponse);
        }
    }
}
