using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using IronSharePoint.Hives;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;
using System.Security;

namespace IronSharePoint.Administration
{
    [Guid("DFEADCC2-F2E6-4797-9514-C2D26BC8747B")]
    public class HiveRegistry : SPPersistedObject
    {
        private static readonly Guid ObjectId = new Guid("DFEADCC2-F2E6-4797-9514-C2D26BC8747B");

        /// <summary>
        /// Maps a target id (SPSite, SPWebApplication, ...) to a list of hives
        /// </summary>
        [Persisted] private Dictionary<Guid, HiveSetupCollection> _mappedHives = new Dictionary<Guid, HiveSetupCollection>();

        /// <summary>
        /// The list of trusted hives
        /// </summary>
        [Persisted] private List<HiveSetup> _trustedHives = new List<HiveSetup>();

        public IEnumerable<HiveSetup> TrustedHives
        {
            get { return _trustedHives.AsEnumerable(); }
        }

        /*The default constructor must be specified for serialization.*/

        public HiveRegistry()
        {
        }

        public HiveRegistry(string name, SPPersistedObject parent, Guid id)
            : base(name, parent, id)
        {
        }

        public static HiveRegistry Local
        {
            get
            {
                var registry = SPFarm.Local.GetObject(ObjectId) as HiveRegistry ??
                               new HiveRegistry("IronSharePoint.Administration.HiveRegistry", SPFarm.Local,
                                                    ObjectId);

                return registry;
            }
        }

        /// <summary>
        /// Maps all <paramref name="hives"/> to the given <paramref name="target"/>. If a mapping already exist for the
        /// given <paramref name="target"/> the <paramref name="hives"/> are appended, otherwise a new mapping is created.
        /// All <paramref name="hives"/> must be trusted.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="hives"></param>
        /// <exception cref="SecurityException"></exception>
        public void AddHiveMapping(object target, params HiveSetup[] hives)
        {
            var targetId = GetTargetId(target);

            HiveSetupCollection hiveSetupCollection;
            if (!_mappedHives.TryGetValue(targetId, out hiveSetupCollection))
            {
                hiveSetupCollection = new HiveSetupCollection {Registry = this};
                _mappedHives[targetId] = hiveSetupCollection;
            }
            foreach (var hiveId in hives)
            {
                EnsureTrustedHive(hiveId);
                hiveSetupCollection.Add(hiveId);
            }
        }

        /// <summary>
        /// Returns all mapped hives for the <paramref name="site"/>
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public HiveSetupCollection GetHiveSetups(SPSite site)
        {
            return GetHiveSetups(site.ID);
        }

        /// <summary>
        /// Returns all mapped hive setups for the SPSite with the given <paramref name="siteId"/>
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public HiveSetupCollection GetHiveSetups(Guid siteId)
        {
            HiveSetupCollection hiveSetups;
            if (!TryGetHiveSetups(siteId, out hiveSetups))
            {
                throw new ArgumentOutOfRangeException("siteId", siteId, "No mapped hive setups found for SPSite");
            }
            return hiveSetups;
        }

        /// <summary>
        /// Returns all mapped hives for the <paramref name="site"/>
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool TryGetHiveSetups(SPSite site, out HiveSetupCollection hiveSetups)
        {
            return TryGetHiveSetups(site.ID, out hiveSetups);
        }

        /// <summary>
        /// Tries to rerieve all hive setups for the SPSite with the given <paramref name="siteId"/> and stores the
        /// result in <paramref name="hiveSetups"/>
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="hiveSetups"></param>
        /// <returns></returns>
        public bool TryGetHiveSetups(Guid siteId, out HiveSetupCollection hiveSetups)
        {
            HiveSetupCollection setups = null; // need an extra variable for out parameter below
            var hasSetups = false;
            SPSecurity.RunWithElevatedPrivileges(() =>
                {
                    using (var site = new SPSite(siteId))
                    {

                        hasSetups = _mappedHives.TryGetValue(site.ID, out setups) ||
                                    (site.SiteSubscription != null &&
                                     _mappedHives.TryGetValue(site.SiteSubscription.Id, out setups)) ||
                                    _mappedHives.TryGetValue(site.WebApplication.Id, out setups) ||
                                    _mappedHives.TryGetValue(SPFarm.Local.Id, out setups);
                    }
                });

            hiveSetups = setups;
            if (hasSetups) hiveSetups.Registry = this;

            return hasSetups;
        }

        private static Guid GetTargetId(object target)
        {
            Guid targetId;

            if (target is SPSite)
                targetId = (target as SPSite).ID;
            else if (target is SPSiteSubscription)
                targetId = (target as SPSiteSubscription).Id;
            else if (target is SPWebApplication)
                targetId = (target as SPWebApplication).Id;
            else if (target is SPFarm)
                targetId = (target as SPFarm).Id;
            else
                throw new NotSupportedException(
                    "Only mappings for objects of type SPSite, SPSiteSubscription, SPWebApplication and SPFarm allowed!");
            return targetId;
        }

        /// <summary>
        /// Adds the <paramref name="hive"/> to the list of trusted hives
        /// </summary>
        /// <param name="hive"></param>
        public void AddTrustedHive(HiveSetup hive)
        {
            if (!TrustedHives.Contains(hive))
            {
                _trustedHives.Add(hive);
            }
        }

        /// <summary>
        /// Removes the <paramref name="hive"/> from the list of trusted hives
        /// </summary>
        /// <param name="hive"></param>
        public void RemoveTrustedHive(HiveSetup hive)
        {
            if (TrustedHives.Contains(hive))
            {
                _trustedHives.Remove(hive);
            }
        }

        /// <summary>
        /// Ensures that the hive with the given <paramref name="id"/> is a trusted hive
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="SecurityException"></exception>
        public virtual void EnsureTrustedHive(object id)
        {
            var hive = TrustedHives.FirstOrDefault(x => Object.Equals(x.HiveArguments[0], id));
            EnsureTrustedHive(hive);
        }

        /// <summary>
        /// Ensures that the <paramref name="hive"/> is a trusted hive
        /// </summary>
        /// <param name="hive"></param>
        /// <exception cref="SecurityException"></exception>
        public virtual void EnsureTrustedHive(HiveSetup hive)
        {
            if(hive != null && !TrustedHives.Contains(hive))
            {
                throw new SecurityException(String.Format("Hive '{0}' is not a trusted hive", hive));
            }
        }

        public override void Update(bool ensure)
        {
            base.Update(ensure);
            SPSecurity.RunWithElevatedPrivileges(() =>
                {
                    foreach (var hive in _trustedHives.Where(x => x.HiveType == typeof(SPDocumentHive)))
                    {
                        using (var hiveSite = new SPSite((Guid) hive.HiveArguments[0]))
                        {
                            if (hiveSite.Features[new Guid(IronConstant.IronHiveSiteFeatureId)] == null)
                            {
                                hiveSite.Features.Add(new Guid(IronConstant.IronHiveSiteFeatureId));
                            }
                        }
                    }
                });
        }

        protected override void OnDeserialization()
        {
            base.OnDeserialization();
            if (_trustedHives == null) _trustedHives = new List<HiveSetup>();
            if (_mappedHives == null) _mappedHives = new Dictionary<Guid, HiveSetupCollection>();
        }
    }
}
