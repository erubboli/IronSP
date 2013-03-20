using System;
using IronSharePoint.Util;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;

namespace IronSharePoint.Framework.Test.Util
{
    [TestFixture]
    public class ScriptEngineExtensions_Fixture
    {
        public ScriptRuntime ScriptRuntime;
        public ScriptEngine Sut;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            ScriptRuntime = TestHelper.CreateRubyRuntime();
            Sut = ScriptRuntime.GetEngine(IronConstant.RubyLanguageName);
        }

        [Test]
        public void CreateInstance_WithExistingType_ReturnsInstance()
        {
            var instance = Sut.CreateInstance("System::String", "foo");

            Assert.AreEqual(instance, "foo");
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void CreateInstance_WithUnknownType_ThrowsArgumentException()
        {
            Sut.CreateInstance("FOO", "foo");
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void CreateInstance_ThrowsArgumentNullException()
        {
            Sut.CreateInstance(null);
        }
    }
}