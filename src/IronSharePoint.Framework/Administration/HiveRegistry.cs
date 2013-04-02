using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using IronSharePoint.Hives;
using IronSharePoint.Util;
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
        [Persisted] private Dictionary<Guid, List<HiveSetup>> _mappedHives = new Dictionary<Guid, List<HiveSetup>>();

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
        /// Maps the <paramref name="hive"/> to the given <paramref name="target"/>. If a mapping already exist for the
        /// given <paramref name="target"/> the <paramref name="hive"/> is appended, otherwise a new mapping is created.
        /// The <paramref name="hive"/> must be a trusted hive.
        /// </summary>
        /// <param name="hive"></param>
        /// <param name="target"></param>
        /// <exception cref="SecurityException"></exception>
        public void Map(HiveSetup hive, object target)
        {
            IsTrusted(hive);
            var targetId = GetTargetId(target);

            List<HiveSetup> hiveSetups;
            if (!_mappedHives.TryGetValue(targetId, out hiveSetups))
            {
                hiveSetups = new List<HiveSetup>();
                _mappedHives[targetId] = hiveSetups;
            }
            hiveSetups.Add(hive);
        }

        /// <summary>
        /// Returns all mapped hives for the <paramref name="site"/>
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public SetupCollection<HiveSetup> Resolve(SPSite site)
        {
            return Resolve(site.ID);
        }

        /// <summary>
        /// Returns all mapped hive setups for the SPSite with the given <paramref name="siteId"/>
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public SetupCollection<HiveSetup> Resolve(Guid siteId)
        {
            SetupCollection<HiveSetup> hiveSetups;
            if (!TryResolve(siteId, out hiveSetups))
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
        public bool TryResolve(SPSite site, out SetupCollection<HiveSetup> hiveSetups)
        {
            return TryResolve(site.ID, out hiveSetups);
        }

        /// <summary>
        /// Tries to resolve all hive setups for the SPSite with the given <paramref name="siteId"/> and stores the
        /// result in <paramref name="hiveSetups"/>
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="hiveSetups"></param>
        /// <returns></returns>
        public bool TryResolve(Guid siteId, out SetupCollection<HiveSetup> hiveSetups)
        {
            hiveSetups = new SetupCollection<HiveSetup>();
            SetupCollection<HiveSetup> localSetups = hiveSetups; // Needed b/c of delegate
            SPSecurity.RunWithElevatedPrivileges(() =>
                {
                    using (var site = new SPSite(siteId))
                    {
                        var ids = new[]
                            {
                                site.ID,
                                site.SiteSubscription != null ? site.SiteSubscription.Id : Guid.Empty,
                                site.WebApplication.Id,
                                SPFarm.Local.Id
                            }.Compact();

                        foreach (var id in ids)
                        {
                            List<HiveSetup> idSetups;
                            if (_mappedHives.TryGetValue(id, out idSetups))
                            {
                                //localSetups.AddRange(idSetups);
                            }
                        }
                    }
                });

            return hiveSetups.Any();
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
        public void Trust(HiveSetup hive)
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
        public void Untrust(HiveSetup hive)
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
        public virtual void IsTrusted(object id)
        {
            var hive = TrustedHives.FirstOrDefault(x => Object.Equals(x.HiveArguments[0], id));
            IsTrusted(hive);
        }

        /// <summary>
        /// Ensures that the <paramref name="hive"/> is a trusted hive
        /// </summary>
        /// <param name="hive"></param>
        /// <exception cref="SecurityException"></exception>
        public virtual void IsTrusted(HiveSetup hive)
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
            if (_mappedHives == null) _mappedHives = new Dictionary<Guid, List<HiveSetup>>();
        }
    }
}
