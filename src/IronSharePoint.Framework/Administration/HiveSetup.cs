using System;
using System.Text;
using IronSharePoint.Hives;
using Microsoft.SharePoint.Administration;

namespace IronSharePoint.Administration
{
    /// <summary>
    /// Stores information needed to setup a Hive
    /// </summary>
    public class HiveSetup : SetupBase
    {
        [Persisted]
        private object[] _hiveArguments;
        [Persisted]
        private Type _hiveType;
        [Persisted]
        private int _priority;

        /// <summary>
        /// Priority of the hive. Files, which reside in multiple hives,
        /// will be loaded from the hive with the highest priority
        /// </summary>
        public int Priority
        {
            get { return _priority; }
            set { _priority = value; }
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
        /// Hive Setup for the IronSP Ruby Framework
        /// </summary>
        public static HiveSetup IronSPRoot
        {
            get
            {
                return new HiveSetup
                    {
                        Id = new Guid("56631301-08ED-4BDA-A1DD-0571308A55B4"),
                        DisplayName = "IronSP Root",
                        Description = "Contains the Ruby part of the IronSP Framework",
                        HiveArguments = new object[] {IronConstant.IronSPRootDirectory},
                        HiveType = typeof (DirectoryHive),
                        Priority = 1000
                    };
            }
        }
    }
}