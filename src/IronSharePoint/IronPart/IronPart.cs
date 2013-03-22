using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using IronSharePoint.Diagnostics;
using IronSharePoint.Util;
using Microsoft.SharePoint;

namespace IronSharePoint.IronPart
{
    [ToolboxItem(false)]
    public class IronPart : WebPart, IIronDataStore, IWrapperControl
    {
        protected Exception Exception;
        protected Control InnerControl;

        [Personalizable(PersonalizationScope.Shared)]
        public string Data { get; set; }

        public void SaveData(string data)
        {
            Data = data;
            SetPersonalizationDirty();
        }

        public string GetData()
        {
            return Data;
        }

        [WebBrowsable(true)]
        [Category("IronPart")]
        [Personalizable(PersonalizationScope.Shared)]
        public string ControlName { get; set; }

        public Exception InstantiationException { get; set; }

        protected override void OnInit(EventArgs e)
        {
            Contract.Requires<InvalidOperationException>(!String.IsNullOrWhiteSpace(ControlName), "ControlName not set");

            if (!this.TryCreateDynamicControl(out InnerControl))
            {
                if (InnerControl is IIronControl)
                {
                    var ironControl = InnerControl as IIronControl;
                    ironControl.WebPart = this;
                    ironControl.DataStore = this;
                }
            }
            Controls.Add(InnerControl);
            base.OnInit(e);
        }

        public new void SetPersonalizationDirty()
        {
            base.SetPersonalizationDirty();
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
                    InnerControl.RenderControl(writer);
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
                                                      IronCategoryDiagnosticsId.WebParts);
            if (!SPContext.Current.Web.UserIsSiteAdmin)
            {
                writer.Write("An error occured.");
            }
            else
            {
                writer.Write(output);
            }
        }

        public override EditorPartCollection CreateEditorParts()
        {
            if (InnerControl is IIronControl)
            {
                var ironControl = InnerControl as IIronControl;
                return new EditorPartCollection(base.CreateEditorParts(), ironControl.CreateEditorParts());
            }

            return base.CreateEditorParts();
        }
    }
}