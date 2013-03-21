using System;
using System.Web;
using IronSharePoint.Console;
using Microsoft.SharePoint;

namespace IronSharePoint
{
    public class IronConsoleService : IHttpHandler
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
                SPSite site = SPContext.Current.Site;
                SPWeb web = SPContext.Current.Web;

                if (!web.CurrentUser.IsSiteAdmin)
                {
                    result.Error = "Only Site Admins are allowed to use the Console";
                }
                else
                {
                    IronRuntime ironRuntime = IronRuntime.GetDefaultIronRuntime(site);
                    string script = HttpContext.Current.Request["script"];

                    if (script == "kill")
                    {
                        ironRuntime.Dispose();
                        result.Output = "Runtime disposed.";
                    }
                    else
                    {
                        result = ironRuntime.Console.Execute(script).Result;
                    }
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