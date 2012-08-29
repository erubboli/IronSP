using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using System.Linq;
using IronSharePoint.EventReceivers;

namespace IronSharePoint.Features.IronSP_Site
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("7195fcc7-3b90-428e-b2d2-e2afa866f265")]
    public class IronSP_SiteEventReceiver : SPFeatureReceiver
    {
        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            //var site = properties.Feature.Parent as SPSite;
            //var web = site.RootWeb;

            //var ironHiveList = web.GetFolder(IronConstants.IronHiveListPath).DocumentLibrary;

            //// register IronScript CT
            //var ironScriptCTId = new SPContentTypeId(IronContentTypeIds.IronScript);
            //var ironScriptCT = ironHiveList.ContentTypes[ironHiveList.ContentTypes.BestMatch(ironScriptCTId)];
            //if (ironScriptCT == null || ironScriptCT.Parent.Id.IsChildOf(ironScriptCTId)==false)
            //{
            //    ironScriptCT = ironHiveList.ContentTypes.Add(web.ContentTypes[ironScriptCTId]);

            //    // set default contenttype
            //    var contentTypeOrderArr = ironHiveList.ContentTypes.Cast<SPContentType>().ToList();
            //    contentTypeOrderArr.Remove(ironScriptCT);
            //    contentTypeOrderArr.Insert(0, ironScriptCT);
            //    ironHiveList.RootFolder.UniqueContentTypeOrder = contentTypeOrderArr.ToArray();
            //    ironHiveList.RootFolder.Update();
            //}

            //// configure hive list
            //ironHiveList.ContentTypesEnabled = true;
            //ironHiveList.EnableVersioning = true;
           

            //// register script compiler event receiver
            //var ironScriptCompilerReceiverType = typeof(IronScriptCompilerReceiver);
            //var ironScriptWebCT = web.ContentTypes[ironScriptCTId];
            //ironScriptWebCT.EventReceivers.Add(SPEventReceiverType.ItemAdded, ironScriptCompilerReceiverType.Assembly.FullName, ironScriptCompilerReceiverType.FullName);
            //ironScriptWebCT.EventReceivers.Add(SPEventReceiverType.ItemUpdated, ironScriptCompilerReceiverType.Assembly.FullName, ironScriptCompilerReceiverType.FullName);
            //ironScriptWebCT.Update(true);

        }


        // Uncomment the method below to handle the event raised before a feature is deactivated.

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            //var site = properties.Feature.Parent as SPSite;
            //var web = site.RootWeb;

           
            ////unregister script compiler event receiver
            //var ironScriptCTId = new SPContentTypeId(IronContentTypeIds.IronScript);
            //var ironScriptWebCT = web.ContentTypes[ironScriptCTId];
            //var ironScriptCompilerReceiverType = typeof(IronScriptCompilerReceiver);
            //ironScriptWebCT.EventReceivers.OfType<SPEventReceiverDefinition>()
            //    .Where(e => e.Class == ironScriptCompilerReceiverType.FullName).ToList().ForEach(e => e.Delete());
            //ironScriptWebCT.Update(true);
           
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
