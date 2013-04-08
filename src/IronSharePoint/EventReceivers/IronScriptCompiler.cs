using System;
using System.Security.Permissions;
using IronSharePoint.Hives;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using System.IO;
using Microsoft.Scripting.Hosting;
using IronSharePoint.Util;

namespace IronSharePoint.EventReceivers
{
    /// <summary>
    /// Compiles created or updated IronScripts
    /// </summary>
    public class IronScriptCompiler : SPItemEventReceiver
    {
        public override void ItemAdded(SPItemEventProperties properties)
        {
            CompileScript(properties);
            
            base.ItemAdded(properties);
        }

        private void CompileScript(SPItemEventProperties properties)
        {
            if (!properties.ListItem.ContentTypeId.IsChildOf(new SPContentTypeId(IronContentTypeId.IronScript)))
                return;

            var runtime = IronRuntime.GetDefaultIronRuntime(properties.Web.Site);
            var engine = runtime.RubyEngine;

            try
            {
                EventFiringEnabled = false;
                properties.ListItem[FieldHelper.IronOutput] = engine.ExecuteSPFile(properties.ListItem.File);
                properties.ListItem[FieldHelper.IronErrorFlag] = false; 
                properties.ListItem.SystemUpdate(false);
            }
            catch (Exception ex)
            {
                var eo = engine.GetService<ExceptionOperations>();
                string error = eo.FormatException(ex);

                properties.ListItem[FieldHelper.IronOutput] = error;
                properties.ListItem[FieldHelper.IronErrorFlag] = true; 
                properties.ListItem.SystemUpdate(false);
            }
            finally
            {
                EventFiringEnabled = true;
            }
        }

        public override void ItemUpdated(SPItemEventProperties properties)
        {
            CompileScript(properties);

            base.ItemUpdated(properties);
        }
    }
}
