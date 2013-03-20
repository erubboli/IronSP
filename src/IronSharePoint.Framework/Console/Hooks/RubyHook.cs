using System;
using System.Collections.Generic;
using Microsoft.Scripting.Hosting;

namespace IronSharePoint.Console.Hooks
{
    class RubyHook : HookBase
    {
        public override void AfterExecute(ScriptEngine scriptEngine, ScriptScope scope, ScriptResult result)
        {
            base.AfterExecute(scriptEngine, scope, result);
            var inspected = scriptEngine.Execute("_.inspect", scope);
            result.ReturnString = Convert.ToString(inspected ?? "nil");
        }

        protected override IEnumerable<string> SupportedLanguages()
        {
            return new[] {IronConstant.RubyLanguageName};
        }
    }
}