using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Security;
using IronSharePoint.EventReceivers;
using System.Linq;

namespace IronSharePoint.Features.IronSP_Hive_Site
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("a78520f4-de3a-446f-96f5-80f3c9b741df")]
    public class IronSP_Hive_SiteEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            // TODO fix event receivers
            //var site = properties.Feature.Parent as SPSite;
            //var web = site.RootWeb;
            //var list = web.GetList(web.ServerRelativeUrl + "/" + IronConstant.IronHiveLibraryPath);

            //string assembly = typeof(IronHiveEventReceiver).Assembly.FullName;
            //string type = typeof(IronHiveEventReceiver).FullName;

            //list.EventReceivers.Add(SPEventReceiverType.ItemAdded, assembly, type);
            //list.EventReceivers.Add(SPEventReceiverType.ItemUpdated, assembly, type);
            //list.EventReceivers.Add(SPEventReceiverType.ItemDeleted, assembly, type);
            //list.EventReceivers.Add(SPEventReceiverType.ItemFileMoved, assembly, type);
            //list.EventReceivers.Add(SPEventReceiverType.ItemCheckedIn, assembly, type);
            //list.EventReceivers.Add(SPEventReceiverType.ItemAdding, assembly, type);
            //list.EventReceivers.Add(SPEventReceiverType.ItemUpdating, assembly, type);
            //list.EventReceivers.Add(SPEventReceiverType.ItemDeleting, assembly, type);
            //list.EventReceivers.Add(SPEventReceiverType.ItemFileMoving, assembly, type);
            //list.EventReceivers.Add(SPEventReceiverType.ItemCheckingIn, assembly, type);
            //list.EventReceivers.Add(SPEventReceiverType.ItemCheckingOut, assembly, type);

            //list.Update();
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            // TODO fix event receivers
            //var site = properties.Feature.Parent as SPSite;
            //var web = site.RootWeb;
            //var list = web.GetList(web.ServerRelativeUrl + "/" + IronConstant.IronHiveLibraryPath);

            //string type = typeof(IronHiveEventReceiver).FullName;

            //list.EventReceivers.OfType<SPEventReceiverDefinition>().Where(d=>d.Class == type).ToList().ForEach(d=>d.Delete());
            //list.Update();
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
