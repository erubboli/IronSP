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
        protected IDynamicControl DynamicControl{get; private set;}

        protected override void OnInit(EventArgs e)
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

            try
            {
                Guid hiveId = String.IsNullOrEmpty(ScriptHiveId) ? Guid.Empty:new Guid(ScriptHiveId);

                var engine = IronRuntime.GetIronRuntime(SPContext.Current.Site, hiveId).GetEngineByExtension(Path.GetExtension(ScriptName));

                var ctrl = engine.CreateDynamicInstance(ScriptClass, ScriptName) as Control;

                DynamicControl = ctrl as IDynamicControl;
                if (DynamicControl != null)
                {
                    DynamicControl.Engine = engine;
                    DynamicControl.WebPart = this;
                    DynamicControl.Data = this;
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
