using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Scripting.Hosting;

namespace IronSharePoint.IronConsole.Hooks
{
    abstract class IronConsoleHookBase : IIronConsoleHook
    {
        #region IIronConsoleHook Members

        public bool SupportsExtension(string extension)
        {
            return SupportedExtensions().Contains(extension, StringComparer.InvariantCultureIgnoreCase);
        }

        public virtual void BeforeExecute(ScriptEngine scriptEngine, IronConsoleResult result) {}

        public virtual void AfterExecute(ScriptEngine scriptEngine, IronConsoleResult result) {}

        #endregion

        protected abstract IEnumerable<string> SupportedExtensions();
    }
}