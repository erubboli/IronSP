using System;
using IronSharePoint.Administration;
using IronSharePoint.Hives;
using NUnit.Framework;
using FluentAssertions;

namespace IronSharePoint.Framework.Test.Administration
{
    [TestFixture]
    public class HiveSetup_Fixture
    {
        public HiveSetup Sut;

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void HiveType_WithTypeNotImplementingIHive_ThrowsArgumentException()
        {
            Sut = new HiveSetup
                {
                    HiveType = typeof (Object)
                };
        }

        [Test]
        public void HiveType_WithTypeImplementingIHive_SetsValue()
        {
            Sut = new HiveSetup
                {
                    HiveType = typeof (SPDocumentHive)
                };
        }

        [Test]
        public void Equals_WhenHiveTypeAndHiveArgumentAreEqual_ReturnsTrue()
        {
            var hiveSetup = new HiveSetup
                {
                    HiveType = typeof (DirectoryHive),
                    HiveArguments = new object[] {1}
                };
            var otherHiveSetup = new HiveSetup
                {
                    HiveType = typeof (DirectoryHive),
                    HiveArguments = new object[] {1}
                };

            hiveSetup.Equals(otherHiveSetup).Should().BeTrue();
        }
    }
}