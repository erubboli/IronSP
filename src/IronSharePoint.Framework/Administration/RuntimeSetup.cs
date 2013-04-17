using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using IronSharePoint.Util;
using Microsoft.SharePoint.Administration;

namespace IronSharePoint.Administration
{
    /// <summary>
    /// Stores information needed to setup a Runtime
    /// </summary>
    public class RuntimeSetup : SetupBase
    {
        [Persisted] private readonly List<Guid> _hiveIds;
        [Persisted] private readonly List<string> _gemPaths;
        [Persisted] private IronEnvironment? _environment;

        public RuntimeSetup()
        {
            _hiveIds = new List<Guid>();
            _gemPaths = new List<string> { IronConstant.GemsDirectory };
        }

        /// <summary>
        /// Get or set the <see cref="IronEnvironment"/> in which the runtime operates.
        /// Defaults to <see cref="IronRegistry.FarmEnvironment"/>
        /// </summary>
        public IronEnvironment Environment
        {
            get { return _environment.HasValue ? _environment.Value : IronRegistry.Local.FarmEnvironment; }
            set { _environment = value; }
        }

        /// <summary>
        /// Enumerates all HiveSetups added to this RuntimeSetup
        /// </summary>
        public IEnumerable<HiveSetup> Hives
        {
            get
            {
                var registry = IronRegistry.Local;
                return _hiveIds.Select(x => registry.Hives[(Guid) x]).Compact();
            }
        }

        /// <summary>
        /// List of gem paths to be used by the runtime
        /// </summary>
        public IEnumerable<string> GemPaths
        {
            get { return _gemPaths; }
        }

        public void AddHive(HiveSetup hiveSetup)
        {
            Contract.Requires<ArgumentNullException>(hiveSetup != null);

            AddHive(hiveSetup.Id);
        }

        public void AddHive(Guid hiveId)
        {
            Contract.Requires<ArgumentException>(hiveId != Guid.Empty);

            if (!_hiveIds.Contains(hiveId)) _hiveIds.Add(hiveId);
        }

        public void RemoveHive(HiveSetup hiveSetup)
        {
            Contract.Requires<ArgumentNullException>(hiveSetup != null);

            _hiveIds.Remove(hiveSetup.Id);
        }

        public void RemoveHive(Guid hiveId)
        {
            Contract.Requires<ArgumentException>(hiveId != Guid.Empty);

            _hiveIds.Remove(hiveId);
        }

        public void AddGemPath(string gemPath)
        {
            Contract.Requires<ArgumentException>(Path.IsPathRooted(gemPath));

            if (!_gemPaths.Contains(gemPath, StringComparer.InvariantCultureIgnoreCase))
            {
                _gemPaths.Add(gemPath);
            }
        }

        public void RemoveGemPath(string gemPath)
        {
            _gemPaths.Remove(gemPath);
        }
    }
}