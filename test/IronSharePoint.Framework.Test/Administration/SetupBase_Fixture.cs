using System;
using FluentAssertions;
using IronSharePoint.Administration;
using NUnit.Framework;

namespace IronSharePoint.Framework.Test.Administration
{
    [TestFixture]
    public class SetupBase_Fixture
    {
        SetupBase Sut;

        [Test]
        public void Equals_WhenIdIsEqual_ReturnsTrue()
        {
            Sut = new SetupBase() {Id = Guid.NewGuid()};

            Sut.Should().Be(new SetupBase {Id = Sut.Id});
        } 
    }
}