using FluentAssertions;
using IronSharePoint.Console;
using NUnit.Framework;

namespace IronSharePoint.Framework.Test.Console
{
    [TestFixture]
    public class ScriptResult_Fixture
    {
        public ScriptResult Sut;

        [Test]
        public void ToJson_IgnoresReturnValue()
        {
            Sut = new ScriptResult
                {
                    Output = "foo",
                    ReturnValue = "ReturnValue"
                };

            System.Console.WriteLine(Sut.ToJson());
            Sut.ToJson().Should().NotContain("ReturnValue");
        }
         
    }
}