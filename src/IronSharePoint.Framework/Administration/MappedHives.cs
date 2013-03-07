using System;
using System.Collections.ObjectModel;

namespace IronSharePoint.Framework.Administration
{
    [Serializable]
    public class MappedHives : Collection<HiveDescription>
    {
        private readonly IronHiveRegistry _registry;

        public MappedHives(IronHiveRegistry registry)
            : base()
        {
            _registry = registry;
        }

        protected override void InsertItem(int index, HiveDescription item)
        {
            _registry.EnsureTrustedHive(item);
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, HiveDescription item)
        {
            _registry.EnsureTrustedHive(item);
            base.SetItem(index, item);
        }
    }
}