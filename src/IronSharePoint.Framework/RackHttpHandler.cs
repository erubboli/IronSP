using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.SharePoint;

namespace IronSharePoint
{
    public class RackHttpHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext ctx)
        {
            var ironRuntime = IronRuntime.GetDefaultIronRuntime(SPContext.Current.Site);
            try
            {
                var rubyEngine = ironRuntime.RubyEngine;

                var scope = rubyEngine.CreateScope();
                scope.SetVariable("ctx", ctx);
                rubyEngine.Execute("IronSP::Routes.process(ctx)", scope);
            }
            catch (Exception ex)
            {
                ironRuntime.ULSLogger.Error("Error in RackHttpHandler", ex);
            }
            finally
            {
                //ctx.Response.End();
            }
        }

        public bool IsReusable { get { return true; } }
    }
}
