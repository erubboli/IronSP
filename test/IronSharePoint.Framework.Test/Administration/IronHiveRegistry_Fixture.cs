using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using IronSharePoint.Administration;
using IronSharePoint.Hives;
using Microsoft.SharePoint;
using NUnit.Framework;
using TypeMock.ArrangeActAssert;

namespace IronSharePoint.Framework.Test.Administration
{
    [TestFixture]
    public class IronHiveRegistry_Fixture
    {
        public IronHiveRegistry Sut;
        public SPSite Site;
        public HiveSetup HiveSetup;
        public HiveSetup OtherHiveSetup;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Isolate.WhenCalled(() => SPSecurity.RunWithElevatedPrivileges(null)).DoInstead(ctx =>
                {
                    var action = ctx.Parameters[0] as SPSecurity.CodeToRunElevated;
                    action.Invoke();
                });
            Site = Isolate.Fake.AllInstances<SPSite>();
            Isolate.WhenCalled(() => Site.ID).WillReturn(Guid.NewGuid());
        }

        [SetUp]
        public void SetUp()
        {
            Sut = new IronHiveRegistry();
            HiveSetup = new HiveSetup()
                {
                    DisplayName = "Test Hive",
                    HiveType = typeof(SPDocumentHive),
                    HiveArguments = new object[] {new Guid("23D9567E-AA85-45E8-A21E-522B18B4CCF2")}
                };
            OtherHiveSetup = new HiveSetup()
                {
                    DisplayName = "Other Test Hive",
                    HiveType = typeof (SPDocumentHive),
                    HiveArguments = new object[] {new Guid("DB479166-77D4-484B-B3A4-D839E5824008")}
                };
        }

        [Test]
        public void AddTrustedHive_AddsTheHive()
        {
            Sut.AddTrustedHive(HiveSetup);

            Sut.TrustedHives.Should().Contain(HiveSetup);
        }

        [Test]
        public void AddTrustedHive_DoesntAddDuplicates()
        {
            Sut.AddTrustedHive(HiveSetup);
            Sut.AddTrustedHive(HiveSetup);

            Sut.TrustedHives.Should().HaveCount(1);
        }

        [Test]
        public void RemoveTrustedHive_WhenExists_RemovesTheHive()
        {
            Sut.AddTrustedHive(HiveSetup);
            Sut.RemoveTrustedHive(HiveSetup);

            Sut.TrustedHives.Should().BeEmpty();
        }

        [Test]
        public void RemoveTrustedHive_WhenNotExists_DoesNothing()
        {
            Sut.AddTrustedHive(HiveSetup);
            Sut.RemoveTrustedHive(OtherHiveSetup);

            Sut.TrustedHives.Should().BeEquivalentTo(new[] {HiveSetup});
        }

        [Test]
        [ExpectedException(typeof(SecurityException))]
        public void EnsureTrustedHive_WithUntrustedHive_ThrowsSecurityExpcetion()
        {
            Sut.EnsureTrustedHive(OtherHiveSetup);
        }

        [Test]
        public void EnsureTrustedHive_WithTrustedHive_DoesNothing()
        {
            Sut.AddTrustedHive(HiveSetup);
            Sut.EnsureTrustedHive(HiveSetup);

            Assert.Pass();
        }

        [Test]
        [ExpectedException(typeof(SecurityException))]
        public void AddHiveMapping_WithUntrustedHive_ThrowsSecurityException()
        {
            Sut.AddHiveMapping(Site, HiveSetup);
        }

        [Test]
        public void AddHiveMapping_WithNewSite_CreatesMapping()
        {
            Sut.AddTrustedHive(HiveSetup);

            Sut.AddHiveMapping(Site, HiveSetup);

            Sut.GetMappedHivesForSite(Site).Should().BeEquivalentTo(new[] {HiveSetup});
        }

        [Test]
        public void AddHiveMapping_WithExistingSite_AppendsHiveToMapping()
        {
            Sut.AddTrustedHive(HiveSetup);
            Sut.AddTrustedHive(OtherHiveSetup);
            Sut.AddHiveMapping(Site, HiveSetup);
            Sut.AddHiveMapping(Site, OtherHiveSetup);

            Sut.GetMappedHivesForSite(Site).Should().ContainInOrder(new[] {HiveSetup, OtherHiveSetup});
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetMappedHivesForSite_WithUnknownSite_ThrowsArgumentException()
        {
            Sut.GetMappedHivesForSite(Site);
        }

        [Test]
        public void GetMappedHivesForSite_WithKnownSite_ReturnsMappedHives()
        {
            Sut.AddTrustedHive(HiveSetup);
            Sut.AddHiveMapping(Site, HiveSetup);

            Sut.GetMappedHivesForSite(Site).Should().ContainInOrder(new[] { HiveSetup });
        }
    }
}
