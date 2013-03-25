using System;
using System.Linq;
using FluentAssertions;
using IronSharePoint.Administration;
using IronSharePoint.Hives;
using NUnit.Framework;
using TypeMock.ArrangeActAssert;

// ReSharper disable CheckNamespace
namespace IronSharePoint.Framework.Test.IronScriptHost_Fixture
// ReSharper restore CheckNamespace
{
    public class IronScriptHost_FixtureBase
    {
        [SetUp]
        public void BaseSetUp()
        {
            HiveRegistry = HiveRegistry.Local;
            SiteId = Guid.NewGuid();
        }

        public IronScriptHost Sut;
        public HiveRegistry HiveRegistry;
        public Guid SiteId;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Isolate.Fake.StaticMethods<HiveRegistry>(Members.ReturnRecursiveFakes);
        }
    }

    [TestFixture]
    public class GivenNoRegisteredHives : IronScriptHost_FixtureBase
    {
        [SetUp]
        public void SetUp()
        {
            HiveSetupCollection hiveSetups;
            Isolate.WhenCalled(() => HiveRegistry.TryResolve(SiteId, out hiveSetups)).WillReturn(false);
        }

        [Test]
        public void Hive_ContainsOnlySystemHive()
        {
            Sut = new IronScriptHost(SiteId);

            Sut.Hive.Should().Contain(hive => hive is SystemHive);
            Sut.Hive.Count().Should().Be(1);
        }
    }

    [TestFixture]
    public class GivenOneRegisteredHive : IronScriptHost_FixtureBase
    {
        [SetUp]
        public void SetUp()
        {
            var hiveSetups = new HiveSetupCollection
                    {
                        new HiveSetup
                            {
                                HiveType = typeof (DirectoryHive),
                                HiveArguments = new[] {"c:\\"},
                                DisplayName = "Test Hive"
                            }
                    };
            Isolate.WhenCalled(() => HiveRegistry.TryResolve(SiteId, out hiveSetups)).WillReturn(true);
        }

        [Test, Explicit] // TODO test fails when running 'All Tests' but works when run in isolation. Don't know why.
        public void Hive_CreatesHiveFromSetup()
        {
            Sut = new IronScriptHost(SiteId);

            Sut.Hive.Should().Contain(hive => hive is DirectoryHive
                                              && hive.Name == "Test Hive"
                                              && (hive as DirectoryHive).Root == "c:\\");
        }

        [Test]
        public void Hive_SystemHiveIsLast()
        {
            Sut = new IronScriptHost(SiteId);

            Sut.Hive.Last().Should().BeOfType<SystemHive>();
        }
    }
}