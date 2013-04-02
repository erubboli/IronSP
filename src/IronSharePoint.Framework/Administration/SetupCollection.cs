using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint.Administration;

namespace IronSharePoint.Administration
{
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
}