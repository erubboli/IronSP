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

namespace IronSharePoint.IronPart
{
    [ToolboxItemAttribute(false)]
    public class IronPart : WebPart, IIronDataStore
    {
        [WebBrowsable(true)]
        [Category("IronPart")]
        [Personalizable(PersonalizationScope.Shared)]
        public string ScriptName { get; set; }

        [Personalizable(PersonalizationScope.Shared)]
        public string Data { get; set; }

        protected IronControl ironControl; 

        protected override void OnInit(EventArgs e)
        {
            if (!String.IsNullOrEmpty(ScriptName) && ScriptName.Contains("@"))
            {
                var engine = IronEngine.GetEngine(".rb", SPContext.Current.Web);
       

                var tmp = ScriptName.Split('@');
                var ctrlClassName = tmp[0];
                var scriptName = tmp[1];

                ctrlClassName = "DynamicControls.DynamicControl"; 

                var dynamicControl = engine.ScriptEngine.Execute(String.Format("defined?({0})?({0}.new):nil", ctrlClassName.Replace(".", "::"), engine.ScriptScope)) as Control;

                if (dynamicControl == null)
                {
                    var file = engine.GetFile(scriptName);
                    engine.ExcecuteScriptFile(file);
                    dynamicControl = engine.ScriptEngine.Execute(String.Format("defined?({0})?({0}.new):nil", ctrlClassName.Replace(".", "::")), engine.ScriptScope) as Control;
                }
               

                //ctrl.Engine = engine;
                this.Controls.Add(dynamicControl);

            }
            else
            {

                ironControl = new IronControl();
                ironControl.ScriptName = ScriptName;
                ironControl.DataStore = this;
                this.Controls.Add(ironControl);
            }

            base.OnInit(e);
        }


        public override EditorPartCollection CreateEditorParts()
        {

            if (!ironControl.ShouldRun) return base.CreateEditorParts();

            var editor = ironControl.IronEngine.InvokeDyamicMethodIfExists("create_editor_part") as EditorPart;

            if (editor == null)
            {
                return base.CreateEditorParts();
            }

    
            return new EditorPartCollection(base.CreateEditorParts(), new List<EditorPart>() { editor  });
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
    }
}
