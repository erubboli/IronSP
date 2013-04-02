using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using IronSharePoint.Hives;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Linq;
using IronSharePoint.Util;

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

    public class SetupCollection<TSetup> : SPAutoSerializingObject, IEnumerable<TSetup>
        where TSetup : SetupBase, new()
    {
        [Persisted]
        private List<TSetup> _setups;

        public SetupCollection()
            :this(new TSetup[0])
        {
        }
        public SetupCollection(params TSetup[] defaults)
        {
            _setups = new List<TSetup>(defaults);
        }

        public TSetup this[Guid id]
        {
            get { return _setups.FirstOrDefault(x => x.Id == id); }
        }

        public TSetup this[int index]
        {
            get { return _setups.ElementAt(index); }
        }

        public TSetup Add()
        {
            var setup = new TSetup
                {
                    Id = Guid.NewGuid()
                };
            _setups.Add(setup);

            return setup;
        }

        public TSetup Remove(TSetup setup)
        {
            return Remove(setup.Id);
        }

        public TSetup Remove(Guid setupId)
        {
            var setup = _setups.SingleOrDefault(x => x.Id == setupId);
            _setups.Remove(setup);
            return setup;
        }

        public void Clear()
        {
            _setups.Clear();
        }

        public IEnumerator<TSetup> GetEnumerator()
        {
            return _setups.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class SetupBase : SPAutoSerializingObject 
    {
        [Persisted] 
        private Guid _id;
        [Persisted] 
        private string _displayName;
        [Persisted]
        private string _description;

        /// <summary>
        /// Display name of the Runtime
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        /// <summary>
        /// Optional description for the Runtime
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Id of the runtime
        /// </summary>
        public Guid Id
        {
            get { return _id; }
            internal set { _id = value; }
        }

        protected bool Equals(SetupBase other)
        {
            return _id.Equals(other._id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SetupBase) obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }

    /// <summary>
    /// Stores information needed to setup a Runtime
    /// </summary>
    public class RuntimeSetup : SetupBase
    {
        [Persisted] private readonly List<Guid> _hiveIds;
        [Persisted] private readonly List<string> _gemPaths;

        public RuntimeSetup()
        {
            _hiveIds = new List<Guid> { HiveSetup.IronSPRoot.Id };
            _gemPaths = new List<string> { IronConstant.GemsDirectory };
        }

        /// <summary>
        /// Enumerates all HiveSetups added to this RuntimeSetup
        /// </summary>
        public IEnumerable<HiveSetup> Hives
        {
            get
            {
                var registry = IronRegistry.Local;
                return _hiveIds.Select(x => registry.Hives[x]).Compact();
            }
        }

        /// <summary>
        /// List of gem paths to be used by the runtime
        /// </summary>
        public IEnumerable<string> GemPaths
        {
            get { return _gemPaths; }
        }

        public void AddHive(HiveSetup hiveSetup)
        {
            Contract.Requires<ArgumentNullException>(hiveSetup != null);

            AddHive(hiveSetup.Id);
        }

        public void AddHive(Guid hiveId)
        {
            Contract.Requires<ArgumentException>(hiveId != Guid.Empty);

            if (!_hiveIds.Contains(hiveId)) _hiveIds.Add(hiveId);
        }

        public void RemoveHive(HiveSetup hiveSetup)
        {
            Contract.Requires<ArgumentNullException>(hiveSetup != null);

            _hiveIds.Remove(hiveSetup.Id);
        }

        public void RemoveHive(Guid hiveId)
        {
            Contract.Requires<ArgumentException>(hiveId != Guid.Empty);

            _hiveIds.Remove(hiveId);
        }

        public void AddGemPath(string gemPath)
        {
            Contract.Requires<ArgumentException>(Path.IsPathRooted(gemPath));

            if (!_gemPaths.Contains(gemPath, StringComparer.InvariantCultureIgnoreCase))
            {
                _gemPaths.Add(gemPath);
            }
        }

        public void RemoveGemPath(string gemPath)
        {
            _gemPaths.Remove(gemPath);
        }
    }

    /// <summary>
    /// Stores information needed to setup a Hive
    /// </summary>
    public class HiveSetup : SetupBase
    {
        [Persisted]
        private object[] _hiveArguments;
        [Persisted]
        private Type _hiveType;
        [Persisted]
        private int _priority;

        /// <summary>
        /// Priority of the hive. Files, which reside in multiple hives,
        /// will be loaded from the hive with the highest priority
        /// </summary>
        public int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        /// <summary>
        /// Arguments passed to the hive type when it is constructed
        /// </summary>
        public object[] HiveArguments
        {
            get { return _hiveArguments; }
            set { _hiveArguments = value; }
        }

        /// <summary>
        /// Type of the hive. Must implement <see cref="IHive"/>
        /// </summary>
        public Type HiveType
        {
            get { return _hiveType; }
            set
            {
                if (!typeof(IHive).IsAssignableFrom(value))
                {
                    throw new ArgumentException("Type doesn't implement IHive", "value");
                }
                _hiveType = value;
            }
        }

        /// <summary>
        /// Hive Setup for the IronSP Ruby Framework
        /// </summary>
        public static HiveSetup IronSPRoot
        {
            get
            {
                return new HiveSetup
                    {
                        Id = new Guid("56631301-08ED-4BDA-A1DD-0571308A55B4"),
                        DisplayName = "IronSP Root",
                        Description = "Contains the Ruby part of the IronSP Framework",
                        HiveArguments = new object[] {IronConstant.IronSPRootDirectory},
                        HiveType = typeof (DirectoryHive),
                        Priority = 1000
                    };
            }
        }
    }
}