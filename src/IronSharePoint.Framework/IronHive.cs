using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using IronSharePoint.Util;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using Microsoft.Scripting;
using System.IO;
using System.Web;
using IronSharePoint.Administration;
using System.Security;
using Microsoft.SharePoint.Utilities;

namespace IronSharePoint
{
    public class IronHive : ScriptHost, IDisposable
    {
        private static object _sync = new object();

        private IronPlatformAdaptationLayer _ironAdaptationLayer;

        public Guid Id { get; internal set; }

        [ThreadStatic] private static Dictionary<Guid, SPSite> _sites;

        public Dictionary<Guid, SPSite> Sites
        {
            get { return _sites ?? (_sites = new Dictionary<Guid, SPSite>()); }
        }

        public SPSite Site 
        {
            get
            {
                var key = IronHelper.GetPrefixedKey("Hive_" + Id);
                var context = HttpContext.Current;
                SPSite site;

                if (context != null)
                {
                    if (!context.Items.Contains(key))
                    {
                        context.Items[key] = new SPSite(Id, SPUserToken.SystemAccount);
                    }
                    site = context.Items[key] as SPSite;
                }
                else
                {
                    if (!Sites.ContainsKey(Id))
                    {
                        Sites[Id] = new SPSite(Id, SPUserToken.SystemAccount);
                    }
                    site = Sites[Id];
                }

                return site;
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
                return Web.GetFolder(IronConstant.HiveLibraryPath);
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
                    throw new InvalidOperationException(String.Format("'IronSP Hive Site' feature is not activated on the site with the id {0}", Id));
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
        
        public event EventHandler<HiveChangedArgs> Events;

        internal void FireHiveEvent(object sender, string eventName, SPItemEventProperties eventProperties)
        {
            if (Events != null)
            {
                Events.Invoke(sender, new HiveChangedArgs(){ Event=eventName, EventProperties=eventProperties});
            }
        }

        public string CurrentDir
        {
            get { return Path.Combine(FeatureFolderPath, IronConstant.IronSpRoot); }
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



        private volatile string[] _files;

        public string[] Files
        {
            get
            {
                if (_files == null)
                {
                    lock (_sync)
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
                                var hiveRelative = siteRelative.Replace(IronConstant.HiveLibraryPath + "/",
                                                                        string.Empty);

                                files.Add(hiveRelative);
                            }

                            _files = files.ToArray();
                        }
                    }
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

        internal void Close()
        {
            if (Sites.ContainsKey(Id))
            {
                Sites[Id].Dispose();
                Sites.Remove(Id);
            }
        }

        public void Dispose()
        {
            Close();
            _files = null;
        }

        public bool ContainsFile(string file)
        {
            file = Normalize(file);

            return Files.Contains(file);
        }

        public bool ContainsDirectory(string path)
        {
            if (path == ".") return true;

            path = Normalize(path);

            return !Files.Contains(path) && Files.Any(x => x.StartsWith(path));
        }

        public string GetFullPath(string file)
        {
            file = Normalize(file);

            return String.Format("{0}/{1}", Folder.ServerRelativeUrl, file);
        }

        public SPFile LoadFile(string file)
        {
            using (new SPMonitoredScope(string.Format("IronHive Access - {0}", file)))
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
            else if (file.StartsWith(IronConstant.HiveLibraryPath + "/"))
            {
                file = file.Replace(IronConstant.HiveLibraryPath + "/", string.Empty);
            }
            else if (file.StartsWith("."))
            {
                string fullPath;
                try
                {
                    file = Path.GetFullPath(file).Replace(CurrentDir + "\\", string.Empty);
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

            var regexPath = path == string.Empty ? "" : path + "/";
            var regexPattern = string.Format("({0}{1})/", regexPath, searchPattern.Replace("*", "[^/]+"));
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

            var regexPath = path == string.Empty ? "" : path + "/";
            var regexPattern = string.Format("{0}{1}", regexPath, searchPattern.Replace("*", "[^/]+$"));
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
