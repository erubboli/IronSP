using System;
using System.Collections.Generic;
using IronSharePoint.AssetPipeline;
using Microsoft.SharePoint;

namespace IronSharePoint.EventReceivers
{
    public class AssetPipelineEventReceiver : SPItemEventReceiver
    {
        private static readonly SPContentTypeId IronAssetId =
            new SPContentTypeId("0x01004076C89F19854074B023E1DF178902A7");

        private static readonly Dictionary<Guid, AssetRuntime> Runtimes;
        private static readonly Object Lock = new Object();

        static AssetPipelineEventReceiver()
        {
            Runtimes = new Dictionary<Guid, AssetRuntime>();
        }

        private AssetRuntime GetRuntime(Guid siteId)
        {
            AssetRuntime runtime;
            if (!Runtimes.TryGetValue(siteId, out runtime))
            {
                lock (Lock)
                {
                    if (!Runtimes.TryGetValue(siteId, out runtime))
                    {
                        runtime = new AssetRuntime(siteId);
                    }
                }
            }

            return runtime;
        }

        public override void ItemUpdated(SPItemEventProperties properties)
        {
            if (!IsIronAsset(properties.ListItem)) return;

            CompileFile(properties);
            base.ItemUpdated(properties);
        }

        private void CompileFile(SPItemEventProperties properties)
        {
            AssetRuntime runtime = GetRuntime(properties.SiteId);
            var asset = new Asset(properties.ListItem, runtime);
            try
            {
                asset.Save(properties.Web);
                properties.ListItem[FieldHelper.IronOutput] = "Success";
                properties.ListItem[FieldHelper.IronErrorFlag] = false;
            }
            catch (Exception ex)
            {
                properties.ListItem[FieldHelper.IronOutput] = ex.Message;
                properties.ListItem[FieldHelper.IronErrorFlag] = true;
            }
            properties.ListItem.SystemUpdate(false);
        }

        public override void ItemAdded(SPItemEventProperties properties)
        {
            if (!IsIronAsset(properties.ListItem)) return;

            CompileFile(properties);
            base.ItemAdded(properties);
        }

        private bool IsIronAsset(SPListItem listItem)
        {
            SPContentTypeId id = listItem.ContentType.Id;
            do
            {
                if (id == IronAssetId)
                    return true;
                id = id.Parent;
            } while (id != default(SPContentTypeId));

            return false;
        }
    }
}