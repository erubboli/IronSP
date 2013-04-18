using System;
using System.Linq;
using System.Web;
using IronSharePoint.Administration;
using IronSharePoint.Console;
using IronSharePoint.Hives;
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
                var site = SPContext.Current.Site;

                string error;
                if (!IsValidConsoleSite(site, out error))
                {
                    result.Error = error;
                }
                else
                {
                    IronRuntime ironRuntime = IronRuntime.GetDefaultIronRuntime(site);
                    string script = HttpContext.Current.Request["script"];

                    if (script.StartsWith("print_log")) script = "IronSP." + script;

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
                result.Error = ex.ToString();
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

        private bool IsValidConsoleSite(SPSite site, out string error)
        {
            RuntimeSetup runtimeSetup;
            error = null;
            if (IronRegistry.Local.TryResolveRuntime(site, out runtimeSetup))
            {
                if (site.RootWeb.CurrentUser == null
                    || !site.RootWeb.CurrentUser.IsSiteAdmin)
                {
                    error = "You do not have sufficient rights to use the IronConsole";
                }

                Func<bool> hasHive = () => runtimeSetup.Hives.Any(x => x.HiveType == typeof (SPDocumentHive) &&
                                                                       x.HiveArguments != null &&
                                                                       ((Guid) x.HiveArguments[0]) == site.ID);
                if (!hasHive())
                {
                    error = "Site is not a valid IronConsole site";
                }
            }
            else
            {
                error = "No runtime associated to this site";
            }
            return error == null;
        }
    }
}