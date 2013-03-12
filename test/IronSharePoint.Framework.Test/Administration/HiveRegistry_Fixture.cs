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
        public void Trust_AddsTheHive()
        {
            Sut.Trust(HiveSetup);

            Sut.TrustedHives.Should().Contain(HiveSetup);
        }

        [Test]
        public void Trust_DoesntAddDuplicates()
        {
            Sut.Trust(HiveSetup);
            Sut.Trust(HiveSetup);

            Sut.TrustedHives.Should().HaveCount(1);
        }

        [Test]
        public void Untrust_WhenExists_RemovesTheHive()
        {
            Sut.Trust(HiveSetup);
            Sut.Untrust(HiveSetup);

            Sut.TrustedHives.Should().BeEmpty();
        }

        [Test]
        public void Untrust_WhenNotExists_DoesNothing()
        {
            Sut.Trust(HiveSetup);
            Sut.Untrust(OtherHiveSetup);

            Sut.TrustedHives.Should().BeEquivalentTo(new[] {HiveSetup});
        }

        [Test]
        [ExpectedException(typeof(SecurityException))]
        public void IsTrusted_WithUntrustedHive_ThrowsSecurityExpcetion()
        {
            Sut.IsTrusted(OtherHiveSetup);
        }

        [Test]
        public void IsTrusted_WithTrustedHive_DoesNothing()
        {
            Sut.Trust(HiveSetup);
            Sut.IsTrusted(HiveSetup);

            Assert.Pass();
        }

        [Test]
        [ExpectedException(typeof(SecurityException))]
        public void Map_WithUntrustedHive_ThrowsSecurityException()
        {
            Sut.Map(HiveSetup, Site);
        }

        [Test]
        public void Map_WithNewSite_CreatesMapping()
        {
            Sut.Trust(HiveSetup);

            Sut.Map(HiveSetup, Site);

            Sut.Resolve(Site).Should().BeEquivalentTo(new[] {HiveSetup});
        }

        [Test]
        public void Map_WithExistingSite_AppendsHiveToMapping()
        {
            Sut.Trust(HiveSetup);
            Sut.Trust(OtherHiveSetup);
            Sut.Map(HiveSetup, Site);
            Sut.Map(OtherHiveSetup, Site);

            Sut.Resolve(Site).Should().ContainInOrder(new[] {HiveSetup, OtherHiveSetup});
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Resolve_WhenNoSetupsRegisteredForSite_ThrowsArgumentException()
        {
            Sut.Resolve(Site);
        }

        [Test]
        public void Resolve_WhenSetupsRegisteredForSite_ReturnsMappedHives()
        {
            Sut.Trust(HiveSetup);
            Sut.Map(HiveSetup, Site);

            Sut.Resolve(Site).Should().ContainInOrder(new[] { HiveSetup });
        }


        [Test]
        public void Resolve_WhenSetupsRegisteredForHive_ReturnsMappedHives()
        {
            Sut.Trust(HiveSetup);
            Sut.Map(HiveSetup, Farm);

            Sut.Resolve(Site).Should().ContainInOrder(new[] { HiveSetup });
        }

        [Test]
        public void Resolve_WhenSetupsRegisteredForWebApplication_ReturnsMappedHives()
        {
            Sut.Trust(HiveSetup);
            Sut.Map(HiveSetup, WebApplication);

            Sut.Resolve(Site).Should().ContainInOrder(new[] { HiveSetup });
        }

        [Test]
        public void Resolve_SetupsAreOrdered()
        {
            Sut.Trust(HiveSetup);
            Sut.Trust(OtherHiveSetup);
            Sut.Trust(ThirdHiveSetup);

            Sut.Map(ThirdHiveSetup, Farm);
            Sut.Map(OtherHiveSetup, WebApplication);
            Sut.Map(HiveSetup, Site);

            Sut.Resolve(Site).Should().ContainInOrder(new[] { HiveSetup, OtherHiveSetup, ThirdHiveSetup });
        }


        [Test]
        public void TryResolve_WhenNoSetupsRegisteredForSite_ReturnsFalse()
        {
            HiveSetupCollection hiveSetups;

            Sut.TryResolve(Site, out hiveSetups).Should().BeFalse();
        }

        [Test]
        public void TryResolve_WhenSetupsRegisteredForSite_ReturnsTrue()
        {
            Sut.Trust(HiveSetup);
            Sut.Map(HiveSetup, Site);
            HiveSetupCollection hiveSetups;

            Sut.TryResolve(Site, out hiveSetups).Should().BeTrue();
        }

        [Test]
        public void TryResolve_WhenSetupsRegisteredForSite_StoresSetupsInOutVariable()
        {
            Sut.Trust(HiveSetup);
            Sut.Map(HiveSetup, Site);
            HiveSetupCollection hiveSetups;

            Sut.TryResolve(Site, out hiveSetups);
            hiveSetups.Should().BeEquivalentTo(new[]{HiveSetup});
        }
    }
}
