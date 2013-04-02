using System;
using IronSharePoint.Hives;

namespace IronSharePoint.AssetPipeline
{
    public class AssetScriptHost : IronScriptHostBase
    {
        private Guid _siteId;

        public AssetScriptHost(Guid siteId)
        {
            _siteId = siteId;
        }

        protected override IHive CreateHive()
        {
            return new HiveComposite(
                new SPDocumentHive(_siteId, "Style%20Library"),
                new DirectoryHive("C:\\code\\IronSP\\src\\IronSharePoint\\TEMPLATE\\Features\\IronSP_Root"));
        }
    }
}