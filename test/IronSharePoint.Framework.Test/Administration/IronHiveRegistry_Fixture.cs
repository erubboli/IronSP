using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using IronSharePoint.Framework.Administration;
using IronSharePoint.Framework.Hives;
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
        public HiveDescription Hive;
        public HiveDescription OtherHive;

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
            Hive = new HiveDescription()
                {
                    Description = "Test Hive",
                    HiveType = typeof(SPDocumentHive),
                    Id = new Guid("23D9567E-AA85-45E8-A21E-522B18B4CCF2")
                };
            OtherHive = new HiveDescription()
            {
                Description = "Other Test Hive",
                HiveType = typeof(SPDocumentHive),
                Id = new Guid("DB479166-77D4-484B-B3A4-D839E5824008")
            };
        }

        [Test]
        public void AddTrustedHive_AddsTheHive()
        {
            Sut.AddTrustedHive(Hive);

            Sut.TrustedHives.Should().Contain(Hive);
        }

        [Test]
        public void AddTrustedHive_DoesntAddDuplicates()
        {
            Sut.AddTrustedHive(Hive);
            Sut.AddTrustedHive(Hive);

            Sut.TrustedHives.Should().HaveCount(1);
        }

        [Test]
        public void RemoveTrustedHive_WhenExists_RemovesTheHive()
        {
            Sut.AddTrustedHive(Hive);
            Sut.RemoveTrustedHive(Hive);

            Sut.TrustedHives.Should().BeEmpty();
        }

        [Test]
        public void RemoveTrustedHive_WhenNotExists_DoesNothing()
        {
            Sut.AddTrustedHive(Hive);
            Sut.RemoveTrustedHive(Guid.NewGuid());

            Sut.TrustedHives.Should().BeEquivalentTo(new[] {Hive});
        }

        [Test]
        [ExpectedException(typeof(SecurityException))]
        public void EnsureTrustedHive_WithUntrustedHive_ThrowsSecurityExpcetion()
        {
            Sut.EnsureTrustedHive(Guid.NewGuid());
        }

        [Test]
        public void EnsureTrustedHive_WithTrustedHive_DoesNothing()
        {
            Sut.AddTrustedHive(Hive);
            Sut.EnsureTrustedHive(Hive);

            Assert.Pass();
        }

        [Test]
        [ExpectedException(typeof(SecurityException))]
        public void AddHiveMapping_WithUntrustedHive_ThrowsSecurityException()
        {
            Sut.AddHiveMapping(Site, Hive);
        }

        [Test]
        public void AddHiveMapping_WithNewSite_CreatesMapping()
        {
            Sut.AddTrustedHive(Hive);

            Sut.AddHiveMapping(Site, Hive);

            Sut.GetMappedHivesForSite(Site).Should().BeEquivalentTo(new[] {Hive});
        }

        [Test]
        public void AddHiveMapping_WithExistingSite_AppendsHiveToMapping()
        {
            Sut.AddTrustedHive(Hive);
            Sut.AddTrustedHive(OtherHive);
            Sut.AddHiveMapping(Site, Hive);
            Sut.AddHiveMapping(Site, OtherHive);

            Sut.GetMappedHivesForSite(Site).Should().ContainInOrder(new[] {Hive, OtherHive});
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
            Sut.AddTrustedHive(Hive);
            Sut.AddHiveMapping(Site, Hive);

            Sut.GetMappedHivesForSite(Site).Should().ContainInOrder(new[] { Hive });
        }

        /// <summary>
        /// TODO seperate fixture for MappedHives ?
        /// </summary>
        [Test]
        [ExpectedException(typeof(SecurityException))]
        public void AddingHiveToMapping_WhenUntrusted_ThrowsSecurityException()
        {
            Sut.AddTrustedHive(Hive);
            Sut.AddHiveMapping(Site, Hive);
            var mappings = Sut.GetMappedHivesForSite(Site);
            mappings.Add(OtherHive);
        }
    }
}
