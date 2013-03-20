using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using IronSharePoint.Diagnostics;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;
using System.ComponentModel;
using System.IO;
using Microsoft.Scripting.Hosting;

namespace IronSharePoint
{
    public class IronWrapperUserControl : UserControl
    {
        [TemplateContainer(typeof(IronWrapperControl))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public ITemplate Template { get; set; }

        public string TemplatePath { get; set; }

        public string ScriptName { get; set; }
        public string ScriptClass { get; set; }
        public string ScriptHiveId { get; set; }

        public string Config { get; set; }

        private Exception _exception;
        private ScriptEngine _scriptEngine;
        protected Control _ctrl;

        protected override void OnInit(EventArgs e)
        {
            try
            {

                if (String.IsNullOrEmpty(ScriptName))
                {
                    _exception = new InvalidEnumArgumentException("Property ScripName is empty!");
                }
                else if (String.IsNullOrEmpty(ScriptClass))
                {
                    _exception = new InvalidEnumArgumentException("Property ScripClass is empty!");
                }

                if (_exception != null)
                    return;

                Guid hiveId = String.IsNullOrEmpty(ScriptHiveId) ? Guid.Empty : new Guid(ScriptHiveId);

                //IronRuntime ironRuntime = IronRuntime.GetIronRuntime(SPContext.Current.Site, hiveId);
                IronRuntime ironRuntime = IronRuntime.GetDefaultIronRuntime(SPContext.Current.Site);
                _scriptEngine = ironRuntime.ScriptRuntime.GetEngineByFileExtension(Path.GetExtension(ScriptName));

                if (_scriptEngine != null)
                {
                    _ctrl = null;// TODO _scriptEngine.CreateDynamicInstance(ScriptClass, ScriptName) as Control;

                    var dynamicControl = _ctrl as IIronControl;
                    if (dynamicControl != null)
                    {
                        dynamicControl.WebPart = null;
                        dynamicControl.DataStore = null;
                    }

                    if (Template != null)
                    {
                        Template.InstantiateIn(_ctrl);
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(TemplatePath))
                        {
                            var path = TemplatePath.Replace("~site", SPContext.Current.Site.ServerRelativeUrl)
                                                   .Replace("~web", SPContext.Current.Web.ServerRelativeUrl);

                            Template = this.LoadTemplate(path);
                            Template.InstantiateIn(_ctrl);
                        }
                    }

                    this.Controls.Add(_ctrl);
                }
            }
            catch (Exception ex)
            {
                IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Controls], TraceSeverity.Unexpected, String.Format("Error: {0}; Stack: {1}", ex.Message, ex.StackTrace));
                _exception = ex;
            }

            base.OnInit(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            // TODO better error handling
            if (_exception!=null)
            {
                if (SPContext.Current.Web.UserIsSiteAdmin)
                {
                    var eo = _scriptEngine.GetService<ExceptionOperations>();
                    string error = eo.FormatException(_exception);

                    IronRuntime.LogError(String.Format("Error executing script {0}: {1}", ScriptName, error), _exception);

                    //if (engine != null)
                    //{
                    //    new IronLogger(engine.IronRuntime).Log(String.Format("Ruby Error: {0} at {1}", _exception.Message, error));
                    //}

                    writer.Write(error);
                }
                else
                {
                    writer.Write("Error occured.");
                }
            }
            try
            {
                _ctrl.RenderControl(writer);
            }
            catch (Exception ex)
            {
                IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Controls], TraceSeverity.Unexpected, String.Format("Error: {0}; Stack: {1}", ex.Message, ex.StackTrace));
                writer.Write(ex.Message);
            }
        }
    }
}
