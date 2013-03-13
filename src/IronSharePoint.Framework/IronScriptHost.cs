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
    public class IronScriptHost : ScriptHost
    {
        private readonly IronPlatformAdaptationLayer _ironPlatformAdaptationLayer;
        private readonly Guid _siteId;
        private readonly IHive _hive;

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

        public IHive Hive
        {
            get { return _hive; }
        }

        public IronScriptHost(Guid siteId)
        {
            _siteId = siteId;
            _hive = CreateHive();
            _ironPlatformAdaptationLayer = new IronPlatformAdaptationLayer(Hive);
        }

        public override Microsoft.Scripting.PlatformAdaptationLayer PlatformAdaptationLayer
        {
            get { return _ironPlatformAdaptationLayer; }
        }

        private IHive CreateHive()
        {
            var hives = GetHiveSetups().Select(x =>
                {
                    var argTypes = x.HiveArguments.Select(y => y.GetType()).ToArray();
                    var ctor = x.HiveType.GetConstructor(argTypes);
                    return ctor != null ? (IHive) ctor.Invoke(x.HiveArguments) : null;
                }).Compact().ToArray();

            return new OrderedHiveList(hives);
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
