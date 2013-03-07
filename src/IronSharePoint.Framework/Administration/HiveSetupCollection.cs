using System.Collections.ObjectModel;

namespace IronSharePoint.Administration
{
    /// <summary>
    /// Custom collection for <see cref="HiveSetup"/>s that perform additional security checks on its operations when
    /// a <see cref="Administration.IronHiveRegistry"/> is set. Also implements some convience methods for changing the
    /// order of the items
    /// </summary>
    public class HiveSetupCollection : Collection<HiveSetup>
    {
        public IronHiveRegistry Registry
        {
            get; internal set;
        }

        protected override void InsertItem(int index, HiveSetup item)
        {
            EnsureTrustedHive(item);
            item.Priority = index;
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, HiveSetup item)
        {
            EnsureTrustedHive(item);
            item.Priority = index;
            base.SetItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            UpdateAllIndices();
        }

        public void Swap(int index1, int index2)
        {
            var tmp = this[index1];
            SetItem(index1, this[index2]);
            SetItem(index2, tmp);
        }

        public void MoveUp(int index)
        {
            if (index > 0)
            {
                var tmp = this[index];
                RemoveItem(index);
                InsertItem(index-1, tmp);
            }
        }

        public void MoveDown(int index)
        {
            if (index < Count-1)
            {
                var tmp = this[index];
                RemoveItem(index);
                InsertItem(index+1, tmp);
            }
        }

        private void EnsureTrustedHive(HiveSetup item)
        {
            if (Registry != null) Registry.EnsureTrustedHive(item);
        }

        private void UpdateAllIndices()
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].Priority = i;
            }
        }
    }
}