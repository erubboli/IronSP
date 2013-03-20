using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Collections.Generic;
using Microsoft.Scripting.Hosting;
using System.IO;
using IronSharePoint.Util;

namespace IronSharePoint.IronPart
{
    [ToolboxItemAttribute(false)]
    public class IronPart : WebPart, IIronDataStore
    {
        [WebBrowsable(true)]
        [Category("IronPart")]
        [Personalizable(PersonalizationScope.Shared)]
        public string ScriptName { get; set; }

        [WebBrowsable(true)]
        [Category("IronPart")]
        [Personalizable(PersonalizationScope.Shared)]
        public string ScriptClass { get; set; }

        [WebBrowsable(false)]
        [Category("IronPart")]
        [Personalizable(PersonalizationScope.Shared)]
        public String ScriptHiveId { get; set; }

        [Personalizable(PersonalizationScope.Shared)]
        public string Data { get; set; }

        protected Exception Exception {get; set;}
        protected IIronControl DynamicControl{get; private set;}
        private IronRuntime ironRuntime;

        protected override void OnInit(EventArgs e)
        {
            Guid hiveId = String.IsNullOrEmpty(ScriptHiveId) ? Guid.Empty : new Guid(ScriptHiveId);

            //ironRuntime = IronRuntime.GetIronRuntime(SPContext.Current.Site, hiveId);
            ironRuntime = IronRuntime.GetDefaultIronRuntime(SPContext.Current.Site);

            if (String.IsNullOrEmpty(ScriptClass))
            {
                Exception = new InvalidEnumArgumentException("Property ScriptClass is empty!");
            }

            if (Exception != null)
                return;

            try
            {

                Control ctrl = null;
                if (!String.IsNullOrEmpty(ScriptName))
                {
                    var engine = ironRuntime.ScriptRuntime.GetEngineByFileExtension(Path.GetExtension(ScriptName));
                    ctrl = engine.CreateInstance(ScriptClass) as Control;
                }
                else
                {
                    ctrl = ironRuntime.CreateDynamicInstance(ScriptClass) as Control;
                }

                DynamicControl = ctrl as IIronControl;
                if (DynamicControl != null)
                {
                    DynamicControl.WebPart = this;
                    DynamicControl.DataStore = this;
                }


                this.Controls.Add(ctrl);

                base.OnInit(e);
            }
            catch (Exception ex)
            {
                Exception = ex;
                IronRuntime.LogError("IronWebPart Error", Exception);
            }
        }

        public new void SetPersonalizationDirty(){
            base.SetPersonalizationDirty();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (Exception != null)
            {
                if (SPContext.Current.Web.UserIsSiteAdmin)
                {
                    IronRuntime.LogError(String.Format("Script: {0}, Error: {1}", ScriptName, Exception.Message),
                                         Exception);

                    writer.Write(Exception.Message);
                }
                else
                {
                    writer.Write("Error occured.");
                }
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

        
        public override EditorPartCollection CreateEditorParts()
        {
            if (DynamicControl != null)
            {
                 return new EditorPartCollection(base.CreateEditorParts(),DynamicControl.CreateEditorParts());
            }

            return base.CreateEditorParts();
        }
       

        public void SaveData(string data)
        {
            Data = data;
            SetPersonalizationDirty();
        }

        public string GetData()
        {
            return Data;
        }

        public string PathScriptName { get; set; }
    }
}
