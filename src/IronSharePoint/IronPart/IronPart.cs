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

        private Exception _exception;
        private IDynamicControl _dynamicControl;

        protected override void OnInit(EventArgs e)
        {
            if (String.IsNullOrEmpty(ScriptName))
            {
                _exception = new InvalidEnumArgumentException("Property ScriptName is empty!");
            }
            else if (String.IsNullOrEmpty(ScriptClass))
            {
                _exception = new InvalidEnumArgumentException("Property ScriptClass is empty!");
            }

            if (_exception != null)
                return;

            try
            {
                var engine = IronEngine.GetEngineByExtension(SPContext.Current.Web.Site, Path.GetExtension(ScriptName));

                var ctrl = engine.CreateDynamicInstance(ScriptClass, ScriptName) as Control;

                _dynamicControl = ctrl as IDynamicControl;
                if (_dynamicControl != null)
                {
                    _dynamicControl.Engine = engine;
                    _dynamicControl.WebPart = this;
                    _dynamicControl.Data = this;
                }

                this.Controls.Add(ctrl);

                base.OnInit(e);
            }
            catch (Exception ex)
            {
                _exception = ex;
                IronEngine.LogError("IronWebPart Error", _exception);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (_exception != null)
            {
                writer.Write(_exception.Message);
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
                    IronEngine.LogError("Error", ex);
                }
            }
        }

        
        public override EditorPartCollection CreateEditorParts()
        {
            if (_dynamicControl != null)
            {
                 return new EditorPartCollection(base.CreateEditorParts(),_dynamicControl.CreateEditorParts());
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
