using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using System.IO;
using System.Web.UI.WebControls;
using Microsoft.Scripting.Hosting.Providers;
using IronSharePoint.Diagnostics;
using Microsoft.SharePoint.Administration;
using System.Web.Caching;
using System.Web;

namespace IronSharePoint
{
    public class IronControl : CompositeControl
    {

        public string ScriptName { get; set; }

        private IronEngine _ironEngine;

        public IronEngine IronEngine
        {
            get { return _ironEngine; }
        }
       
        protected Exception exception;
        protected object dynOnInitControlsMethod;
        protected object dynOnLoadControlsMethod;
        protected object dynCreateChildControlsMethod;
        protected object dynOnPreRenderMethod;
        protected object dynRenderMethod;
        protected string Config;       
        public IIronDataStore DataStore { get; set; }

        public System.Web.UI.WebControls.WebParts.WebPart WebPart { get; set; }  

        protected override void OnInit(EventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(ScriptName))
                {
                    _ironEngine = IronEngine.GetEngine(Path.GetExtension(ScriptName), SPContext.Current.Web);
                               
                    _ironEngine.ScriptScope.SetVariable("controls", Controls);
                    _ironEngine.ScriptScope.SetVariable("page", this.Page);
                    _ironEngine.ScriptScope.SetVariable("parent", this.Parent);
                    _ironEngine.ScriptScope.SetVariable("httpCtx", HttpContext.Current);
                    _ironEngine.ScriptScope.SetVariable("request", HttpContext.Current.Request);
                    _ironEngine.ScriptScope.SetVariable("response", HttpContext.Current.Response);
                    _ironEngine.ScriptScope.SetVariable("store", DataStore);
                    _ironEngine.ScriptScope.SetVariable("config", Config);
                    _ironEngine.ScriptScope.SetVariable("webpart", WebPart);

                    if (Controls.Count > 0)
                    {
                        foreach (Control ctrl in Controls)
                        {
                            if (ctrl.ID != null)
                            {
                                _ironEngine.ScriptScope.SetVariable(ctrl.ID, ctrl);
                            }
                        }
                    }

                    var initialScriptFile = _ironEngine.GetFile(ScriptName);

                    if (initialScriptFile != null && initialScriptFile.Exists)
                    {
                        _ironEngine.ExcecuteScriptFile(initialScriptFile);

                        _ironEngine.InvokeDyamicMethodIfExists("on_init", e);
                    }
                }

            }
            catch (Exception ex)
            {
                IronEngine.LogError(String.Format("Error executing script {0}", ScriptName), ex);
                exception = ex;
            }

            base.OnInit(e);
        }
       

        protected override void OnLoad(EventArgs e)
        {
            if (!ShouldRun) return;

            _ironEngine.InvokeDyamicMethodIfExists("on_load", e);

            base.OnLoad(e);
        }

        protected override void CreateChildControls()
        {
            if (!ShouldRun) return;

            _ironEngine.InvokeDyamicMethodIfExists("create_child_controls");

            base.CreateChildControls();
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (!ShouldRun) return;

            _ironEngine.InvokeDyamicMethodIfExists("on_pre_render", e);

            base.OnPreRender(e);
        }


        protected override void Render(HtmlTextWriter writer)
        {
            if (exception != null)
            {
                writer.Write(exception.Message);
                return;
            }

            if (!ShouldRun)
            {
                if (Controls.Count > 0)
                {
                    base.Render(writer);
                }

                return;
            }

            object renderMethod = null;
            if (_ironEngine.ScriptScope.TryGetVariable("render", out renderMethod))
            {
                _ironEngine.InvokeDyamicMethodIfExists("render", writer);
                if (exception != null)
                {
                    writer.Write(exception.Message);
                }

            }
            else
            {
                base.Render(writer);
            }

        }
   

        public bool ShouldRun
        {
            get { return exception == null && !String.IsNullOrEmpty(ScriptName); }
        }

    }
}
