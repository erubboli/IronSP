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

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Isolate.Fake.StaticMethods<HiveRegistry>(Members.ReturnRecursiveFakes);
        }


        [Test]
        public void Hive_WhenOneHiveSetup_CreatesHiveFromSetup()
        {
            var siteId = Guid.NewGuid();
            var hiveSetups = new HiveSetupCollection
                {
                    new HiveSetup
                        {
                            HiveType = typeof (DirectoryHive),
                            HiveArguments = new[] {"c:\\"}
                        }
                };
            var registry = HiveRegistry.Local;
            Isolate.WhenCalled(() => registry.TryResolve(siteId, out hiveSetups)).WillReturn(true);

            Sut = new IronScriptHost(siteId);

            Sut.Hive.Should().BeOfType<HiveComposite>();
            (Sut.Hive as HiveComposite).Should().Contain(hive =>
                                                           hive is DirectoryHive &&
                                                           (hive as DirectoryHive).Root == "c:\\");
        }
    }
}
