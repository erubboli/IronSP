using System;
using System.Threading;
using IronSharePoint.IronConsole.Hooks;
using Microsoft.Scripting.Hosting;

namespace IronSharePoint.IronConsole
{
    class IronConsoleTask
    {
        static readonly IIronConsoleHook[] _hooks = new IIronConsoleHook[]
                                                    {
                                                        new RubyConsoleHook()
                                                    };

        readonly string _expression;
        readonly string _extension;
        readonly IronRuntime _runtime;
        readonly AutoResetEvent _waitHandle;
        private IronEngine _engine;

        public IronConsoleTask(IronRuntime runtime, string expression, string extension)
        {
            _runtime = runtime;
            _expression = expression;
            _extension = extension;
            _waitHandle = new AutoResetEvent(false);
        }

        public IronConsoleResult IronConsoleResult { get; private set; }

        public void Execute()
        {
            IronConsoleResult = new IronConsoleResult();
            try
            {
                _engine = _runtime.GetEngineByExtension(_extension, false);
                var scriptEngine = _engine.ScriptEngine;

                ExecuteBeforeHooks(scriptEngine, IronConsoleResult);

                var expressionResult = scriptEngine.Execute(_expression, _runtime.ScriptRuntime.Globals);
                IronConsoleResult.Result = expressionResult != null ? expressionResult.ToString() : null;

                ExecuteAfterHooks(scriptEngine, IronConsoleResult);
            }
            catch (Exception ex)
            {
                IronConsoleResult.Error = ex.Message;
                IronConsoleResult.StackTrace = ex.StackTrace;

                if (_engine != null)
                {
                    var eo = _engine.ScriptEngine.GetService<ExceptionOperations>();
                    IronConsoleResult.StackTrace = eo.FormatException(ex);
                }
            }
            finally
            {
                _waitHandle.Set();
            }
        }

        void ExecuteBeforeHooks(ScriptEngine scriptEngine, IronConsoleResult ironConsoleResult)
        {
            foreach (var hook in _hooks)
            {
                hook.BeforeExecute(scriptEngine, ironConsoleResult);
            }
        }

        void ExecuteAfterHooks(ScriptEngine scriptEngine, IronConsoleResult ironConsoleResult)
        {
            foreach (var hook in _hooks)
            {
                hook.AfterExecute(scriptEngine, ironConsoleResult);
            }
        }

        public void WaitFor(int timeout)
        {
            _waitHandle.WaitOne(5000);
        }
    }
}