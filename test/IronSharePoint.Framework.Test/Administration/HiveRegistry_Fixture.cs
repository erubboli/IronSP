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
using Microsoft.SharePoint.Administration;
using NUnit.Framework;
using TypeMock.ArrangeActAssert;

namespace IronSharePoint.Framework.Test.Administration
{
    [TestFixture]
    public class HiveRegistry_Fixture
    {
        public HiveRegistry Sut;
        public SPSite Site;
        public SPWebApplication WebApplication;
        public SPFarm Farm;
        public HiveSetup HiveSetup;
        public HiveSetup OtherHiveSetup;
        public HiveSetup ThirdHiveSetup;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Isolate.Fake.StaticMethods<SPFarm>();
            Farm = SPFarm.Local;
            Site = Isolate.Fake.AllInstances<SPSite>();
            WebApplication = Isolate.Fake.Instance<SPWebApplication>();
            Isolate.WhenCalled(() => Site.ID).WillReturn(Guid.NewGuid());
            Isolate.WhenCalled(() => Site.WebApplication).WillReturn(WebApplication);
            Isolate.WhenCalled(() => WebApplication.Id).WillReturn(Guid.NewGuid());
            Isolate.WhenCalled(() => Farm.Id).WillReturn(Guid.NewGuid());

            Isolate.WhenCalled(() => SPSecurity.RunWithElevatedPrivileges(null)).DoInstead(ctx =>
                {
                    var action = ctx.Parameters[0] as SPSecurity.CodeToRunElevated;
                    action.Invoke();
                });
        }

        [SetUp]
        public void SetUp()
        {
            Sut = new HiveRegistry();
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
            ThirdHiveSetup = new HiveSetup()
            {
                DisplayName = "Third Test Hive",
                HiveType = typeof(SPDocumentHive),
                HiveArguments = new object[] { new Guid("EDD13B27-7944-4626-BA25-6905172C0F0B") }
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

            Sut.GetHiveSetups(Site).Should().BeEquivalentTo(new[] {HiveSetup});
        }

        [Test]
        public void AddHiveMapping_WithExistingSite_AppendsHiveToMapping()
        {
            Sut.AddTrustedHive(HiveSetup);
            Sut.AddTrustedHive(OtherHiveSetup);
            Sut.AddHiveMapping(Site, HiveSetup);
            Sut.AddHiveMapping(Site, OtherHiveSetup);

            Sut.GetHiveSetups(Site).Should().ContainInOrder(new[] {HiveSetup, OtherHiveSetup});
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetHiveSetups_WhenNoSetupsRegisteredForSite_ThrowsArgumentException()
        {
            Sut.GetHiveSetups(Site);
        }

        [Test]
        public void GetHiveSetups_WhenSetupsRegisteredForSite_ReturnsMappedHives()
        {
            Sut.AddTrustedHive(HiveSetup);
            Sut.AddHiveMapping(Site, HiveSetup);

            Sut.GetHiveSetups(Site).Should().ContainInOrder(new[] { HiveSetup });
        }


        [Test]
        public void GetHiveSetups_WhenSetupsRegisteredForHive_ReturnsMappedHives()
        {
            Sut.AddTrustedHive(HiveSetup);
            Sut.AddHiveMapping(Farm, HiveSetup);

            Sut.GetHiveSetups(Site).Should().ContainInOrder(new[] { HiveSetup });
        }

        [Test]
        public void GetHiveSetups_WhenSetupsRegisteredForWebApplication_ReturnsMappedHives()
        {
            Sut.AddTrustedHive(HiveSetup);
            Sut.AddHiveMapping(WebApplication, HiveSetup);

            Sut.GetHiveSetups(Site).Should().ContainInOrder(new[] { HiveSetup });
        }

        [Test]
        public void GetHiveSetups_SetupsAreOrdered()
        {
            Sut.AddTrustedHive(HiveSetup);
            Sut.AddTrustedHive(OtherHiveSetup);
            Sut.AddTrustedHive(ThirdHiveSetup);

            Sut.AddHiveMapping(Farm, ThirdHiveSetup);
            Sut.AddHiveMapping(WebApplication, OtherHiveSetup);
            Sut.AddHiveMapping(Site, HiveSetup);

            Sut.GetHiveSetups(Site).Should().ContainInOrder(new[] { HiveSetup, OtherHiveSetup, ThirdHiveSetup });
        }


        [Test]
        public void TryGetHiveSetups_WhenNoSetupsRegisteredForSite_ReturnsFalse()
        {
            HiveSetupCollection hiveSetups;

            Sut.TryGetHiveSetups(Site, out hiveSetups).Should().BeFalse();
        }

        [Test]
        public void TryGetHiveSetups_WhenSetupsRegisteredForSite_ReturnsTrue()
        {
            Sut.AddTrustedHive(HiveSetup);
            Sut.AddHiveMapping(Site, HiveSetup);
            HiveSetupCollection hiveSetups;

            Sut.TryGetHiveSetups(Site, out hiveSetups).Should().BeTrue();
        }

        [Test]
        public void TryGetHiveSetups_WhenSetupsRegisteredForSite_StoresSetupsInOutVariable()
        {
            Sut.AddTrustedHive(HiveSetup);
            Sut.AddHiveMapping(Site, HiveSetup);
            HiveSetupCollection hiveSetups;

            Sut.TryGetHiveSetups(Site, out hiveSetups);
            hiveSetups.Should().BeEquivalentTo(new[]{HiveSetup});
        }
    }
}
