using FluentAssertions;
using IronSharePoint.Util;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;
using TypeMock.ArrangeActAssert;

namespace IronSharePoint.Framework.Test.Util
{
    [TestFixture]
    public class ScriptRuntimeExtensions_Fixture
    {
        [Test]
        public void GetRubyEngine()
        {
            var rubyEngine = Isolate.Fake.Instance<ScriptEngine>();
            var sut = Isolate.Fake.Instance<ScriptRuntime>();
            Isolate.WhenCalled(() => sut.GetEngine(IronRuntime.RubyEngineName))
                   .WithExactArguments()
                   .WillReturn(rubyEngine);

            sut.GetRubyEngine().Should().Be(rubyEngine);
        }
    }
}