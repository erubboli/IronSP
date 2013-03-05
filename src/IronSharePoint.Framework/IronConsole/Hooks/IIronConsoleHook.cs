using Microsoft.Scripting.Hosting;

namespace IronSharePoint.IronConsole.Hooks
{
    interface IIronConsoleHook
    {
        bool SupportsExtension(string extension);

        void BeforeExecute(ScriptEngine scriptEngine, IronConsoleResult result);
        void AfterExecute(ScriptEngine scriptEngine, IronConsoleResult result);
    }
}