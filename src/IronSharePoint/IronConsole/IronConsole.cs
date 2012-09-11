using System;
using System.Collections.Generic;
using System.Threading;
using IronSharePoint.Util;

namespace IronSharePoint.IronConsole
{
    public class IronConsole : IDisposable
    {
        // HiveId -> IIronConsole
        static readonly IDictionary<Guid, IronConsole> _hiveConsoles = new Dictionary<Guid, IronConsole>();

        readonly IBlockingQueue<IronConsoleTask> _queue;
        readonly Thread _queueWorker;
        readonly IronRuntime _runtime;

        static IronConsole()
        {
            _hiveConsoles = new Dictionary<Guid, IronConsole>();
        }

        protected IronConsole(IronRuntime runtime, params string[] supportedExtensions)
        {
            _runtime = runtime;
            _queue = new BlockingQueue<IronConsoleTask>();
            _queueWorker = new Thread(ProcessTaskQueue)
                           {
                               Name = string.Format("IronConsole Worker for {0}",
                                   string.Join(", ", supportedExtensions)),
                               IsBackground = true
                           };
            _queueWorker.Start();
        }

        public IronRuntime Runtime
        {
            get { return _runtime; }
        }

        public bool IsDisposed { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                _queue.Stop();
                _queueWorker.Join(5000);
            }
        }

        #endregion

        public static IronConsole GetConsoleForRuntime(IronRuntime runtime)
        {
            var hiveId = runtime.IronHive.Id;
            IronConsole console;
            if (!_hiveConsoles.TryGetValue(hiveId, out console) || console.IsDisposed)
            {
                console = new IronConsole(runtime);
                _hiveConsoles[hiveId] = console;
            }

            return console;
        }

        void ProcessTaskQueue()
        {
            IronConsoleTask task;
            while ((task = _queue.Dequeue()) != null)
            {
                task.Execute();
            }
        }

        public IronConsoleResult Execute(string expression, string extension)
        {
            var task = new IronConsoleTask(Runtime, expression, extension);
            _queue.Enqueue(task);
            task.WaitFor(5000);

            return task.IronConsoleResult;
        }
    }
}