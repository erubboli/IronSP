using System;
using FluentAssertions;
using IronSharePoint.Administration;
using IronSharePoint.Hives;
using Moq;
using NUnit.Framework;

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
    }
}