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

        private IronEngine engine;

        protected Control ctrl;

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

                IronRuntime ironRuntime = IronRuntime.GetIronRuntime(SPContext.Current.Site, hiveId);
                engine = ironRuntime.GetEngineByExtension(Path.GetExtension(ScriptName));

                if (engine != null)
                {
                    ctrl = engine.CreateDynamicInstance(ScriptClass, ScriptName) as Control;

                    var dynamicControl = ctrl as IIronControl;
                    if (dynamicControl != null)
                    {
                        dynamicControl.WebPart = null;
                        dynamicControl.Data = null;
                        dynamicControl.Config = Config;
                    }

                    if (Template != null)
                    {
                        Template.InstantiateIn(ctrl);
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(TemplatePath))
                        {
                            var path = TemplatePath.Replace("~site", SPContext.Current.Site.ServerRelativeUrl)
                                .Replace("~web", SPContext.Current.Web.ServerRelativeUrl)
                                .Replace("~hiveSite", engine.IronRuntime.IronHive.Site.ServerRelativeUrl)
                                .Replace("~hiveWeb", engine.IronRuntime.IronHive.Web.ServerRelativeUrl)
                                .Replace("~hiveFolder", engine.IronRuntime.IronHive.Folder.ServerRelativeUrl);

                            Template = this.LoadTemplate(path);
                            Template.InstantiateIn(ctrl);
                        }
                    }

                    this.Controls.Add(ctrl);
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
            if (_exception!=null)
            {
                if (SPContext.Current.Web.UserIsSiteAdmin && engine.IronRuntime.IronHive.Web.CurrentUser.IsSiteAdmin)
                {
                    var eo = engine.ScriptEngine.GetService<ExceptionOperations>();
                    string error = eo.FormatException(_exception);

                    IronRuntime.LogError(String.Format("Error executing script {0}: {1}", ScriptName, error), _exception);

                    writer.Write(error);
                }
                else
                {
                    writer.Write("Error occured.");
                }
            }
            try
            {
                ctrl.RenderControl(writer);
            }
            catch (Exception ex)
            {
                IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Controls], TraceSeverity.Unexpected, String.Format("Error: {0}; Stack: {1}", ex.Message, ex.StackTrace));
                writer.Write(ex.Message);
            }
        }
    }
}
