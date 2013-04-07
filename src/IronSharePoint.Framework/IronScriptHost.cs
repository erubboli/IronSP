using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronSharePoint.Administration;
using IronSharePoint.Hives;
using IronSharePoint.Util;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using IronSharePoint.Exceptions;

namespace IronSharePoint
{
    public class IronScriptHost : ScriptHost, IDisposable
    {
        private readonly Lazy<HiveComposite> _hive;
        private readonly HiveSetup[] _hiveSetups;
        private readonly Lazy<IronPlatformAdaptationLayer> _ironPlatformAdaptationLayer;

        public IronScriptHost(params HiveSetup[] hiveSetups)
        {
            _hiveSetups = hiveSetups.ToArray();
            _hive = new Lazy<HiveComposite>(CreateHive);
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

        public HiveComposite Hive
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

        protected HiveComposite CreateHive()
        {
            IHive[] hives = _hiveSetups
                .Select(x =>
                {
                    Type[] argTypes = x.HiveArguments == null || !x.HiveArguments.Any()
                                          ? Type.EmptyTypes
                                          : x.HiveArguments.Select(y => y.GetType()).ToArray();
                    ConstructorInfo ctor = x.HiveType.GetConstructor(argTypes);
                    if (ctor == null)
                    {
                        var argumentString = "null";
                        if (x.HiveArguments != null)
                        {
                            argumentString = String.Format("[{0}]", x.HiveArguments.StringJoin(", "));
                        }
                        var message =
                            string.Format("Could not instantiate Hive '{0}'. HiveType: {1}, HiveArguments: {2}",
                                          x.DisplayName,
                                          x.HiveType,
                                          argumentString);
                        throw new HiveInstantiationException(message);
                    }
                    var hive = (IHive) ctor.Invoke(x.HiveArguments);
                    hive.Name = x.DisplayName;
                    hive.Description = x.Description;
                    hive.Priority = x.Priority;

                    return hive;
                }).ToArray();

            var composite = new HiveComposite(hives);

            return composite;
        }
    }
}