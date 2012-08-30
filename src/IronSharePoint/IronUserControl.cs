﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using IronSharePoint.Diagnostics;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;
using System.ComponentModel;
using System.IO;

namespace IronSharePoint
{
    public class IronUserControl : UserControl
    {
        [TemplateContainer(typeof(IronControl))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public ITemplate Template { get; set; }

        public string TemplatePath { get; set; }

        public string ScriptName { get; set; }

        public string ScriptClass { get; set; }

        private Exception _exception;

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

                var engine = IronRuntime.GetIronRuntime(SPContext.Current.Site.ID).GetEngineByExtension(Path.GetExtension(ScriptName));

                var ctrl = engine.CreateDynamicInstance(ScriptClass, ScriptName) as Control;

                var dynamicControl = ctrl as IDynamicControl;
                if (dynamicControl != null)
                {
                    dynamicControl.Engine = engine;
                    dynamicControl.WebPart = null;
                    dynamicControl.Data = null;
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
                            .Replace("~hiveSite", engine.IronRuntime.IronHost.HiveSite.ServerRelativeUrl)
                            .Replace("~hiveWeb", engine.IronRuntime.IronHost.HiveWeb.ServerRelativeUrl)
                            .Replace("~hiveFolder", engine.IronRuntime.IronHost.HiveFolder.ServerRelativeUrl);

                        Template = this.LoadTemplate(path);
                        Template.InstantiateIn(ctrl);
                    }
                }

                this.Controls.Add(ctrl);
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
                writer.Write(_exception.Message);
                return;
            }
            try
            {
                base.Render(writer);
            }
            catch (Exception ex)
            {
                IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Controls], TraceSeverity.Unexpected, String.Format("Error: {0}; Stack: {1}", ex.Message, ex.StackTrace));
                writer.Write(ex.Message);
            }
        }
    }
}
