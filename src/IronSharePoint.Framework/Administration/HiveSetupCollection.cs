using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronSharePoint.Administration
{
    /// <summary>
    /// Custom collection for <see cref="HiveSetup"/>s that perform additional security checks on its operations when
    /// a <see cref="HiveRegistry"/> is set. Also implements some convience methods for changing the
    /// order of the items
    /// </summary>
    public class HiveSetupCollection : Collection<HiveSetup>
    {
        public HiveRegistry Registry
        {
            get; internal set;
        }

        public void AddRange(IEnumerable<HiveSetup> setups)
        {
            foreach (var setup in setups)
            {
                Add(setup);
            }
        }

        protected override void InsertItem(int index, HiveSetup item)
        {
            EnsureTrustedHive(item);
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, HiveSetup item)
        {
            EnsureTrustedHive(item);
            base.SetItem(index, item);
        }

        private void EnsureTrustedHive(HiveSetup item)
        {
            if (Registry != null) Registry.EnsureTrustedHive(item);
        }
    }
}