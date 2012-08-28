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
            try
            {
                var engine = IronEngine.GetEngineByExtension(properties.Web.Site, Path.GetExtension(properties.ListItem.File.Name));

                EventFiringEnabled = false;
                properties.ListItem[IronFields.IronOutput] = engine.ExcecuteScriptFile(properties.ListItem.File);
                properties.ListItem[IronFields.IronErrorFlag] = false; 
                properties.ListItem.SystemUpdate(false);
            }
            catch (Exception ex)
            {
                properties.ListItem[IronFields.IronOutput] = ex.Message + Environment.NewLine + ex.StackTrace;
                properties.ListItem[IronFields.IronErrorFlag] = true; 
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
