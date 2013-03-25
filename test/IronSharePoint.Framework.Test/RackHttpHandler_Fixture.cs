using System;
using System.Web;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using NUnit.Framework;
using FluentAssertions;
using TypeMock.ArrangeActAssert;

namespace IronSharePoint.Framework.Test
{
    [TestFixture]
    public class RackHttpHandler_Fixture
    {
        public RackHttpHandler Sut;
        public IronRuntime IronRuntime;
        public HttpContext HttpContext;


        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Isolate.Fake.StaticMethods<SPContext>(Members.ReturnRecursiveFakes);

            HttpContext = Isolate.Fake.Instance<HttpContext>();
            IronRuntime = Isolate.Fake.Instance<IronRuntime>();
            Isolate.WhenCalled(() => IronRuntime.GetDefaultIronRuntime(null)).WillReturn(IronRuntime);
        }

        [SetUp]
        public void SetUp()
        {
            Sut = new RackHttpHandler();
        }

        [Test]
        public void IsReusable_ReturnsTrue()
        {
            Sut.IsReusable.Should().BeTrue();
        }

        [Test]
        public void Process_AddsHttpContextToRubyScope()
        {
            var scope = Isolate.Fake.Instance<ScriptScope>();
            Isolate.WhenCalled(() => IronRuntime.RubyEngine.CreateScope()).WillReturn(scope);

            Sut.ProcessRequest(HttpContext);

            Isolate.Verify.WasCalledWithExactArguments(() => scope.SetVariable("ctx", HttpContext));
        }
    }
}