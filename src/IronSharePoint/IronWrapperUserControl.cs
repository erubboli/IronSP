using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Web.UI;
using IronSharePoint.Diagnostics;
using IronSharePoint.Util;
using Microsoft.SharePoint;

namespace IronSharePoint
{
    public class IronWrapperUserControl : UserControl, IWrapperControl
    {
        protected Control _control;

        [TemplateContainer(typeof (IronWrapperControl))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public ITemplate Template { get; set; }

        public string TemplatePath { get; set; }
        public string Config { get; set; }
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
            Contract.Requires<InvalidOperationException>(!String.IsNullOrWhiteSpace(ControlName),
                                                         "ControlName not set");

            if (this.TryCreateDynamicControl(out _control))
            {
                if (_control is IIronControl)
                {
                    var ironControl = _control as IIronControl;
                    ironControl.WebPart = null;
                    ironControl.DataStore = null;
                }
                if (Template != null)
                {
                    Template.InstantiateIn(_control);
                }
                else if (!String.IsNullOrEmpty(TemplatePath))
                {
                    string path = TemplatePath.Replace("~site", SPContext.Current.Site.ServerRelativeUrl)
                                              .Replace("~web", SPContext.Current.Web.ServerRelativeUrl);

                    Template = LoadTemplate(path);
                    Template.InstantiateIn(_control);
                }

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