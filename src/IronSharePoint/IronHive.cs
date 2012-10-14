using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
        
        public Guid Id
        {
            get { return _siteId; }
        }

        public event EventHandler<HiveChangedArgs> Events;

        internal void FireHiveEvent(object sender, string eventName, SPItemEventProperties eventProperties)
        {
            if (Events != null)
            {
                Events.Invoke(sender, new HiveChangedArgs(){ Event=eventName, EventProperties=eventProperties});
            }
        }

        internal void Init(Guid hiveSiteId)
        {
            _siteId = hiveSiteId;
            _site = null;
            _files = null;
        }

        public void ReloadFiles()
        {
            _files = null;
            
            //load files
            var files = Files;
            
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

        private IList<String> _files;

        public IList<string> Files
        {
            get
            {
                if (_files == null)
                {
                    var query = new SPQuery();
                    query.Query = "<Where></Where>";
                    query.ViewFields = "<FieldRef Name='FileRef'/><FieldRef Name='FileLeafRef'/>";
                    query.ViewAttributes = "Scope='Recursive'";
                    query.IncludeMandatoryColumns = false;

                    var allItems = this.List.GetItems(query);

                    var files = new List<String>();
                    
                    foreach (SPListItem item in allItems)
                    {
                        var fileRef = item["FileRef"].ToString();
                        var siteRelative = fileRef.Replace((Web.ServerRelativeUrl + "/"), String.Empty);
                        var hiveRelative = siteRelative.Replace(IronConstant.IronHiveListPath + "/", string.Empty);

                        files.Add(hiveRelative);
                    }

                    _files = files.AsReadOnly();
                }

                return _files;
            }
        }

        public string LoadText(string file)
        {
            if (ContainsFile(file))
            {
                return Web.GetFileAsString(GetFullPath(file));
            }

            return null;
        } 

        public void Close()
        {
            if (_site != null && !_closed)
            {
                _site.Dispose();
                _closed=true;
            }
        }

        public bool ContainsFile(string file)
        {
            file = Normalize(file);
            return Files.Contains(file);
        }

        public string GetFullPath(string file)
        {
            file = Normalize(file);

            return String.Format("{0}/{1}", Folder.ServerRelativeUrl, file);
        }

        public SPFile LoadFile(string file)
        {
            if (ContainsFile(file))
            {
                var spFile = Web.GetFile(GetFullPath(file));
                if (spFile.Exists)
                {
                    return spFile;
                }
            }

            return null;
        }

        private string Normalize(string file)
        {
            if (file.StartsWith(IronConstant.IronHiveRoot))
            {
                file = file.Replace(IronConstant.IronHiveRoot, string.Empty);
            }
            return file;
        }

        public class HiveChangedArgs : EventArgs
        {
            public string Event { get; set; }
            public SPItemEventProperties EventProperties { get; set; }
        }
    }
}
