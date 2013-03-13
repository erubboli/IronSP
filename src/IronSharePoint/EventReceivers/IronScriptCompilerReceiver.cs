﻿using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using System.IO;
using Microsoft.Scripting.Hosting;

namespace IronSharePoint.EventReceivers
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class IronScriptCompilerReceiver : SPItemEventReceiver
    {
        private IronEngine engine;

        public override void ItemAdded(SPItemEventProperties properties)
        {
            CompileScript(properties);
            
            base.ItemAdded(properties);
        }

        private void CompileScript(SPItemEventProperties properties)
        {
            
            if (!properties.ListItem.ContentTypeId.IsChildOf(new SPContentTypeId(IronContentTypeId.IronScript)))
                return;

            IronRuntime runtime = null;


            try
            {
                runtime = IronRuntime.GetDefaultIronRuntime(properties.Web.Site);
                engine =  runtime.GetEngineByExtension(Path.GetExtension(properties.ListItem.File.Name));

                EventFiringEnabled = false;
                properties.ListItem[FieldHelper.IronOutput] = engine.ExcecuteScriptFile(properties.ListItem.File);
                properties.ListItem[FieldHelper.IronErrorFlag] = false; 
                properties.ListItem.SystemUpdate(false);

                //cause compile bug??
               // runtime.IronHive.ReloadFiles();

            }
            catch (Exception ex)
            {
                var eo = engine.ScriptEngine.GetService<ExceptionOperations>();
                string error = eo.FormatException(ex);

                properties.ListItem[FieldHelper.IronOutput] = error;
                properties.ListItem[FieldHelper.IronErrorFlag] = true; 
                properties.ListItem.SystemUpdate(false);
            }
            finally
            {
                EventFiringEnabled = true;

                if (runtime != null)
                {
                    //runtime.IronHive.Dispose();
                }
            }
        }

        public override void ItemUpdated(SPItemEventProperties properties)
        {
            CompileScript(properties);

            base.ItemUpdated(properties);
        }
    }
}
