using System;

namespace IronSharePoint.Framework.Administration
{
    [Serializable]
    public class HiveDescription
    {
        /// <summary>
        /// Id of the hive. Used as the contructor parameter
        /// </summary>
        public object Id { get; set; }

        /// <summary>
        /// Type of the hive used for creating instances
        /// </summary>
        public Type HiveType { get; set; }

        /// <summary>
        /// Human readable description of the hive
        /// </summary>
        public string Description { get; set; }

        protected bool Equals(HiveDescription other)
        {
            return Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HiveDescription) obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}