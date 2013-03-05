using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.SharePoint;
using System.IO;

namespace IronSharePoint
{
    public class IronHttpHandler : IHttpHandler
    {

        private IHttpHandler _dynamicHandler;

        public IHttpHandler DynamicHandler
        {
            get 
            {
                if (_dynamicHandler == null)
                {
                    var ironRuntime = IronRuntime.GetDefaultIronRuntime(SPContext.Current.Site);

                    if (!String.IsNullOrEmpty(ironRuntime.HttpHandlerClass))
                    {
                        _dynamicHandler = ironRuntime.CreateDynamicInstance(ironRuntime.HttpHandlerClass) as IHttpHandler;
  
                    }
                    else
                    {

                        throw new InvalidOperationException("No dynamic HttpHandlerFactory registered");
                    }
                }

                return _dynamicHandler; 
            }
        }



        public bool IsReusable
        {
            get { return DynamicHandler.IsReusable; }
        }

        public void ProcessRequest(HttpContext context)
        {
            DynamicHandler.ProcessRequest(context);
        }

    }
}
