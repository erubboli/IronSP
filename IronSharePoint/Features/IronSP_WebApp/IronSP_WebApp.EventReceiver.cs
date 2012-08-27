using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Administration;
using System.Collections.ObjectModel;

namespace IronSharePoint.Features.IronSP_WebApp
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("62d325a0-90f5-4b80-a484-4681c4b7de5d")]
    public class IronSP_WebAppEventReceiver : SPFeatureReceiver
    {
        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                var t = typeof(IronExpressionBuilder);
                var webApp = properties.Feature.Parent as SPWebApplication;

                SPWebConfigModification myModification = new SPWebConfigModification();
                myModification.Path = "configuration/system.web/compilation/expressionBuilders";
                myModification.Name = String.Format("add[@expressionPrefix='Iron'][@type='{0}']",t.AssemblyQualifiedName);
                myModification.Sequence = 0;
                myModification.Owner = t.AssemblyQualifiedName;
                myModification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
                myModification.Value = String.Format("<add expressionPrefix='Iron' type='{0}' />", t.AssemblyQualifiedName);
                webApp.WebConfigModifications.Add(myModification);

                /*Call Update and ApplyWebConfigModifications to save changes*/
                webApp.Update();
                webApp.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            });

        }


        // Uncomment the method below to handle the event raised before a feature is deactivated.

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            SPSecurity.RunWithElevatedPrivileges(() => {

                var t = typeof(IronExpressionBuilder);

                SPWebConfigModification configModFound = null;
                SPWebApplication webApplication = properties.Feature.Parent as SPWebApplication;
                Collection<SPWebConfigModification> modsCollection = webApplication.WebConfigModifications;

                // Find the most recent modification of a specified owner
                int modsCount1 = modsCollection.Count;
                for (int i = modsCount1 - 1; i > -1; i--)
                {
                    if (modsCollection[i].Owner == t.AssemblyQualifiedName)
                    {
                        configModFound = modsCollection[i];
                    }
                }

                // Remove it and save the change to the configuration database  
                modsCollection.Remove(configModFound);
                webApplication.Update();

                // Reapply all the configuration modifications
                webApplication.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications();

            });
        }


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}
    }
}
