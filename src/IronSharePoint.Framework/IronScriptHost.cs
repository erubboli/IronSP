using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronSharePoint.Administration;
using IronSharePoint.Hives;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint.Administration;

namespace IronSharePoint
{
    public class IronScriptHost : ScriptHost
    {
        private readonly IronPlatformAdaptationLayer _ironPlatformAdaptationLayer;
        private readonly Guid _siteId;
        private readonly IHive _hive;

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
            var hives = CreateHivesFromRegistry().ToList();

            if (!hives.Any(x => x is PhysicalHive && 
                (x as PhysicalHive).Root == IronConstant.IronSPRootDirectory))
            {
                var spRootHive = new PhysicalHive(IronConstant.IronSPRootDirectory);
                hives.Add(spRootHive);
            }

            return new OrderedHiveList(hives.ToArray());
        }

        private IEnumerable<IHive> CreateHivesFromRegistry()
        {
            var registry = HiveRegistry.Local;
            try
            {
                var descriptions = registry.GetMappedHivesForSite(_siteId).OrderBy(x => x.Priority);
                return descriptions.Select(x =>
                    {
                        var argTypes = x.HiveArguments.GetType();
                        var ctor = x.HiveType.GetConstructor(new[] {argTypes});
                        return (IHive) ctor.Invoke(new[] {x.HiveArguments});
                    });
            }
            catch (ArgumentException)
            {
                return new IHive[0];
            }
        }
    }
}
