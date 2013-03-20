using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Scripting.Hosting;

namespace IronSharePoint.Console.Hooks
{
    abstract class HookBase : IHook
    {
        #region IHook Members

        public bool SupportsLanguage(string languageName)
        {
            return SupportedLanguages().Contains(languageName, StringComparer.InvariantCultureIgnoreCase);
        }

        public virtual void BeforeExecute(ScriptEngine scriptEngine, ScriptScope scope, ScriptResult result) {}

        public virtual void AfterExecute(ScriptEngine scriptEngine, ScriptScope scope, ScriptResult result) {}

        #endregion

        protected abstract IEnumerable<string> SupportedLanguages();
    }
}