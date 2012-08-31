using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace IronSharePoint
{
    public class IronHttpModule:IHttpModule
    {
       

        public void Init(HttpApplication application)
        {
            application.EndRequest += new EventHandler(EndRequest);

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
