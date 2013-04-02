using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronSharePoint.Administration;
using IronSharePoint.Hives;
using IronSharePoint.Util;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace IronSharePoint
{
    public class IronScriptHost : ScriptHost, IDisposable
    {
        private readonly Lazy<IHive> _hive;
        private readonly HiveSetup[] _hiveSetups;
        private readonly Lazy<IronPlatformAdaptationLayer> _ironPlatformAdaptationLayer;

        public IronScriptHost(params HiveSetup[] hiveSetups)
        {
            _hiveSetups = hiveSetups.ToArray();
            _hive = new Lazy<IHive>(CreateHive);
            _ironPlatformAdaptationLayer =
                new Lazy<IronPlatformAdaptationLayer>(() => new IronPlatformAdaptationLayer(Hive));
        }

        public IronPlatformAdaptationLayer IronPlatformAdaptationLayer
        {
            get { return _ironPlatformAdaptationLayer.Value; }
        }

        public override PlatformAdaptationLayer PlatformAdaptationLayer
        {
            get { return IronPlatformAdaptationLayer; }
        }

        public IHive Hive
        {
            get { return _hive.Value; }
        }

        public void Dispose()
        {
            if (_hive.IsValueCreated)
            {
                Hive.Dispose();
            }
        }

        protected IHive CreateHive()
        {
            IHive[] hives = _hiveSetups.Select(x =>
            {
                Type[] argTypes = x.HiveArguments.Select(y => y.GetType()).ToArray();
                ConstructorInfo ctor = x.HiveType.GetConstructor(argTypes);
                IHive hive = ctor != null ? (IHive) ctor.Invoke(x.HiveArguments) : null;
                if (hive != null) hive.Name = x.DisplayName;

                return hive;
            }).Compact().ToArray();

            var composite = new HiveComposite(hives);

            return composite;
        }
    }
}