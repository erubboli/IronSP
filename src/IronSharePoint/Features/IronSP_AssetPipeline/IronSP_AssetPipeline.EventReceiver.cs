using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using IronSharePoint.EventReceivers;
using Microsoft.SharePoint;
using System.Linq;
using IronSharePoint.Util;

namespace IronSharePoint.Features.IronSP_AssetPipeline
{
    /// <summary>
    ///     This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    ///     The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    [Guid("1aff5b4a-42ad-4098-bf62-40ba785bff86")]
    public class IronSP_AssetPipelineEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            var web = (SPWeb) properties.Feature.Parent;
            SPList styleLib = web.Lists["Style Library"];
            SPContentType assetCT = web.ContentTypes[new SPContentTypeId("0x01004076C89F19854074B023E1DF178902A7")];

            //styleLib.ContentTypesEnabled = true;
            //if (styleLib.ContentTypes.OfType<SPContentType>().All(x => x.Id != assetCT.Id))
            //{
            //    //styleLib.ContentTypes.Add(assetCT);
            //}
            //styleLib.EventReceivers.Add(SPEventReceiverType.ItemUpdated,
            //                            typeof (AssetPipelineEventReceiver).Assembly.FullName,
            //                            typeof (AssetPipelineEventReceiver).FullName);
            //styleLib.EventReceivers.Add(SPEventReceiverType.ItemAdded,
            //                            typeof (AssetPipelineEventReceiver).Assembly.FullName,
            //                            typeof (AssetPipelineEventReceiver).FullName);
            //styleLib.Update();

            //RegisterContentTypeEventReceiver(assetCT, SPEventReceiverType.ItemAdded, typeof (AssetPipelineEventReceiver),
            //                                 100);
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            //var web = (SPWeb) properties.Feature.Parent;
            //SPList styleLib = web.GetList("Style Library");
            //SPContentType assetCT = web.ContentTypes[new SPContentTypeId("0x01004076C89F19854074B023E1DF178902A7")];

            //styleLib.ContentTypes.Delete(assetCT.Id);
            //styleLib.EventReceivers.OfType<SPEventReceiverDefinition>().
            //    Where(x => x.Class == typeof(AssetPipelineEventReceiver).FullName)
            //    .ToArray().ForEach(x => x.Delete());
            //styleLib.Update();

            //UnregisterContentTypeEventReceiver(assetCT, typeof (AssetPipelineEventReceiver));
        }

        private void RegisterContentTypeEventReceiver(SPContentType contentType, SPEventReceiverType type, Type target,
                                                      int sequenceNumber)
        {
            SPEventReceiverDefinition addingEventReceiver = contentType.EventReceivers.Add();
            addingEventReceiver.Type = type;
            addingEventReceiver.Assembly = target.Assembly.FullName;
            addingEventReceiver.Class = target.FullName;
            addingEventReceiver.SequenceNumber = sequenceNumber;
            addingEventReceiver.Update();

            contentType.Update();
        }

        private void UnregisterContentTypeEventReceiver(SPContentType contentType, Type target)
        {
            var toDelete = new List<SPEventReceiverDefinition>();
            foreach (SPEventReceiverDefinition eventReceiver in contentType.EventReceivers)
            {
                if (eventReceiver.Class == target.FullName)
                {
                    toDelete.Add(eventReceiver);
                }
            }
            foreach (SPEventReceiverDefinition eventRecevier in toDelete)
            {
                eventRecevier.Delete();
            }
            contentType.Update(true);
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