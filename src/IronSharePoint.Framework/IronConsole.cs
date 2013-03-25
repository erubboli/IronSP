using System;
using System.Diagnostics;
using System.Threading.Tasks;
using IronSharePoint.Console;
using IronSharePoint.Util;
using Microsoft.Scripting.Hosting;

namespace IronSharePoint
{
    public class IronConsole
    {
        private readonly ScriptRuntime _runtime;
        private readonly ScriptScope _scope;

        internal IronConsole(ScriptRuntime scriptRuntime)
        {
            _runtime = scriptRuntime;
            _scope = scriptRuntime.Globals;
        }

        public ScriptRuntime Runtime
        {
            get { return _runtime; }
        }

        public ScriptEngine Engine
        {
            get { return _runtime.GetRubyEngine(); }
        }

        public ExceptionOperations ExceptionOperations
        {
            get { return Engine.GetService<ExceptionOperations>(); }
        }

        public bool IsDisposed { get; private set; }

        public async Task<ScriptResult> Execute(string script)
        {
            return await Task.Run(() => RunScript(script));
        }

        private ScriptResult RunScript(string script)
        {
            var result = new ScriptResult();
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                using (var io = new IOCapture(Runtime))
                {
                    script = string.Format("_ = ({0}); _.inspect", script);
                    result.ReturnValue = Convert.ToString(Engine.Execute(script, _scope) ?? "nil");
                    io.Read();
                    result.Output = io.Out;
                    result.Error = io.Error;
                }
            }
            catch (Exception ex)
            {
                result.Error = ExceptionOperations.FormatException(ex);
            }
            sw.Stop();
            result.ExecutionTime = sw.ElapsedMilliseconds;

            return result;
        }
    }
}