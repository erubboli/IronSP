using System;
using System.Collections.Generic;
using IronSharePoint.Hives;
using Microsoft.SharePoint.Administration;
using System.Linq;

namespace IronSharePoint.Administration
{
    /// <summary>
    /// Stores information needed to setup a Hive
    /// </summary>
    public class HiveSetup : SPAutoSerializingObject
    {
        [Persisted]
        private object[] _hiveArguments;
        [Persisted]
        private Type _hiveType;
        [Persisted]
        private string _displayName;
        [Persisted]
        private string _description;
        [Persisted]
        private int _order;

        /// <summary>
        /// Display name of the hive
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        /// <summary>
        /// Arguments passed to the hive type when it is constructed
        /// </summary>
        public object[] HiveArguments
        {
            get { return _hiveArguments; }
            set { _hiveArguments = value; }
        }

        /// <summary>
        /// Type of the hive. Must implement <see cref="IHive"/>
        /// </summary>
        public Type HiveType
        {
            get { return _hiveType; }
            set
            {
                if (!typeof(IHive).IsAssignableFrom(value))
                {
                    throw new ArgumentException("Type doesn't implement IHive", "value");
                }
                _hiveType = value;
            }
        }

        /// <summary>
        /// Additional Information describing this hive setup
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Priority of the hive when accessing files. The lower the number, the higher the priority.
        /// Will be set by the <see cref="HiveSetupCollection"/> depending on its position.
        /// </summary>
        public int Priority
        {
            get { return _order; }
            internal set { _order = value; }
        }

        protected bool Equals(HiveSetup other)
        {
            return (_hiveArguments ?? new object[0]).SequenceEqual(other._hiveArguments ?? new object[0]) 
                && Equals(_hiveType, other._hiveType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HiveSetup) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_hiveArguments != null ? _hiveArguments.GetHashCode() : 0)*397) ^ (_hiveType != null ? _hiveType.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// Hive Setup for the IronSP Ruby Framework
        /// </summary>
        public static HiveSetup IronSPRoot
        {
            get
            {
                return new HiveSetup
                    {
                        DisplayName = "IronSP Root",
                        Description = "Contains the Ruby part of the IronSP Framework",
                        HiveArguments = new object[] {IronConstant.IronSPRootDirectory},
                        HiveType = typeof (PhysicalHive)
                    };
            }
        }
    }
}