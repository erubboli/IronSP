using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;
using System.Security;

namespace IronSharePoint.Framework.Administration
{
    [Guid("BAC9FD8C-27B1-4D15-9D10-8A3901F455DB")]
    public class IronHiveRegistry : SPPersistedObject
    {
        private static readonly Guid ObjectId = new Guid("{BAC9FD8C-27B1-4D15-9D10-8A3901F455DB}");

        /// <summary>
        /// Maps a target id (SPSite, SPWebApplication, ...) to a list of hives
        /// </summary>
        [Persisted] private readonly Dictionary<Guid, MappedHives> _mappedHives = new Dictionary<Guid, MappedHives>();

        /// <summary>
        /// The list of trusted hives
        /// </summary>
        [Persisted] private readonly List<HiveDescription> _trustedHives = new List<HiveDescription>();

        public IEnumerable<HiveDescription> TrustedHives
        {
            get { return _trustedHives.AsEnumerable(); }
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
                var registry = SPFarm.Local.GetObject(ObjectId) as IronHiveRegistry ??
                               new IronHiveRegistry("IronSharePoint.Administration.IronHiveRegistry", SPFarm.Local,
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
        public void AddHiveMapping(object target, params HiveDescription[] hives)
        {
            var targetId = GetTargetId(target);

            MappedHives mappedHives;
            if (!_mappedHives.TryGetValue(targetId, out mappedHives))
            {
                mappedHives = new MappedHives(this);
                _mappedHives[targetId] = mappedHives;
            }
            foreach (var hiveId in hives)
            {
                EnsureTrustedHive(hiveId);
                mappedHives.Add(hiveId);
            }
        }

        /// <summary>
        /// Returns all mapped hives for the <paramref name="site"/>
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public IList<HiveDescription> GetMappedHivesForSite(SPSite site)
        {
            return GetMappedHivesForSite(site.ID);
        }

        /// <summary>
        /// Returns all mapped hives for the SPSite with the given <paramref name="siteId"/>
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public IList<HiveDescription> GetMappedHivesForSite(Guid siteId)
        {
            var mappings = new MappedHives(this);
            SPSecurity.RunWithElevatedPrivileges(() =>
                {
                    using (var site = new SPSite(siteId))
                    {

                        var hasMapping = _mappedHives.TryGetValue(site.ID, out mappings) ||
                                         _mappedHives.TryGetValue(site.SiteSubscription.Id, out mappings) ||
                                         _mappedHives.TryGetValue(site.WebApplication.Id, out mappings) ||
                                         _mappedHives.TryGetValue(SPFarm.Local.Id, out mappings);

                        if (!hasMapping)
                        {
                            throw new ArgumentException("No hive mappings found for SPSite", "siteId");
                        }
                    }
                });

            return mappings;
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
        public void AddTrustedHive(HiveDescription hive)
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
        public void RemoveTrustedHive(HiveDescription hive)
        {
            if (TrustedHives.Contains(hive))
            {
                _trustedHives.Remove(hive);
            }
        }

        /// <summary>
        /// Removes the hive with the given <paramref name="id"/> from the list of trusted hives
        /// </summary>
        /// <param name="id"></param>
        public void RemoveTrustedHive(object id)
        {
            var hive = TrustedHives.FirstOrDefault(x => x.Id == id);
            RemoveTrustedHive(hive);
        }

        /// <summary>
        /// Ensures that the hive with the given <paramref name="id"/> is a trusted hive
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="SecurityException"></exception>
        public void EnsureTrustedHive(object id)
        {
            var hive = TrustedHives.FirstOrDefault(x => x.Id == id);
            EnsureTrustedHive(hive);
        }

        /// <summary>
        /// Ensures that the <paramref name="hive"/> is a trusted hive
        /// </summary>
        /// <param name="hive"></param>
        /// <exception cref="SecurityException"></exception>
        public void EnsureTrustedHive(HiveDescription hive)
        {
            if (hive == null || !TrustedHives.Contains(hive))
            {
                throw new SecurityException(String.Format("Hive '{0}' is not a trusted hive", hive));
            }
        }

        public override void Update(bool ensure)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
                {
                    foreach (var hive in _trustedHives.Where(x => x.HiveType == typeof(SPDocumentLibrary)))
                    {
                        using (var hiveSite = new SPSite((Guid) hive.Id))
                        {
                            if (hiveSite.Features[new Guid(IronConstant.IronHiveSiteFeatureId)] == null)
                            {
                                hiveSite.Features.Add(new Guid(IronConstant.IronHiveSiteFeatureId));
                            }
                        }
                    }

                });

            base.Update(ensure);
        }

    }
}
