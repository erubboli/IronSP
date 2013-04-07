using System;
using FluentAssertions;
using IronSharePoint.Administration;
using IronSharePoint.Hives;
using Moq;
using NUnit.Framework;
using TypeMock.ArrangeActAssert;

namespace IronSharePoint.Framework.Test.Administration
{
    [TestFixture]
    public class RuntimeSetup_Fixture
    {
        private RuntimeSetup Sut;
        private IronRegistry IronRegistry;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            IronRegistry = new IronRegistry();
            Isolate.WhenCalled(() => IronRegistry.Local).WillReturn(IronRegistry);
        }

        [SetUp]
        public void SetUp()
        {
            Sut = new RuntimeSetup();
            IronRegistry.Hives.Clear();
        }


        [Test, ExpectedException(typeof (ArgumentException))]
        public void AddGemPath_WithInvalidPath_ThrowsArgumentException()
        {
            Sut.AddGemPath("c:\\<invalid>");
        }

        [Test]
        public void AddGemPath_WithValidPath_AddsPath()
        {
            Sut.AddGemPath("c:\\");

            Sut.GemPaths.Should().BeEquivalentTo(new object[] {"c:\\"});
        }

        [Test]
        public void RemoveGemPath_RemovesPath()
        {
            Sut.AddGemPath("c:\\");

            Sut.RemoveGemPath("c:\\");

            Sut.GemPaths.Should().BeEmpty();

        }

        [Test, ExpectedException(typeof (ArgumentNullException))]
        public void AddHive_WithNull_ThrowsArgumentNullException()
        {
            Sut.AddHive(null);
        }

        [Test]
        public void AddHive_WithHive_AddsHive()
        {
            var setup = IronRegistry.Hives.Add();

            Sut.AddHive(setup);

            Sut.Hives.Should().BeEquivalentTo(new object[] {setup});
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void AddHive_WithEmptyGuid_ThrowsArgumentException()
        {
            Sut.AddHive(Guid.Empty);
        }

        [Test]
        public void AddHive_WithGuid_AddsHiveWithGuid()
        {
            var setup = IronRegistry.Hives.Add();

            Sut.AddHive(setup.Id);

            Sut.Hives.Should().BeEquivalentTo(new object[] {setup});
        }
    }
}