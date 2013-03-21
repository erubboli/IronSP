using System.IO;
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
        public void ToJson()
        {
            Sut = new ScriptResult
                {
                    Error = "Error",
                    Output = "Output",
                    ReturnValue = "ReturnValue",
                    ExecutionTime = 1,
                    StackTrace = "StackTrace"
                };

            System.Console.WriteLine(Sut.ToJson());
            Sut.ToJson().Should().Be(File.ReadAllText("./_assets/script_result.json"));
        }
    }
}