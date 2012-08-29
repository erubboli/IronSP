using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;

namespace IronSharePoint
{
    public class IronRuntime
    {
        // the key is the ID of the hive
        private static readonly Dictionary<Guid, IronRuntime> _runningRuntimes = new Dictionary<Guid, IronRuntime>();

        private ScriptRuntime _scriptRuntime;

        public ScriptRuntime ScriptRuntime
        {
            get { return _scriptRuntime; }
        }

        private IronHost _host;

        public IronHost Host
        {
            get { return _host; }
        }

        private Dictionary<String, IronEngine> _runningEngines = new Dictionary<String, IronEngine>();

        internal Dictionary<String, IronEngine> RunningEngines
        {
            get { return _runningEngines; }
            set { _runningEngines = value; }
        }

        public static IronRuntime GetRuntime(SPSite hiveSite)
        {
            IronRuntime ironRuntime = null;
            


            if (_runningRuntimes.ContainsKey(hiveSite.ID))
            {
                ironRuntime = _runningRuntimes[hiveSite.ID];
            }
            else
            {
                ironRuntime = new IronRuntime();
                // create new runtime
                var setup = new ScriptRuntimeSetup();
                setup.LanguageSetups.Add(new LanguageSetup(
                       "IronRuby.Runtime.RubyContext, IronRuby, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", IronConstants.IronRubyLanguageName,
                             new List<String>() { "IronRuby", "Ruby", "rb" }, new List<String>() { ".rb" }));

                setup.HostType = typeof(IronHost);

#if DEBUG
                setup.DebugMode = true;
#endif

                ironRuntime._scriptRuntime = new ScriptRuntime(setup);               

                _runningRuntimes.Add(hiveSite.ID, ironRuntime);
            }

            ironRuntime._host = ironRuntime._scriptRuntime.Host as IronHost;
            ironRuntime._host.SetHiveSite(hiveSite);

            return ironRuntime;
        }

        private IronRuntime()
        {

        }

    }
}
