using FluentAssertions;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;

namespace IronSharePoint.Framework.Test
{
    [TestFixture]
    public class IOCapture_Fixture
    {
        internal IOCapture Sut;
        public ScriptRuntime ScriptRuntime;

        [SetUp]
        public void SetUp()
        {
            ScriptRuntime = TestHelper.CreateRubyRuntime();
        }

        [Test]
        public void ReplacesOutputStreamInScope()
        {
            var originalOut = ScriptRuntime.IO.OutputStream;

            using (Sut = new IOCapture(ScriptRuntime))
            {
                ScriptRuntime.IO.OutputStream.Should().NotBeSameAs(originalOut);
            }
        }

        [Test]
        public void ReplacesErrorStreamInScope()
        {
            var originalOut = ScriptRuntime.IO.ErrorStream;

            using (Sut = new IOCapture(ScriptRuntime))
            {
                ScriptRuntime.IO.ErrorStream.Should().NotBeSameAs(originalOut);
            }
        }

        [Test]
        public void Dispose_SetsOutputStreamBackToOriginal()
        {
            var originalOut = ScriptRuntime.IO.OutputStream;

            using (Sut = new IOCapture(ScriptRuntime)) {}
            
            ScriptRuntime.IO.OutputStream.Should().BeSameAs(originalOut);
        }

        [Test]
        public void Dispose_SetsErrorStreamBackToOriginal()
        {
            var originalOut = ScriptRuntime.IO.ErrorStream;

            using (Sut = new IOCapture(ScriptRuntime)) { }

            ScriptRuntime.IO.ErrorStream.Should().BeSameAs(originalOut);
        }

        [Test]
        public void Read_WritesCapturedOutputToProperty()
        {
            using (Sut = new IOCapture(ScriptRuntime))
            {
                ScriptRuntime.IO.OutputWriter.Write("foo");
                Sut.Read();
                Sut.Out.Should().Be("foo");
            }
        }

        [Test]
        public void Read_WritesCapturedErrorToProperty()
        {
            using (Sut = new IOCapture(ScriptRuntime))
            {
                ScriptRuntime.IO.ErrorWriter.Write("foo");
                Sut.Read();
                Sut.Error.Should().Be("foo");
            }
        }
    }
}