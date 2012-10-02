﻿using IronSharePoint.IronConsole;
using NUnit.Framework;

namespace IronSharePoint.Test.Unit.IronConsole
{
    [TestFixture]
    public class IronConsoleResult_Fixture
    {
        IronConsoleResult Sut;

        #region SetUp & TearDown

        [TestFixtureSetUp]
        public void FixtureSetUp() {}

        [TestFixtureTearDown]
        public void FixureTearDown() {}

        [SetUp]
        public void SetUp()
        {
            Sut = new IronConsoleResult
                  {
                      Error = "Error",
                      Output = "Output",
                      Result = "Result",
                      StackTrace = "StackTrace"
                  };
        }

        [TearDown]
        public void TearDown() {}

        #endregion

        [Test]
        public void ToJson()
        {
           Assert.AreEqual(Assets.Load("response.json"), Sut.ToJson()); 
        }

        [Test]
        public void FromJson()
        {
            Sut = IronConsoleResult.FromJson(Assets.Load("response.json"));

            Assert.AreEqual("Error", Sut.Error);
            Assert.AreEqual(true, Sut.HasError);
            Assert.AreEqual("Output", Sut.Output);
            Assert.AreEqual("Result", Sut.Result);
            Assert.AreEqual("StackTrace", Sut.StackTrace);
        }

        [Test]
        public void HasError_True()
        {
            Sut.Error = "Error";
            Assert.IsTrue(Sut.HasError);
        }

        [Test]
        public void HasError_False()
        {
            Sut.Error = null;
            Assert.IsFalse(Sut.HasError);
        }
    }
}
