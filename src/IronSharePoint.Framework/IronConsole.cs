using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IronSharePoint.Console;
using IronSharePoint.Console.Hooks;
using IronSharePoint.Util;
using Microsoft.Scripting.Hosting;

namespace IronSharePoint
{
    public class IronConsole
    {
        private readonly ScriptRuntime _runtime;
        private readonly IHook[] _hooks;

        internal IronConsole(ScriptRuntime scriptRuntime)
        {
            _runtime = scriptRuntime;
            _hooks = new IHook[]
                {
                    new RubyHook()
                };
        }

        public ScriptRuntime Runtime
        {
            get { return _runtime; }
        }

        public bool IsDisposed { get; private set; }

        public async Task<ScriptResult> Execute(string script, string languageName)
        {
            return await Task.Run(() => RunScript(script, languageName));
        }

        private ScriptResult RunScript(string script, string languageName)
        {
            var result = new ScriptResult();
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                var engine = Runtime.GetEngine(languageName);
                var scope = engine.CreateScope();

                ExecuteBeforeHooks(engine, result, scope);

                using (var io = new IOCapture(Runtime))
                {
                    script = string.Format("_ = ({0})", script);
                    result.ReturnValue = engine.Execute(script, scope);
                    io.Read();
                    result.Output = io.Out;
                    result.Error = io.Error;
                }

                ExecuteAfterHooks(engine, result, scope);


            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                result.StackTrace = ex.StackTrace;
            }
            sw.Stop();
            result.ExecutionTime = sw.ElapsedMilliseconds;

            return result;
        }

        private void ExecuteBeforeHooks(ScriptEngine engine, ScriptResult result, ScriptScope scope)
        {
            var languageName = engine.Setup.DisplayName;

            foreach (var hook in _hooks.Where(x => x.SupportsLanguage(languageName)))
            {
                hook.BeforeExecute(engine, scope, result);
            }
        }

        private void ExecuteAfterHooks(ScriptEngine engine, ScriptResult result, ScriptScope scope)
        {
            var languageName = engine.Setup.DisplayName;

            foreach (var hook in _hooks.Where(x => x.SupportsLanguage(languageName)))
            {
                hook.AfterExecute(engine, scope, result);
            }
        }
    }

    internal class IOCapture : IDisposable
    {
        public IOCapture(ScriptRuntime runtime)
        {
            _runtime = runtime;
            _outBackup = runtime.IO.OutputStream;
            _outEncoding = runtime.IO.OutputEncoding;
            _errBackup = runtime.IO.ErrorStream;
            _errEncoding = runtime.IO.ErrorEncoding;

            _out = new MemoryStream();
            _err = new MemoryStream();

            _runtime.IO.SetOutput(_out, _outEncoding);
            _runtime.IO.SetErrorOutput(_err, _errEncoding);
        }

        private readonly Encoding _errEncoding;
        private readonly Stream _errBackup;
        private readonly Encoding _outEncoding;
        private readonly Stream _outBackup;

        private readonly MemoryStream _out;
        private readonly MemoryStream _err;
        private readonly ScriptRuntime _runtime;

        public void Read()
        {
            _runtime.IO.OutputWriter.Flush();
            _runtime.IO.ErrorWriter.Flush();
            _out.Position = 0;
            _err.Position = 0;
            using (var sr = new StreamReader(_out))
            {
                Out = sr.ReadToEnd();
            }
            using (var sr = new StreamReader(_err))
            {
                Error = sr.ReadToEnd();
            }
        }

        public string Out { get; private set; }
        public string Error { get; private set; }

        public void Dispose()
        {
            _runtime.IO.SetOutput(_outBackup, _outEncoding);
            _runtime.IO.SetErrorOutput(_errBackup, _errEncoding);

            _out.Dispose();
            _err.Dispose();
        }
    }
}