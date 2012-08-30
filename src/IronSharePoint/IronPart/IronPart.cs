using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Collections.Generic;
using IronRuby;
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

        [Personalizable(PersonalizationScope.Shared)]
        public string Data { get; set; }

        private Exception exception;

        private IDynamicControl dynamicControl;

        protected override void OnInit(EventArgs e)
        {

            if (String.IsNullOrEmpty(ScriptName))
            {
                exception = new InvalidEnumArgumentException("Property ScripName is empty!");
            }
            else if (String.IsNullOrEmpty(ScriptClass))
            {
                exception = new InvalidEnumArgumentException("Property ScripClass is empty!");
            }

            if (exception != null)
                return;

            try
            {
                var engine = IronRuntime.GetRuntime(SPContext.Current.Site).GetEngineByExtension(Path.GetExtension(ScriptName));

                var ctrl = engine.CreateDynamicInstance(ScriptClass, ScriptName) as Control;

                dynamicControl = ctrl as IDynamicControl;
                if (dynamicControl != null)
                {
                    dynamicControl.Engine = engine;
                    dynamicControl.WebPart = this;
                    dynamicControl.Data = this;
                }

                this.Controls.Add(ctrl);

                base.OnInit(e);
            }
            catch (Exception ex)
            {
                exception = ex;
                IronRuntime.LogError("IronWebPart Error", exception);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (exception != null)
            {
                writer.Write(exception.Message);
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
            if (dynamicControl != null)
            {
                 return new EditorPartCollection(base.CreateEditorParts(),dynamicControl.CreateEditorParts());
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
