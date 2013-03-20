using FluentAssertions;
using IronSharePoint.Console;
using IronSharePoint.Console.Hooks;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;

namespace IronSharePoint.Framework.Test.Console.Hooks
{
    [TestFixture]
    public class RubyHook_Fixture
    {
        internal RubyHook Sut;
        public ScriptRuntime ScriptRuntime;
        public ScriptEngine ScriptEngine;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            ScriptRuntime = TestHelper.CreateRubyRuntime();
            ScriptEngine = ScriptRuntime.GetEngine(IronConstant.RubyLanguageName);
        }

        [SetUp]
        public void SetUp()
        {
            Sut = new RubyHook();
        }

        [Test]
        public void AfterExecute_InspectsReturnValueAndSetsReturnString()
        {
            var scope = ScriptEngine.CreateScope();
            var returnValue = ScriptEngine.Execute("_ = Object.new", scope);
            var scriptResult = new ScriptResult {ReturnValue = returnValue};

            Sut.AfterExecute(ScriptEngine, scope, scriptResult);

            scriptResult.ReturnString.Should().Match("#<Object:0x*>");
        }

        [Test]
        public void SupportsRuby()
        {
            Sut.SupportsLanguage(IronConstant.RubyLanguageName).Should().BeTrue();
        }
    }
}