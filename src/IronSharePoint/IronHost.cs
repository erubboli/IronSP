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

        private SPSite _hiveSite;

        private bool _disposed = false;

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


        internal void SetHiveSite(Guid hiveSiteId)
        {
            _hiveSite = new SPSite(hiveSiteId, SPUserToken.SystemAccount);
            _hiveWeb = _hiveSite.RootWeb;
            _hiveFolder = _hiveWeb.GetFolder(IronConstants.IronHiveListPath);
            _hiveList = _hiveFolder.DocumentLibrary;
            _ironAdaptationLayer = null;

            var hiveFeature = _hiveSite.Features[new Guid(IronConstants.IronHiveSiteFeatureId)];

            if (hiveFeature == null)
            {
                throw new InvalidOperationException(String.Format("'IronSP Hive Site' feature is not activated on the site with id {0}", hiveSiteId));
            }

            _featureFolderPath = new DirectoryInfo(hiveFeature.Definition.RootDirectory).Parent.FullName;
        }

        public override PlatformAdaptationLayer PlatformAdaptationLayer
        {
            get
            {
                if (_ironAdaptationLayer == null)
                {
                    _ironAdaptationLayer = new IronPlatformAdaptationLayer(this);
                }
                return _ironAdaptationLayer;
            }
        }

        public void Dispose()
        {
            if (_hiveSite == null && !_disposed)
            {
                _hiveSite.Dispose();
                _disposed=true;
            }
        }
    }
}
