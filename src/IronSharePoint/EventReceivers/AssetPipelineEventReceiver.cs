using System;
using System.Collections.Generic;
using System.Text;
using IronSharePoint.AssetPipeline;
using Microsoft.SharePoint;

namespace IronSharePoint.EventReceivers
{
    public class AssetPipelineEventReceiver : SPItemEventReceiver
    {
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
            AssetRuntime runtime = GetRuntime(properties.SiteId);
            var asset = new Asset(properties.ListItem, runtime);
            try
            {
                Save(asset, properties.Web);
                properties.ListItem[FieldHelper.IronOutput] = "Success";
                properties.ListItem[FieldHelper.IronErrorFlag] = false;
            }
            catch (Exception ex)
            {
                properties.ListItem[FieldHelper.IronOutput] = ex.Message;
                properties.ListItem[FieldHelper.IronErrorFlag] = true;
            }
            base.ItemUpdated(properties);
        }
        public override void ItemAdded(SPItemEventProperties properties)
        {
            AssetRuntime runtime = GetRuntime(properties.SiteId);
            var asset = new Asset(properties.ListItem, runtime);
            try
            {
                Save(asset, properties.Web);
                properties.ListItem[FieldHelper.IronOutput] = "Success";
                properties.ListItem[FieldHelper.IronErrorFlag] = false;
            }
            catch (Exception ex)
            {
                properties.ListItem[FieldHelper.IronOutput] = ex.Message;
                properties.ListItem[FieldHelper.IronErrorFlag] = true;
            }
            base.ItemAdded(properties);
        }

        public void Save(Asset asset, SPWeb web)
        {
            SPFolder folder = web.GetFolder(asset.FolderName);
            byte[] bytes = Encoding.UTF8.GetBytes(asset.Compile());
            folder.Files.Add(string.Format("{0}/{1}", asset.FolderName, asset.Name), bytes, true);
        }
    }
}