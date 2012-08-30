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
    public class IronHost : ScriptHost
    {
        private IronPlatformAdaptationLayer _ironAdaptationLayer;

        public SPSite HiveSite { get; private set; }
        public SPWeb HiveWeb { get; private set; }
        public SPList HiveList { get; private set; }
        public SPFolder HiveFolder { get; set; }
        public string FeatureFolderPath { get; set; }

        internal void SetHiveSite(SPSite hiveSite)
        {
            HiveSite = hiveSite;
            HiveWeb = HiveSite.RootWeb;
            HiveFolder = HiveWeb.GetFolder(IronConstants.IronHiveListPath);
            HiveList = HiveFolder.DocumentLibrary;
            _ironAdaptationLayer = null;

            var hiveFeature = hiveSite.Features[new Guid(IronConstants.IronHiveSiteFeatureId)];
            FeatureFolderPath = new DirectoryInfo(hiveFeature.Definition.RootDirectory).Parent.FullName;
        }

        public override PlatformAdaptationLayer PlatformAdaptationLayer
        {
            get
            {
                return _ironAdaptationLayer ?? (_ironAdaptationLayer = new IronPlatformAdaptationLayer(this));
            }
        }
    }
}
