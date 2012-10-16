using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.SharePoint;
using IronSharePoint.IronLog;
using Microsoft.Scripting.Hosting;

namespace IronSharePoint
{
    public class IronHttpModule:IHttpModule
    {
       

        public void Init(HttpApplication application)
        {
            application.EndRequest += new EventHandler(EndRequest);
            application.Error += new EventHandler(Error);

        }

        void Error(object sender, EventArgs e)
        {
            var application = sender as HttpApplication;

            if (SPContext.Current != null)
            {
                var runtime = IronRuntime.GetDefaultIronRuntime(SPContext.Current.Site);

                var logger = new IronLogger(runtime);

                var exception =  application.Server.GetLastError();

                logger.Log(String.Format("Error: {0} at {1}!", exception.Message, exception.StackTrace), LogLevel.Fatal);

                //hack: ruby engine hard coded
                var engine = runtime.GetEngineByExtension(".rb");

                if (engine != null)
                {
                    var eo = engine.ScriptEngine.GetService<ExceptionOperations>();
                    string error = eo.FormatException(exception);

                    logger.Log(String.Format("Ruby Error: {0} at {1}", exception.Message, error),LogLevel.Fatal);

                }
            }
        }
        
        void EndRequest(object sender, EventArgs e)
        {
            var application = sender as HttpApplication;

            CleanUp(application);      
        }

        private static void CleanUp(HttpApplication application)
        {
            var ironObjectsToDispose = new List<IDisposable>();

            foreach (var key in application.Context.Items.Keys.OfType<String>())
            {
                if (key.StartsWith(IronConstant.IronPrefix))
                {
                    var disposableObj = application.Context.Items[key] as IDisposable;
                    if (disposableObj != null)
                    {
                        ironObjectsToDispose.Add(disposableObj);
                    }
                }
            }

            ironObjectsToDispose.ForEach(o=>o.Dispose());
        }

        public void Dispose() { }

    }
}
