using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using System.IO;

namespace IronSharePoint.EventReceivers
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class IronScriptCompilerReceiver : SPItemEventReceiver
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

            try
            {
                var engine =  IronRuntime.GetDefaultIronRuntime(properties.Web.Site).GetEngineByExtension(Path.GetExtension(properties.ListItem.File.Name));

                EventFiringEnabled = false;
                properties.ListItem[IronField.IronOutput] = engine.ExcecuteScriptFile(properties.ListItem.File);
                properties.ListItem[IronField.IronErrorFlag] = false; 
                properties.ListItem.SystemUpdate(false);
            }
            catch (Exception ex)
            {
                properties.ListItem[IronField.IronOutput] = ex.Message + Environment.NewLine + ex.StackTrace;
                properties.ListItem[IronField.IronErrorFlag] = true; 
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
