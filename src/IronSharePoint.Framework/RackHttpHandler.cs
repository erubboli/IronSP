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
    class RackHttpHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext ctx)
        {
            var ironRuntime = IronRuntime.GetDefaultIronRuntime(SPContext.Current.Site);
            var rubyEngine = ironRuntime.RubyEngine;

            var scope = rubyEngine.CreateScope();
            scope.SetVariable("ctx", ctx);
            rubyEngine.Execute("Rack::Handler::IIS.process(ctx)", scope);
        }

        public bool IsReusable { get { return true; } }
    }
}
