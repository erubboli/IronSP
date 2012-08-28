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
        private static readonly Dictionary<Guid, ScriptRuntime> _runningRuntimes = new Dictionary<Guid, ScriptRuntime>();

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

        public static IronRuntime GetRuntime(SPSite hiveSite)
        {
            var ironRuntime = new IronRuntime();
            
            if (_runningRuntimes.ContainsKey(hiveSite.ID))
            {
                ironRuntime._scriptRuntime = _runningRuntimes[hiveSite.ID];
            }
            else
            { 
                // create new runtime
                var setup = new ScriptRuntimeSetup();
                setup.LanguageSetups.Add(new LanguageSetup(
                       "IronRuby.Runtime.RubyContext, IronRuby, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", IronConstants.IronRubyLanguageName,
                             new List<String>() { "IronRuby", "Ruby", "rb" }, new List<String>() { ".rb" }));
                 
                setup.HostType = typeof(IronHost);

                ironRuntime._scriptRuntime = new ScriptRuntime(setup);

                _runningRuntimes.Add(hiveSite.ID, ironRuntime._scriptRuntime);
            }

            ironRuntime._host = ironRuntime._scriptRuntime.Host as IronHost;
            ironRuntime._host.SetHiveSite(hiveSite);

            return ironRuntime;
        }

    }
}
