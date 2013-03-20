using Microsoft.Scripting.Hosting;

namespace IronSharePoint.Framework.Test
{
    public static class TestHelper
    {
        public static ScriptRuntime CreateRubyRuntime()
        {
            var setup = new ScriptRuntimeSetup();
            var languageSetup = new LanguageSetup(
                "IronRuby.Runtime.RubyContext, IronRuby, Version=1.1.3.0, Culture=neutral, PublicKeyToken=7f709c5b713576e1",
                IronConstant.RubyLanguageName,
                new[] { "IronRuby", "Ruby", "rb" },
                new[] { ".rb" });
            setup.LanguageSetups.Add(languageSetup);
            setup.HostType = typeof(ScriptHost);
            setup.DebugMode = true;

            return new ScriptRuntime(setup);
        }
    }
}