using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using System.IO;
using System.Web.UI.WebControls;
using Microsoft.Scripting.Hosting.Providers;
using IronSharePoint.Diagnostics;
using Microsoft.SharePoint.Administration;
using System.Web.Caching;
using System.Web;
using System.ComponentModel;

namespace IronSharePoint
{
    public class IronControl : CompositeControl
    {
        public string ScriptName { get; set; }

        public string ScriptClass { get; set; }

        public IIronDataStore DataStore { get; set; }

        protected IronEngine engine;
      
        protected System.Web.UI.WebControls.WebParts.WebPart WebPart { get; set; }
        protected Exception Exception { get; set; }

        protected override void OnInit(EventArgs e)
        {
            try
            {
                if (String.IsNullOrEmpty(ScriptName))
                {
                    Exception = new InvalidEnumArgumentException("Property ScriptName is empty!");
                }
                else if (String.IsNullOrEmpty(ScriptClass))
                {
                    Exception = new InvalidEnumArgumentException("Property ScriptClass is empty!");
                }

                if (Exception != null)
                    return;

                var engine = IronRuntime.GetIronRuntime(SPContext.Current.Site.ID).GetEngineByExtension(Path.GetExtension(ScriptName));

                var ctrl = engine.CreateDynamicInstance(ScriptClass, ScriptName) as Control;

                var dynamicControl = ctrl as IDynamicControl;
                if (dynamicControl != null)
                {
                    dynamicControl.Engine = engine;
                    dynamicControl.WebPart = null;
                    dynamicControl.Data = null;
                }

                this.Controls.Add(ctrl); 

            }
            catch (Exception ex)
            {
                IronRuntime.LogError(String.Format("Error executing script {0}", ScriptName), ex);
                Exception = ex;
            }

            base.OnInit(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (Exception != null)
            {
                writer.Write(Exception.Message);
            }
            else
            {
                try
                {
                    base.Render(writer);
                }
                catch (Exception ex)
                {
                    writer.Write(ex.Message);
                    IronRuntime.LogError("Error", ex);
                }
            }
        }     
    }
}
