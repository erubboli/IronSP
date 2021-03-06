﻿using System;
using System.Threading;
using FluentAssertions;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;
using IronSharePoint.Util;

namespace IronSharePoint.Framework.Test
{
    [TestFixture]
    public class IronConsole_Fixture
    {
        public IronConsole Sut;
        public ScriptRuntime ScriptRuntime;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            ScriptRuntime = TestHelper.CreateRubyRuntime();
        }

        [SetUp]
        public void SetUp()
        {
            Sut = new IronConsole(ScriptRuntime);
        }

        [Test]
        public void Execute_SetsOutputOnResult()
        {
            var result = Sut.Execute("puts 'foo'").Result;

            result.Output.Should().Be("foo\r\n");
        }

        [Test]
        public void Execute_SetsErrorOnResult()
        {
            var result = Sut.Execute("$stderr.puts 'foo'").Result;

            result.Error.Should().Be("foo\r\n");
        }

        [Test]
        public void Execute_SetsReturnValueOnResult()
        {
            var result = Sut.Execute("1").Result;

            result.ReturnValue.Should().Be("1");
        }

        [Test]
        public void Execute_SetsExecutionTimeOnResult()
        {
            var result = Sut.Execute("sleep 0.01 # 10 ms").Result;
            result.ExecutionTime.Should().BeGreaterOrEqualTo(10);
        }

        [Test]
        public void Execute_AssignsResultToUnderscoreInGlobalScope()
        {
            Sut.Execute("1+1").Wait();

            Assert.AreEqual(Sut.Execute("_").Result.ReturnValue, "2");
        }
    }
}