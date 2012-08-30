using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;
using System.Collections.ObjectModel;

namespace IronSharePoint.Administration
{
    public class IronHiveRegistry: SPPersistedObject
    {
        private static readonly Guid _objectId = new Guid("{7AC6F8BC-7977-4F04-AE74-CCA2232AC135}"); 

       [Persisted]
        // key is the target object and value is the hive site id
        private Dictionary<Guid, String> _hiveMappings = new Dictionary<Guid, String>();

       public ReadOnlyCollection<HiveMapping> HiveMappings
       {
           get {

               var list = new List<HiveMapping>();

               foreach (var entry in _hiveMappings)
               {
                   list.Add(new HiveMapping(entry.Value));
               }

               return new ReadOnlyCollection<HiveMapping>(list);     
           }
       }

       /*The default constructor must be specified for serialization.*/
       public IronHiveRegistry()
       {
       }

       public IronHiveRegistry(string name, SPPersistedObject parent, Guid id)
          : base(name, parent, id)
       {
       }

       public static IronHiveRegistry Local
       {
           get
           {            
               var registry = SPFarm.Local.GetObject(_objectId) as IronHiveRegistry;
               if (registry == null)
               {
                   registry = new IronHiveRegistry("IronSharePoint.Administration.IronHiveRegistry", SPFarm.Local, _objectId);
               }

               return registry;
           }
       }

        public Guid GetHiveBySiteId(Guid targetSitId)
        {
            var hiveSiteId = Guid.Empty; 

            if (_hiveMappings.Count==0)
            {
                return hiveSiteId;
            }

            SPSecurity.RunWithElevatedPrivileges(() =>
            {

                using (SPSite site = new SPSite(targetSitId))
                {
                    // target site is mapped to a hive site
                    if (_hiveMappings.ContainsKey(targetSitId))
                    {
                        hiveSiteId = new HiveMapping(_hiveMappings[targetSitId]).HiveSiteId;
                    }
                    else
                    {
                        // if not, check the subscription id
                        if (site.SiteSubscription != null && _hiveMappings.ContainsKey(site.SiteSubscription.Id))
                        {
                            hiveSiteId = new HiveMapping(_hiveMappings[site.SiteSubscription.Id.Id]).HiveSiteId;
                        }
                        else
                        {
                            //check for web app mapping
                            var webAppId = site.WebApplication.Id;
                            if (_hiveMappings.ContainsKey(webAppId))
                            {
                                hiveSiteId = new HiveMapping(_hiveMappings[webAppId]).HiveSiteId;
                            }
                            else
                            {
                                //check for farm mapping
                                var farmId = SPFarm.Local.Id;
                                if (_hiveMappings.ContainsKey(farmId))
                                {
                                    hiveSiteId = new HiveMapping(_hiveMappings[farmId]).HiveSiteId;
                                }
                            }
                        }
                    }
                }
            });

            return hiveSiteId;
            
        }

        public void AddHiveMapping(SPSite hiveSite, object targetObject)
        {
            var targetSite = targetObject as SPSite;
            if (targetSite != null)
            {
                var hiveMapping = new HiveMapping()
                {
                    HiveSiteId = hiveSite.ID,
                    TargetObjectId = targetSite.ID,
                    TargetObjectType = targetSite.GetType().Name
                };

                _hiveMappings.Add(hiveMapping.TargetObjectId, hiveMapping.ToString());

                return;
            }

            var siteSubscription = targetObject as SPSiteSubscription;
            if (siteSubscription != null)
            {
                var hiveMapping = new HiveMapping()
                {
                    HiveSiteId = hiveSite.ID,
                    TargetObjectId = siteSubscription.Id.Id,
                    TargetObjectType = siteSubscription.GetType().Name
                };

                _hiveMappings.Add(hiveMapping.TargetObjectId, hiveMapping.ToString());

                return;
            }

            var webApp = targetObject as SPWebApplication;
            if (webApp != null)
            {
                var hiveMapping = new HiveMapping()
                {
                    HiveSiteId = hiveSite.ID,
                    TargetObjectId = webApp.Id,
                    TargetObjectType = webApp.GetType().Name
                };

                _hiveMappings.Add(hiveMapping.TargetObjectId, hiveMapping.ToString());

                return;
            }

            var farm = targetObject as SPFarm;
            if (farm != null)
            {
                var hiveMapping = new HiveMapping()
                {
                    HiveSiteId = hiveSite.ID,
                    TargetObjectId = farm.Id,
                    TargetObjectType = farm.GetType().Name
                };

                _hiveMappings.Add(hiveMapping.TargetObjectId, hiveMapping.ToString());

                return;
            }

            throw new NotSupportedException("Only mappings for objects of type SPSite, SPSiteSubscription, SPWebApplication and SPFarm allowed!");  
        }

        public void DeleteHiveMapping(Guid targetId)
        {
            _hiveMappings.Remove(targetId);
        }

        [Serializable]
        public class HiveMapping
        {
            public Guid HiveSiteId { get; set; }
            public Guid TargetObjectId { get; set; }
            public string TargetObjectType { get; set; }

            public override string ToString()
            {
                return HiveSiteId.ToString() + ";" + TargetObjectId.ToString() + ";" + TargetObjectType.ToString();
            }

            public HiveMapping() { }

            public HiveMapping(string str)
            {
                var arr = str.Split(';');
                HiveSiteId = new Guid(arr[0]);
                TargetObjectId = new Guid(arr[1]);
                TargetObjectType = arr[2];
            }
        }
    }
}
