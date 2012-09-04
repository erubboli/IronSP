using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using Microsoft.Scripting;
using System.IO;
using System.Web;
using IronSharePoint.Administration;
using System.Security;

namespace IronSharePoint
{
    public class IronHive : ScriptHost
    {
        private IronPlatformAdaptationLayer _ironAdaptationLayer;
        private bool _closed = false;
        private Guid _siteId;
        private SPSite _site;
        public SPSite Site 
        {
            get
            {
                if (_site == null || _closed)
                {
                    var key = IronHelper.GetPrefixedKey(_siteId.ToString());

                    if (HttpContext.Current != null && HttpContext.Current.Items[key] != null)
                    {
                        _site = HttpContext.Current.Items[key] as SPSite;
                    }
                    else
                    {
                        _site = new SPSite(_siteId, SPUserToken.SystemAccount);
                        _closed = false;

                        if (HttpContext.Current != null)
                        {                 
                            HttpContext.Current.Items[key] = _site;                         
                        }
                    }
                }

                return _site;
            }
        }

        public SPWeb Web 
        { 
            get
            {
                return Site.RootWeb;
            }
        }

        public SPFolder Folder 
        {
            get
            {
                return Web.GetFolder(IronConstant.IronHiveListPath);
            }
        }

        public SPList List 
        {
            get
            {
                return Folder.DocumentLibrary;
            }
        }

        public SPFeature Feature 
        { 
            get
            {
                var hiveFeature = Site.Features[new Guid(IronConstant.IronHiveSiteFeatureId)];

                if (hiveFeature == null)
                {
                    throw new InvalidOperationException(String.Format("'IronSP Hive Site' feature is not activated on the site with the id {0}", _siteId));
                }

                return hiveFeature;
            }
        }

       
        public string FeatureFolderPath 
        { 
            get
            {
                return new DirectoryInfo(Feature.Definition.RootDirectory).Parent.FullName;
            }
        }

        internal void Open(Guid hiveSiteId)
        {
            _siteId = hiveSiteId;
            _site = null;
            _hiveFileDictionary = null;
        }

        public override PlatformAdaptationLayer PlatformAdaptationLayer
        {
            get
            {
                return _ironAdaptationLayer ?? (_ironAdaptationLayer = new IronPlatformAdaptationLayer(this));
            }
        }

        public IronPlatformAdaptationLayer IronPlatformAdaptationLayer
        {
            get
            {
                return _ironAdaptationLayer ?? (_ironAdaptationLayer = new IronPlatformAdaptationLayer(this));
            }
        }

        public SPFile GetFile(string fileName)
        {
            if (!fileName.Contains(IronConstant.IronHiveDefaultRoot))
            {
                fileName = (IronConstant.IronHiveDefaultRoot + "/" + fileName).Replace("//", "/");
            }

            if (!PlatformAdaptationLayer.FileExists(fileName))
            {
                throw new FileNotFoundException();
            }

            return IronPlatformAdaptationLayer.GetIronHiveFile(fileName);
        }

        private Dictionary<String, String> _hiveFileDictionary;

        public Dictionary<String, String> HiveFileDictionary
        {
            get
            {
                if (_hiveFileDictionary == null)
                {
                    _hiveFileDictionary = new Dictionary<string, string>();

                    var query = new SPQuery();
                    query.Query = "<Where></Where>";
                    query.ViewFields = "<FieldRef Name='FileRef'/><FieldRef Name='FileLeafRef'/>";
                    query.ViewAttributes = "Scope='Recursive'";
                    query.IncludeMandatoryColumns = false;

                    var allItems = this.List.GetItems(query);

                    foreach (SPListItem item in allItems)
                    {
                        //todo: refactor
                        _hiveFileDictionary.Add(item["FileRef"].ToString().Replace((Web.ServerRelativeUrl + "/").Replace("//","/") , String.Empty).ToLower(), item["FileLeafRef"].ToString().ToLower());
                    }
                }

                return _hiveFileDictionary;
            }
        }


        public string LoadText(string fileName)
        {
            var file = GetFile(fileName);
            var str = Web.GetFileAsString(file.Url);

            return str;
        } 

        public void Close()
        {
            if (_site != null && !_closed)
            {
                _site.Dispose();
                _closed=true;
            }
        }
    }
}
