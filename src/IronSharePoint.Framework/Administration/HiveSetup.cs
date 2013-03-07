using System;
using Microsoft.SharePoint.Administration;

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
    }
}