using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using Microsoft.Scripting;
using System.IO;
using System.Web;

namespace IronSharePoint
{
    public class IronHost : ScriptHost, IDisposable
    {
        private IronPlatformAdaptationLayer _ironAdaptationLayer;
        private bool _disposed;
        private Guid _hiveSiteId;
        private SPSite _hiveSite;
        public SPSite HiveSite 
        {
            get
            {
                if (_hiveSite == null)
                {
                    if (HttpContext.Current != null && HttpContext.Current.Items[_hiveSiteId] != null)
                    {
                        _hiveSite = HttpContext.Current.Items[_hiveSiteId] as SPSite;
                    }
                    else
                    {
                        _hiveSite = new SPSite(_hiveSiteId, SPUserToken.SystemAccount);

                        if (HttpContext.Current != null)
                        {
                            HttpContext.Current.Items[_hiveSiteId] = _hiveSite;
                        }
                    }
                }

                return _hiveSite;
            }
        }

        public SPWeb HiveWeb 
        { 
            get
            {
                return HiveSite.RootWeb;
            }
        }

        public SPFolder HiveFolder 
        {
            get
            {
                return HiveWeb.GetFolder(IronConstants.IronHiveListPath);
            }
        }

        public SPList HiveList 
        {
            get
            {
                return HiveFolder.DocumentLibrary;
            }
        }

        public SPFeature HiveFeature 
        { 
            get
            {
                var hiveFeature = HiveSite.Features[new Guid(IronConstants.IronHiveSiteFeatureId)];

                if (hiveFeature == null)
                {
                    throw new InvalidOperationException(String.Format("'IronSP Hive Site' feature is not activated on the site with the id {0}", _hiveSiteId));
                }

                return hiveFeature;
            }
        }

       
        public string FeatureFolderPath 
        { 
            get
            {
                return new DirectoryInfo(HiveFeature.Definition.RootDirectory).Parent.FullName;
            }
        }

        internal void SetHiveSite(Guid hiveSiteId)
        {
            _hiveSiteId = hiveSiteId;  
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
            if (HiveSite != null && !_disposed)
            {
                HiveSite.Dispose();
                _disposed=true;
            }
        }
    }
}
