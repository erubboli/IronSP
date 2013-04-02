using System;
using System.Linq;
using FluentAssertions;
using IronSharePoint.Administration;
using NUnit.Framework;

namespace IronSharePoint.Framework.Test.Administration
{
    [TestFixture]
    public class SetupCollection_Fixture
    {
        SetupCollection<SetupBase> Sut;

        [SetUp]
        public void SetUp()
        {
            Sut = new SetupCollection<SetupBase>();
        }

        [Test]
        public void Add_CreatesSetupWithFreshId()
        {
            var setup = Sut.Add();

            setup.Id.Should().NotBe(Guid.Empty);
        }

        [Test]
        public void Add_AddsSetupToCellction()
        {
            Sut.Add();

            Sut.Count().Should().Be(1);
        }

        [Test]
        public void Remove_WithSetup_WhenSetupExists_RemovesIt()
        {
            var setup = Sut.Add();

            var removed = Sut.Remove(setup);

            removed.Should().Be(setup);
            Sut.Count().Should().Be(0);
        }

        [Test]
        public void Remove_WithSetup_WhenNotExists_ReturnsNull()
        {
            Sut.Add();

            var removed = Sut.Remove(new SetupBase());

            removed.Should().BeNull();
            Sut.Count().Should().Be(1);
        }

        [Test]
        public void Remove_WithGuid_WhenSetupExists_RemovesIt()
        {
            var setup = Sut.Add();

            var removed = Sut.Remove(setup.Id);

            removed.Should().Be(setup);
            Sut.Count().Should().Be(0);
        }

        [Test]
        public void Remove_WithGuid_WhenNotExists_ReturnsNull()
        {
            Sut.Add();

            var removed = Sut.Remove(Guid.NewGuid());

            removed.Should().BeNull();
            Sut.Count().Should().Be(1);
        }

        [Test]
        public void GetEnumerator_EnumeratesAllSetups()
        {
            var setups = new object[]
                {
                    Sut.Add(),
                    Sut.Add()
                };

            Sut.Should().BeEquivalentTo(setups);
        }
    }
}