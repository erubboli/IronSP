using System;
using System.Linq;
using FluentAssertions;
using IronSharePoint.Administration;
using IronSharePoint.Hives;
using NUnit.Framework;
using TypeMock.ArrangeActAssert;

namespace IronSharePoint.Framework.Test
{
    public class IronScriptHost_Fixture
    {
        [SetUp]
        public void BaseSetUp()
        {
            SiteId = Guid.NewGuid();
        }

        public IronScriptHost Sut;
        public IronRegistry IronRegistry;
        public Guid SiteId;

        //[Test]
        //public void InstantiatesHiveFromSetup()
        //{
        //    var hiveSetup = new HiveSetup
        //        {
        //            Description = "Description",
        //            DisplayName = "DisplayName",
        //            HiveArguments = new[] {"c:\\"},
        //            HiveType = typeof (DirectoryHive),
        //            Priority = 1
        //        };
        //    Sut = new IronScriptHost(new[] {hiveSetup});

        //    Sut.Hive.First().
        //}
    }
}