using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using IronSharePoint.Diagnostics;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;

namespace IronSharePoint
{
    public class IronUserControl : UserControl
    {
        [TemplateContainer(typeof(IronControl))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public ITemplate Template { get; set; }

        public string TemplatePath { get; set; }

        public string ScriptName { get; set; }

        private Exception exception;

        protected override void OnInit(EventArgs e)
        {
            try
            {

                var ironControl = new IronControl();
                ironControl.ScriptName = ScriptName;

                if (Template != null)
                {
                    Template.InstantiateIn(ironControl);
                }
                else
                {
                    if (!String.IsNullOrEmpty(TemplatePath))
                    {
                        var path = TemplatePath.Replace("~site", SPContext.Current.Site.ServerRelativeUrl).Replace("~web", SPContext.Current.Web.ServerRelativeUrl);
                        
                        Template = this.LoadTemplate(path);
                        Template.InstantiateIn(ironControl);
                    }
                }

                Controls.Add(ironControl);
            }
            catch (Exception ex)
            {
                IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Controls], TraceSeverity.Unexpected, String.Format("Error: {0}; Stack: {1}", ex.Message, ex.StackTrace));
                exception = ex;
            }

            base.OnInit(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (exception!=null)
            {
                writer.Write(exception.Message);
                return;
            }

            base.Render(writer);
        }
    }
}
