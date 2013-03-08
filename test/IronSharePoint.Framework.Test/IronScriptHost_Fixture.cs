using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronSharePoint.Administration;
using IronSharePoint.Hives;
using NUnit.Framework;
using TypeMock.ArrangeActAssert;
using FluentAssertions;

namespace IronSharePoint.Framework.Test
{
    [TestFixture]
    public class IronScriptHost_Fixture
    {
        IronScriptHost Sut;

        [Test]
        public void Hive_WhenNoHiveSetupsInRegistry_ContainsOnlyIronSPRootHive()
        {
            var siteId = Guid.NewGuid();
            var hiveSetups = new HiveSetupCollection();
            Isolate.Fake.StaticMethods<HiveRegistry>(Members.ReturnRecursiveFakes);
            var registry = HiveRegistry.Local;
            Isolate.WhenCalled(() => registry.TryGetHiveSetups(siteId, out hiveSetups)).WillReturn(true);

            Sut = new IronScriptHost(siteId);

            Sut.Hive.Should().BeOfType<OrderedHiveList>();
            (Sut.Hive as OrderedHiveList).Should()
                                         .Contain(
                                             hive =>
                                             hive is PhysicalHive &&
                                             (hive as PhysicalHive).Root == IronConstant.IronSPRootDirectory);
        }

        [Test]
        public void Hive_WhenOneHiveSetup_CreatesHiveFromSetup()
        {
            var siteId = Guid.NewGuid();
            var hiveSetups = new HiveSetupCollection()
                {
                    new HiveSetup()
                        {
                            HiveType = typeof(PhysicalHive),
                            HiveArguments = new[]{"c:\\"}
                        }
                };
            Isolate.Fake.StaticMethods<HiveRegistry>(Members.ReturnRecursiveFakes);
            var registry = HiveRegistry.Local;
            Isolate.WhenCalled(() => registry.TryGetHiveSetups(siteId, out hiveSetups)).WillReturn(true);

            Sut = new IronScriptHost(siteId);

            Sut.Hive.Should().BeOfType<OrderedHiveList>();
            (Sut.Hive as OrderedHiveList).Should()
                                         .Contain(
                                             hive =>
                                             hive is PhysicalHive &&
                                             (hive as PhysicalHive).Root == "c:\\");
        }

        [Test]
        public void Hive_WhenRegistryHasIronSPRootSetup_DoesntCreateItTwice()
        {
            var siteId = Guid.NewGuid();
            var hiveSetups = new HiveSetupCollection()
                {
                    HiveSetup.IronSPRoot
                };
            Isolate.Fake.StaticMethods<HiveRegistry>(Members.ReturnRecursiveFakes);
            var registry = HiveRegistry.Local;
            Isolate.WhenCalled(() => registry.TryGetHiveSetups(siteId, out hiveSetups)).WillReturn(true);

            Sut = new IronScriptHost(siteId);

            Sut.Hive.Should().BeOfType<OrderedHiveList>();
            (Sut.Hive as OrderedHiveList).Should()
                                         .ContainSingle(
                                             hive =>
                                             hive is PhysicalHive &&
                                             (hive as PhysicalHive).Root == IronConstant.IronSPRootDirectory);
        } 
    }
}
