using Microsoft.Scripting.Hosting;

namespace IronSharePoint.Util
{
    public static class ScriptRuntimeExtensions
    {
        public static ScriptEngine GetRubyEngine(this ScriptRuntime scriptRuntime)
        {
            return scriptRuntime.GetEngine(IronRuntime.RubyEngineName);
        }
    }
}