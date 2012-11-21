using System;
using System.Collections.Generic;
using Microsoft.Scripting.Hosting;

namespace IronSharePoint.IronConsole.Hooks
{
    class RubyConsoleHook : IronConsoleHookBase
    {
        public override void BeforeExecute(ScriptEngine scriptEngine, IronConsoleResult result)
        {
            base.BeforeExecute(scriptEngine, result);
            scriptEngine.Execute(
                @"
                    unless respond_to? :console_out
                      begin
                        include IronSharePoint::IronConsole::Utils 
                      rescue
                        raise 'Could not load IronConsole Utils'
                      end
                    end");
        }

        public override void AfterExecute(ScriptEngine scriptEngine, IronConsoleResult result)
        {
            base.AfterExecute(scriptEngine, result);
            var consoleOut = scriptEngine.Execute(String.Format("console_out '{0}'", Environment.NewLine));
            result.Output = Convert.ToString(consoleOut);
            result.Result = result.Result ?? "nil";
        }

        protected override IEnumerable<string> SupportedExtensions()
        {
            return new[] {".rb", ".ruby"};
        }
    }
}