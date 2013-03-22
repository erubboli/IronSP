using System;
using System.Diagnostics.Contracts;
using System.Web.UI;
using System.Web.UI.WebControls;
using IronSharePoint.Diagnostics;
using IronSharePoint.Util;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;

namespace IronSharePoint
{
    public class IronWrapperControl : CompositeControl, IWrapperControl
    {
        private Control _control;
        public string ControlName { get; set; }
        public Exception InstantiationException { get; set; }

        protected override void OnInit(EventArgs e)
        {
            Contract.Requires<InvalidOperationException>(!String.IsNullOrWhiteSpace(ControlName), "ControlName not set");

            if (this.TryCreateDynamicControl(out _control))
            {
                Controls.Add(_control);
            }

            base.OnInit(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (InstantiationException != null)
            {
                HandleException(InstantiationException, writer);
            }
            else
            {
                try
                {
                    _control.RenderControl(writer);
                }
                catch (Exception ex)
                {
                    HandleException(ex, writer);
                }
            }
        }

        private void HandleException(Exception ex, HtmlTextWriter writer)
        {
            string output = IronULSLogger.Local.Error("Error in IronWrapperControl", ex,
                                                      IronCategoryDiagnosticsId.Controls);
            if (!SPContext.Current.Web.UserIsSiteAdmin)
            {
                writer.Write("An error occured.");
            }
            else
            {
                writer.Write(output);
            }
        }
    }
}