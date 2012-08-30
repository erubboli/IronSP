using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using Microsoft.Scripting;
using System.IO;

namespace IronSharePoint
{
    public class IronHost : ScriptHost, IDisposable
    {
        private IronPlatformAdaptationLayer _ironAdaptationLayer;
        private bool _disposed;

        public SPSite HiveSite { get; private set; }
        public SPWeb HiveWeb { get; private set; }
        public SPList HiveList { get; private set; }
        public SPFolder HiveFolder { get; set; }
        public string FeatureFolderPath { get; set; }

        internal void SetHiveSite(Guid hiveSiteId)
        {

            HiveSite =new SPSite(hiveSiteId, SPUserToken.SystemAccount);
            HiveWeb = HiveSite.RootWeb;
            HiveFolder = HiveWeb.GetFolder(IronConstants.IronHiveListPath);
            HiveList = HiveFolder.DocumentLibrary;
            _ironAdaptationLayer = null;

            var hiveFeature = HiveSite.Features[new Guid(IronConstants.IronHiveSiteFeatureId)];
            FeatureFolderPath = new DirectoryInfo(hiveFeature.Definition.RootDirectory).Parent.FullName;

            if (hiveFeature == null)
            {
                throw new InvalidOperationException(String.Format("'IronSP Hive Site' feature is not activated on the site with id {0}", hiveSiteId));
            }

        }

        public override PlatformAdaptationLayer PlatformAdaptationLayer
        {
            get
            {
                return _ironAdaptationLayer ?? (_ironAdaptationLayer = new IronPlatformAdaptationLayer(this));
            }
        }

        public void Dispose()
        {
            if (HiveSite == null && !_disposed)
            {
                HiveSite.Dispose();
                _disposed=true;
            }
        }
    }
}
