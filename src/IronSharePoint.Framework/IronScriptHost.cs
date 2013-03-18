using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronSharePoint.Administration;
using IronSharePoint.Hives;
using IronSharePoint.Util;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace IronSharePoint
{
    public class IronScriptHost : ScriptHost, IDisposable
    {
        private readonly IronPlatformAdaptationLayer _ironPlatformAdaptationLayer;
        private readonly Guid _siteId;
        private readonly HiveComposite _hive;

        public event EventHandler<SPItemEventProperties> ItemAdded;
        public event EventHandler<SPItemEventProperties> ItemUpdated;
        public event EventHandler<SPItemEventProperties> ItemDeleted;
        public event EventHandler<SPItemEventProperties> ItemFileMoved;
        public event EventHandler<SPItemEventProperties> ItemCheckedIn;
        public event EventHandler<SPItemEventProperties> ItemAdding;
        public event EventHandler<SPItemEventProperties> ItemUpdating;
        public event EventHandler<SPItemEventProperties> ItemDeleting;
        public event EventHandler<SPItemEventProperties> ItemFileMoving;
        public event EventHandler<SPItemEventProperties> ItemCheckingIn;
        public event EventHandler<SPItemEventProperties> ItemCheckingOut;

        public HiveComposite Hive
        {
            get { return _hive; }
        }

        public IronScriptHost(Guid siteId)
        {
            _siteId = siteId;
            _hive = CreateHive();
            _ironPlatformAdaptationLayer = new IronPlatformAdaptationLayer(Hive);
        }

        public IronPlatformAdaptationLayer IronPlatformAdaptationLayer
        {
            get { return _ironPlatformAdaptationLayer; }
        }

        public override Microsoft.Scripting.PlatformAdaptationLayer PlatformAdaptationLayer
        {
            get { return IronPlatformAdaptationLayer; }
        }

        private HiveComposite CreateHive()
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
            composite.Append(new SystemHive());

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

        public void Dispose()
        {
            if (_hive != null)
            {
                _hive.Dispose();
            }
        }
    }
}
