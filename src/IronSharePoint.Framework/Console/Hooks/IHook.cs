using Microsoft.Scripting.Hosting;

namespace IronSharePoint.Console.Hooks
{
    interface IHook
    {
        bool SupportsLanguage(string languageName);

        void BeforeExecute(ScriptEngine scriptEngine, ScriptScope scope, ScriptResult result);
        void AfterExecute(ScriptEngine scriptEngine, ScriptScope scope, ScriptResult result);
    }
}