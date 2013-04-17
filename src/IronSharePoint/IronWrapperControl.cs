using System;
using System.Diagnostics.Contracts;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IronSharePoint.Diagnostics;
using IronSharePoint.Util;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using System.Linq;

namespace IronSharePoint
{
    public class IronWrapperControl : CompositeControl, IWrapperControl
    {
        private Control _control;
        private string _controlName; 
       
        public string ControlName
        {
            get 
            {
                return _controlName ?? ScriptClass.Replace(".", "::");
            }
            set
            {
                _controlName = value;
            }
        }
        public Exception InstantiationException { get; set; }
        public string ScriptClass { get; set; }

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
                var lines = output.Split('\n').Select(HttpUtility.HtmlEncode).ToArray();
                var outputHtml = string.Format(@"
                    <div class='ironsp-error' style='display: inline-block; max-height: 100px; width: 100%; max-width: 600px; overflow-y: scroll; overflow-x: hidden; border: 2px solid red; font-size: 10px;line-height: 110%; text-align: left;'>
                      <p style='margin: 3px;'>{0}</p>
                      <ul style='padding-left: 20px; margin-bottom: 0px;'>{1}</ul>
                    </div>
                ", lines.FirstOrDefault(), lines.Skip(1).Select(x => "<li>" + x + "</li>").StringJoin("\n"));

                writer.Write(outputHtml);
            }
        }
    }
}