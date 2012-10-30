﻿using System;
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
    public class IronHive : ScriptHost, IDisposable
    {
        private IronPlatformAdaptationLayer _ironAdaptationLayer;
        
        private Guid _siteId;
        
        [ThreadStatic]
        private static SPSite _site;

        [ThreadStatic]
        private static bool _closed = false;

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

        public IronHive()
        {
            _currentDir = Directory.GetCurrentDirectory() + "\\";
        }

        /// maybe cause complie bug?!?!? 

        //public void ReloadFiles()
        //{
        //    _files = null;
            
        //    //load files
        //    var files = Files;
            
        //}


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
        private string _currentDir;

        public IList<string> Files
        {
            get
            {

#if DEBUG

                _files = new List<String>(); 

                DirSearch(IronDebug.IronDevHivePah);

                return _files;
#else

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
#endif

            }
        }

#if DEBUG

        void DirSearch(string sDir)
        {
            try
            {
                foreach (string f in Directory.GetFiles(sDir, "*.*"))
                {
                    _files.Add(f.Replace(IronDebug.IronDevHivePah+ "\\", "").Replace("\\","/"));
                }

                foreach (string d in Directory.GetDirectories(sDir))
                {   
                    DirSearch(d);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }
#endif



        public string LoadText(string file)
        {
            if (ContainsFile(file))
            {
                return Web.GetFileAsString(GetFullPath(file));
            }

            return null;
        } 

        internal void Close()
        {
            if (_site != null && !_closed)
            {
                _site.Dispose();
                _closed=true;
            }
        }

        public void Dispose()
        {
            Close();
        }

        public bool ContainsFile(string file)
        {
            file = Normalize(file);
            return Files.Contains(file);
        }

        public bool ContainsDirectory(string path)
        {
            path = Normalize(path);

            return Files.Any(file => file.StartsWith(path));
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

        public void Add(string file, byte[] data)
        {
            if (ContainsFile(file))
            {
                var spFile = LoadFile(file);
                spFile.SaveBinary(data);
            }
            else
            {
                Folder.Files.Add(GetFullPath(file), data);
            }
        }

        internal string Normalize(string file)
        {
            if (file.StartsWith(IronConstant.IronHiveRoot))
            {
                file = file.Replace(IronConstant.IronHiveRoot, string.Empty);
            }
            else if (file.StartsWith(IronConstant.IronHiveListPath + "/"))
            {
                file = file.Replace(IronConstant.IronHiveListPath + "/", string.Empty);
            }
            else
            {
                string fullPath;
                try
                {
                    file = Path.GetFullPath(file).Replace(_currentDir, string.Empty);
                }
                catch
                {
                    // do nothing
                }
            }

            return file.Replace('\\', '/');
        }

        public class HiveChangedArgs : EventArgs
        {
            public string Event { get; set; }
            public SPItemEventProperties EventProperties { get; set; }
        }


        public string[] GetDirectories(string path, string searchPattern)
        {
            path = Normalize(path);

            var regexPattern = string.Format("({0}/{1})/", path, searchPattern.Replace("*", "[^/]+"));
            var regex = new Regex(regexPattern);
            return Files.Select(file =>
                                    {
                                        var match = regex.Match(file);
                                        return match.Success ? match.Groups[1].Value : null;
                                    })
                .Where(x => x != null)
                .Distinct()
                .ToArray();
        }

        public string[] GetFiles(string path, string searchPattern)
        {
            path = Normalize(path);

            var regexPattern = string.Format("{0}/{1}", path, searchPattern.Replace("*", ".+"));
            var regex = new Regex(regexPattern);
            return Files.Select(file =>
            {
                var match = regex.Match(file);
                return match.Success ? match.Groups[0].Value : null;
            })
                .Where(x => x != null)
                .Distinct()
                .ToArray();
        }
    }
}
