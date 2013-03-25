using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Administration;
using System.Collections.ObjectModel;
using System.Collections.Generic;

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
        private static readonly string modificationOwner = IronConstant.GetPrefixed("WebAppEventReceiver");

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                var webApp = properties.Feature.Parent as SPWebApplication;

                RegisterHttpModule(webApp);
                RegisterRackHttpHandler(webApp);

                /*Call Update and ApplyWebConfigModifications to save changes*/
                webApp.Update();
                webApp.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            });
        }

        private static void RegisterHttpModule(SPWebApplication webApp)
        {
            var httpModuleType = typeof(IronHttpModule);
            var mod = new SPWebConfigModification
                {
                    Path = "configuration/system.webServer/modules",
                    Name = String.Format("add[@name='IronHttpModule'][@type='{0}']", httpModuleType.AssemblyQualifiedName),
                    Sequence = 0,
                    Owner = modificationOwner,
                    Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode,
                    Value = String.Format("<add name='IronHttpModule' type='{0}' />", httpModuleType.AssemblyQualifiedName)
                };
            webApp.WebConfigModifications.Add(mod);
        }

        private static void RegisterRackHttpHandler(SPWebApplication webApp)
        {
            var httpHandlerType = typeof(RackHttpHandler);
            var rackMod = new SPWebConfigModification
                {
                    Path = "configuration/system.webServer/handlers",
                    Name = String.Format("add[@name='RackHttpHandler'][@type='{0}']", httpHandlerType.AssemblyQualifiedName),
                    Sequence = 0,
                    Owner = modificationOwner,
                    Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode,
                    Value = String.Format("<add name='RackHttpHandler' path='_iron/*' verb='*' type='{0}' /> ",
                        httpHandlerType.AssemblyQualifiedName)
                };
            var assetsMod = new SPWebConfigModification
            {
                Path = "configuration/system.webServer/handlers",
                Name = String.Format("add[@name='AssetsHttpHandler'][@type='{0}']", httpHandlerType.AssemblyQualifiedName),
                Sequence = 0,
                Owner = modificationOwner,
                Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode,
                Value = String.Format("<add name='AssetsHttpHandler' path='assets/*' verb='*' type='{0}' />",
                    httpHandlerType.AssemblyQualifiedName)
            };
            webApp.WebConfigModifications.Add(rackMod);
            webApp.WebConfigModifications.Add(assetsMod);
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                var webApplication = properties.Feature.Parent as SPWebApplication;
                var webMods = webApplication.WebConfigModifications;
                var modsToRemove = webMods.Where(mod => mod.Owner == modificationOwner).ToList();
                modsToRemove.ForEach(m => webMods.Remove(m));

                // Remove it and save the change to the configuration database  
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
