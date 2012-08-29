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
        private SPSite _hiveSite;

        public SPSite HiveSite
        {
            get { return _hiveSite; }
        }

        private SPWeb _hiveWeb;

        public SPWeb HiveWeb
        {
            get { return _hiveWeb; }
        }

        private SPList _hiveList;

        public SPList HiveList
        {
            get { return _hiveList; }
        }

        private SPFolder _hiveFolder;

        public SPFolder HiveFolder
        {
            get { return _hiveFolder; }
            set { _hiveFolder = value; }
        }

        private string _featureFolderPath;

        public string FeatureFolderPath
        {
            get { return _featureFolderPath; }
            set { _featureFolderPath = value; }
        }


        internal void SetHiveSite(SPSite hiveSite)
        {
            _hiveSite = hiveSite;
            _hiveWeb = _hiveSite.RootWeb;
            _hiveFolder = _hiveWeb.GetFolder(IronConstants.IronHiveListPath);
            _hiveList = _hiveFolder.DocumentLibrary;

            var hiveFeature = hiveSite.Features[new Guid(IronConstants.IronHiveSiteFeatureId)];
            _featureFolderPath = new DirectoryInfo(hiveFeature.Definition.RootDirectory).Parent.FullName;
        }

        public override PlatformAdaptationLayer PlatformAdaptationLayer
        {
            get
            {
                return new IronPlatformAdaptationLayer(this);
            }
        }
    }
}
