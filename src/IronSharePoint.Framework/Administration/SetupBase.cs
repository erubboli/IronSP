using System;
using Microsoft.SharePoint.Administration;

namespace IronSharePoint.Administration
{
    public class SetupBase : SPAutoSerializingObject 
    {
        [Persisted] 
        private Guid _id;
        [Persisted] 
        private string _displayName;
        [Persisted]
        private string _description;

        /// <summary>
        /// Display name of the Runtime
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        /// <summary>
        /// Optional description for the Runtime
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Id of the runtime
        /// </summary>
        public Guid Id
        {
            get { return _id; }
            internal set { _id = value; }
        }

        protected bool Equals(SetupBase other)
        {
            return _id.Equals(other._id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SetupBase) obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }
}