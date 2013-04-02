using System;
using System.Collections.Generic;
using System.Linq;
using IronSharePoint.Administration;
using IronSharePoint.Hives;
using IronSharePoint.Util;
using Microsoft.Scripting.Hosting;

namespace IronSharePoint
{
    public abstract class IronScriptHostBase : ScriptHost, IDisposable
    {
        private readonly Lazy<IHive> _hive;
        private readonly Lazy<IronPlatformAdaptationLayer> _ironPlatformAdaptationLayer;

        public IronPlatformAdaptationLayer IronPlatformAdaptationLayer
        {
            get { return _ironPlatformAdaptationLayer.Value; }
        }

        public override Microsoft.Scripting.PlatformAdaptationLayer PlatformAdaptationLayer
        {
            get { return IronPlatformAdaptationLayer; }
        }

        public IHive Hive
        {
            get { return _hive.Value; }
        }

        protected IronScriptHostBase()
        {
            _hive = new Lazy<IHive>(CreateHive);
            _ironPlatformAdaptationLayer =
                new Lazy<IronPlatformAdaptationLayer>(() => new IronPlatformAdaptationLayer(Hive));
        }

        protected abstract IHive CreateHive();

        public void Dispose()
        {
            if (_hive.IsValueCreated)
            {
                Hive.Dispose();
            }
        }
    }

    public class IronScriptHost : IronScriptHostBase
    {
        private readonly Guid _siteId;

        //public event EventHandler<SPItemEventProperties> ItemAdded;
        //public event EventHandler<SPItemEventProperties> ItemUpdated;
        //public event EventHandler<SPItemEventProperties> ItemDeleted;
        //public event EventHandler<SPItemEventProperties> ItemFileMoved;
        //public event EventHandler<SPItemEventProperties> ItemCheckedIn;
        //public event EventHandler<SPItemEventProperties> ItemAdding;
        //public event EventHandler<SPItemEventProperties> ItemUpdating;
        //public event EventHandler<SPItemEventProperties> ItemDeleting;
        //public event EventHandler<SPItemEventProperties> ItemFileMoving;
        //public event EventHandler<SPItemEventProperties> ItemCheckingIn;
        //public event EventHandler<SPItemEventProperties> ItemCheckingOut;

        public IronScriptHost(Guid siteId)
        {
            _siteId = siteId;
        }

        protected override IHive CreateHive()
        {
            var hives = GetHiveSetups().Select(x =>
                {
                    var argTypes = x.HiveArguments.Select(y => y.GetType()).ToArray();
                    var ctor = x.HiveType.GetConstructor(argTypes);
                    var hive = ctor != null ? (IHive) ctor.Invoke(x.HiveArguments) : null;
                    if (hive != null) hive.Name = x.DisplayName;

                    return hive;
                }).Compact().ToArray();

            var composite = new HiveComposite(hives);

            return composite;
        }

        private IEnumerable<HiveSetup> GetHiveSetups()
        {
            var registry = HiveRegistry.Local;
            HiveSetupCollection setups;
            if (!registry.TryResolve(_siteId, out setups))
            {
                setups = new HiveSetupCollection();
            }
            return setups;
        }

    }
}
