using System;
using IronSharePoint.Administration;
using IronSharePoint.Hives;
using Moq;
using NUnit.Framework;
using FluentAssertions;

namespace IronSharePoint.Framework.Test.Administration
{
    [TestFixture]
    public class HiveSetupCollection_Fixture
    {
        HiveSetupCollection Sut;

        public HiveSetup HiveSetup;
        public HiveSetup OtherHiveSetup;

        [SetUp]
        public void SetUp()
        {
            Sut = new HiveSetupCollection();

            HiveSetup = new HiveSetup()
            {
                DisplayName = "Test Hive",
                HiveType = typeof(SPDocumentHive),
                HiveArguments = new object[] { new Guid("23D9567E-AA85-45E8-A21E-522B18B4CCF2") }
            };
            OtherHiveSetup = new HiveSetup()
            {
                DisplayName = "Other Test Hive",
                HiveType = typeof(SPDocumentHive),
                HiveArguments = new object[] { new Guid("DB479166-77D4-484B-B3A4-D839E5824008") }
            };
        }

        [Test]
        public void AddItem_EnsuresTrustedHive()
        {
            var registryMock = new Mock<HiveRegistry>();
            registryMock.Setup(x => x.EnsureTrustedHive(HiveSetup)).Verifiable();
            Sut.Registry = registryMock.Object;

            Sut.Add(HiveSetup);

            registryMock.VerifyAll();
        }

        [Test]
        public void AddItem_UpdatesHiveSetupPriority()
        {
            HiveSetup.Priority = -1;

            Sut.Add(HiveSetup);

            HiveSetup.Priority.Should().Be(0);
        }

        [Test]
        public void RemoveItem_UpdatesRemainingHiveSetupPriorities()
        {
            Sut.Add(HiveSetup);
            Sut.Add(OtherHiveSetup);
            Sut.Remove(HiveSetup);

            OtherHiveSetup.Priority.Should().Be(0);
        }

        [Test]
        public void Swap_SwapsItems()
        {
            Sut.Add(HiveSetup);
            Sut.Add(OtherHiveSetup);

            Sut.Swap(0, 1);

            Sut.Should().ContainInOrder(new[] {OtherHiveSetup, HiveSetup});
        }

        [Test]
        public void MoveUp_MovesItemAtIndexOneUp()
        {
            Sut.Add(HiveSetup);
            Sut.Add(OtherHiveSetup);

            Sut.MoveUp(1);

            Sut.Should().ContainInOrder(new[] {OtherHiveSetup, HiveSetup});
        }

        [Test]
        public void MoveUp_WithFirstItem_DoesNothing()
        {
            Sut.Add(HiveSetup);
            Sut.Add(OtherHiveSetup);

            Sut.MoveUp(0);

            Sut.Should().ContainInOrder(new[] {HiveSetup, OtherHiveSetup});
        }

        [Test]
        public void MoveDown_MovesItemAtIndexOneDown()
        {
            Sut.Add(HiveSetup);
            Sut.Add(OtherHiveSetup);

            Sut.MoveDown(0);

            Sut.Should().ContainInOrder(new[] {OtherHiveSetup, HiveSetup});
        }

        [Test]
        public void MoveDown_WithLastItem_DoesNothing()
        {
            Sut.Add(HiveSetup);
            Sut.Add(OtherHiveSetup);

            Sut.MoveDown(1);

            Sut.Should().ContainInOrder(new[] { HiveSetup, OtherHiveSetup });
        }
    }

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
    }
}