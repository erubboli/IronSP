using System;
using System.Diagnostics.Contracts;
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
        public string ControlName { get; set; }
        public Exception InstantiationException { get; set; }

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
                writer.Write(output);
            }
        }
    }
}