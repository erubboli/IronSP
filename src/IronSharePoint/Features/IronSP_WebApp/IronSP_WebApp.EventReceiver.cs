using System;
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

                RegisterExpressionBuilder(webApp);

                RegisterHttpModule(webApp);

                RegisterHttpHandlerFactory(webApp);

                /*Call Update and ApplyWebConfigModifications to save changes*/
                webApp.Update();
                webApp.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            });

        }

        private static void RegisterHttpModule(SPWebApplication webApp)
        {
            var httpModuleType = typeof(IronHttpModule);
            SPWebConfigModification httpModuleMod = new SPWebConfigModification();
            httpModuleMod.Path = "configuration/system.webServer/modules";
            httpModuleMod.Name = String.Format("add[@name='IronHttpModule'][@type='{0}']", httpModuleType.AssemblyQualifiedName);
            httpModuleMod.Sequence = 0;
            httpModuleMod.Owner = modificationOwner;
            httpModuleMod.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            httpModuleMod.Value = String.Format("<add name='IronHttpModule' type='{0}' />", httpModuleType.AssemblyQualifiedName);
            webApp.WebConfigModifications.Add(httpModuleMod);
        }

        private static void RegisterHttpHandlerFactory(SPWebApplication webApp)
        {
            var httpHandlerType = typeof(IronHttpHandler);
            SPWebConfigModification httpHandlerFacotry = new SPWebConfigModification();
            httpHandlerFacotry.Path = "configuration/system.webServer/handlers";
            httpHandlerFacotry.Name = String.Format("add[@type='{0}']", httpHandlerType.AssemblyQualifiedName);
            httpHandlerFacotry.Sequence = 0;
            httpHandlerFacotry.Owner = modificationOwner;
            httpHandlerFacotry.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            httpHandlerFacotry.Value = String.Format("<add  name='IronHttpHandler' path='_iron/*' verb='*' type='{0}' />", httpHandlerType.AssemblyQualifiedName);
            webApp.WebConfigModifications.Add(httpHandlerFacotry);
        }

        private static void RegisterExpressionBuilder(SPWebApplication webApp)
        {
            var expressionBuilderType = typeof(IronExpressionBuilder);
            SPWebConfigModification expressionBuilderMod = new SPWebConfigModification();
            expressionBuilderMod.Path = "configuration/system.web/compilation/expressionBuilders";
            expressionBuilderMod.Name = String.Format("add[@expressionPrefix='Iron'][@type='{0}']", expressionBuilderType.AssemblyQualifiedName);
            expressionBuilderMod.Sequence = 0;
            expressionBuilderMod.Owner = modificationOwner;
            expressionBuilderMod.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            expressionBuilderMod.Value = String.Format("<add expressionPrefix='Iron' type='{0}' />", expressionBuilderType.AssemblyQualifiedName);
            webApp.WebConfigModifications.Add(expressionBuilderMod);
        }


        // Uncomment the method below to handle the event raised before a feature is deactivated.

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            SPSecurity.RunWithElevatedPrivileges(() => {

                var webApplication = properties.Feature.Parent as SPWebApplication;
                var webMods = webApplication.WebConfigModifications;
                var modsToRemove = new List<SPWebConfigModification>();

                foreach(var mod in webMods)
                {
                    if (mod.Owner == modificationOwner)
                    {
                        modsToRemove.Add(mod);
                    }
                }

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
