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
    public class IronWrapperControl : CompositeControl
    {
        public string ControlName { get; set; }
        private IronEngine _engine;
        private Control _control;
        private Exception Exception { get; set; }

        protected override void OnInit(EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(ControlName))
            {
                Exception = new InvalidOperationException("ControlName not set");
                return;
            }

            try
            {
                IronRuntime ironRuntime = IronRuntime.GetDefaultIronRuntime(SPContext.Current.Site);
                _engine = ironRuntime.GetEngineByExtension(".rb");
                _control = _engine.CreateDynamicInstance(ControlName) as Control;

                this.Controls.Add(_control);
            }
            catch (Exception ex)
            {
                Exception = ex;
            }

            base.OnInit(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            // Todo better error handling
            if (Exception != null)
            {
                string errorMessage = null;
                if (SPContext.Current.Web.UserIsSiteAdmin)
                {
                    var eo = _engine.ScriptEngine.GetService<ExceptionOperations>();
                    errorMessage = eo.FormatException(Exception);
                    IronRuntime.LogError(String.Format("Error creating control: {0}", ControlName), Exception);
                }
                else
                {
                    errorMessage = "An Error occured.";
                }
                writer.Write(errorMessage);
                return;
            }

            try
            {
                _control.RenderControl(writer);
            }
            catch (Exception ex)
            {
                writer.Write(ex.Message);
                IronRuntime.LogError("Error while rendering " + GetType().AssemblyQualifiedName, ex);
            }
        }     
    }
}
