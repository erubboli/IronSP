using System;
using System.Linq;
using IronSharePoint.Exceptions;
using IronSharePoint.Util;
using Microsoft.Scripting.Hosting;
using Moq;
using NUnit.Framework;

namespace IronSharePoint.Framework.Test.Util
{
    [TestFixture]
    public class ScriptEngineExtensions_Fixture
    {
        public ScriptEngine Sut;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Sut = TestHelper.CreateRubyEngine();
        }

        [Test]
        public void CreateInstance_WithExistingType_ReturnsInstance()
        {
            var instance = Sut.CreateInstance("System::String", "foo");

            Assert.AreEqual(instance, "foo");
        }

        [Test, ExpectedException(typeof (DynamicInstanceInitializationException))]
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