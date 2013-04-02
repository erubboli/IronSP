using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using IronSharePoint.Util;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace IronSharePoint.Administration
{
    [Guid("6BE3E581-0E9A-4C6D-A03B-C5566F2BD89A")]
    public class IronRegistry : SPPersistedObject
    {
        private static readonly Guid ObjectId = new Guid("6BE3E581-0E9A-4C6D-A03B-C5566F2BD89A");

        [Persisted]
        private SetupCollection<RuntimeSetup> _runtimes = new SetupCollection<RuntimeSetup>();
        [Persisted] 
        private SetupCollection<HiveSetup> _hives = new SetupCollection<HiveSetup>(HiveSetup.IronSPRoot);
        [Persisted]
        private Dictionary<Guid,Guid> _targetToRuntimeAssociations = new Dictionary<Guid, Guid>();

        private static readonly Lazy<IronRegistry> _local;

        static IronRegistry()
        {
            _local = new Lazy<IronRegistry>(() => SPFarm.Local.GetObject(ObjectId) as IronRegistry ??
                                                  new IronRegistry("IronSharePoint.Administration.IronRegistry",
                                                                   SPFarm.Local, ObjectId), false);

        }

        protected IronRegistry(string name, SPPersistedObject parent)
            : base(name, parent)
        {
        }

        protected IronRegistry(string name, SPPersistedObject parent, Guid id)
            : base(name, parent, id) {}

        public SetupCollection<RuntimeSetup> Runtimes
        {
            get { return _runtimes; }
            private set { _runtimes = value; }
        }

        public SetupCollection<HiveSetup> Hives
        {
            get { return _hives; }
            private set { _hives = value; }
        }

        public IronRegistry() {}

        public static IronRegistry Local
        {
            get
            {
                return _local.Value;
            }
        }

        public void Associate(object target, RuntimeSetup runtime, bool overwrite = false)
        {
            Contract.Requires<ArgumentNullException>(runtime != null); 
            Associate(target, runtime.Id, overwrite);
        }

        public void Associate(object target, Guid runtimeId, bool overwrite = false)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<ArgumentException>(runtimeId != Guid.Empty);
            var targetId = GetTargetId(target);

            if (_targetToRuntimeAssociations.ContainsKey(targetId) && !overwrite)
            {
                var associatedId = _targetToRuntimeAssociations[targetId];
                throw new InvalidOperationException(string.Format("Target already associated to runtime with id '{0}'. Dissociate the target first or explicity allow overwriting", associatedId));
            }
            _targetToRuntimeAssociations[targetId] = runtimeId;
        }

        public void Dissociate(object target, RuntimeSetup runtime)
        {
            Contract.Requires<ArgumentNullException>(runtime != null); 
            Associate(target, runtime.Id);
        }

        public void Dissociate(object target, Guid runtimeId)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<ArgumentException>(runtimeId != Guid.Empty);

            var targetId = GetTargetId(target);

            if (!_targetToRuntimeAssociations.ContainsKey(targetId))
            {
                throw new InvalidOperationException("Target not mapped to any runtimes");
            }

            var associatedId = _targetToRuntimeAssociations[targetId];
            if (runtimeId != associatedId)
            {
                throw new InvalidOperationException(string.Format("Target not associated to runtime with id {0}",
                                                                  runtimeId));
            }

            _targetToRuntimeAssociations.Remove(targetId);
        }

        public RuntimeSetup ResolveRuntime(SPSite site)
        {
            RuntimeSetup runtimeSetup;
            if (!TryResolveRuntime(site, out runtimeSetup))
            {
                throw new ArgumentException("No runtime associated to site");
            }
            return runtimeSetup;
        }

        public bool TryResolveRuntime(SPSite site, out RuntimeSetup runtimeSetup)
        {
            runtimeSetup = null;
            var hierarchy = new[]
                {
                    site.ID,
                    site.SiteSubscription != null ? site.SiteSubscription.Id : Guid.Empty,
                    site.WebApplication.Id,
                    SPFarm.Local.Id
                }.Compact();

            foreach (var targetId in hierarchy)
            {
                Guid associatedId;
                if (_targetToRuntimeAssociations.TryGetValue(targetId, out associatedId))
                {
                    runtimeSetup = _runtimes.SingleOrDefault(x => x.Id == associatedId);
                }
            }
            return runtimeSetup != null;
        }

        private static Guid GetTargetId(object target)
        {
            Contract.Requires<ArgumentException>(
                target is SPSite || target is SPSiteSubscription || target is SPWebApplication || target is SPFarm,
                "target must be of type SPSite, SPSiteSubscription, SPWebApplication or SPFarm");

            Guid targetId;

            if (target is SPSite)
                targetId = (target as SPSite).ID;
            else if (target is SPSiteSubscription)
                targetId = (target as SPSiteSubscription).Id;
            else if (target is SPWebApplication)
                targetId = (target as SPWebApplication).Id;
            else
                targetId = (target as SPFarm).Id;

            return targetId;
        }

        protected override void OnDeserialization()
        {
            base.OnDeserialization();
            System.Console.WriteLine(this);
        }
    }
}