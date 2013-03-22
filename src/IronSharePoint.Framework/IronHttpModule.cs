using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IronSharePoint.Diagnostics;

namespace IronSharePoint
{
    public class IronHttpModule : IHttpModule
    {
        public void Init(HttpApplication application)
        {
            application.EndRequest += EndRequest;
            application.Error += Error;
        }

        public void Dispose()
        {
        }

        private void Error(object sender, EventArgs e)
        {
            var application = sender as HttpApplication;

            IronULSLogger.Local.Error("Error in HttpModule", application.Server.GetLastError(), IronCategoryDiagnosticsId.Core);
        }

        private void EndRequest(object sender, EventArgs e)
        {
            var application = sender as HttpApplication;

            CleanUp(application);
        }

        private static void CleanUp(HttpApplication application)
        {
            var ironObjectsToDispose = new List<IDisposable>();

            foreach (string key in application.Context.Items.Keys.OfType<String>())
            {
                if (key.StartsWith(IronConstant.IronPrefix))
                {
                    var disposableObj = application.Context.Items[key] as IDisposable;
                    if (disposableObj != null && !(disposableObj is IronRuntime))
                    {
                        ironObjectsToDispose.Add(disposableObj);
                    }
                }
            }

            ironObjectsToDispose.ForEach(o => o.Dispose());
        }
    }
}