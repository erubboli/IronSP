using System;
using System.IO;
using System.Text;
using Microsoft.Scripting.Hosting;

namespace IronSharePoint.Console
{
    internal class IOCapture : IDisposable
    {
        private readonly MemoryStream _err;
        private readonly Stream _errBackup;
        private readonly Encoding _errEncoding;

        private readonly MemoryStream _out;
        private readonly Stream _outBackup;
        private readonly Encoding _outEncoding;
        private readonly ScriptRuntime _runtime;

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

        public string Out { get; private set; }
        public string Error { get; private set; }

        public void Dispose()
        {
            _runtime.IO.SetOutput(_outBackup, _outEncoding);
            _runtime.IO.SetErrorOutput(_errBackup, _errEncoding);

            _out.Dispose();
            _err.Dispose();
        }

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
    }
}