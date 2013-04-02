using System.Collections.Generic;
using IronSharePoint.AssetPipeline;
using Microsoft.SharePoint;
using NUnit.Framework;
using TypeMock.ArrangeActAssert;
using FluentAssertions;

namespace IronSharePoint.Framework.Test
{
    [TestFixture]
    public class Asset_Fixture
    {
        public Asset Sut;
        public AssetConfiguration Configuration;

        [SetUp]
        public void SetUp()
        {
            Configuration = new AssetConfiguration();
        }

        [Test]
        public void Name_WhenPathsAreConfigured_ExtractsNameFromItemUrl()
        {
            var listItem = Isolate.Fake.Instance<SPListItem>();
            Isolate.WhenCalled(() => listItem.Url).WillReturn("Style Library/Stylesheets/foo/bar.css.scss");
            Configuration.Paths.Add("Stylesheets");
            Configuration.Paths.Add("Javascripts");

            Sut = new Asset(listItem, Isolate.Fake.Instance<AssetRuntime>(), Configuration);

            Sut.Name.Should().Be("foo/bar.css");
        }

        [Test]
        public void Name_WhenNoPathsAreConfigured_ExtractsNameFromItemUrl()
        {
            var listItem = Isolate.Fake.Instance<SPListItem>();
            Isolate.WhenCalled(() => listItem.Url).WillReturn("Style Library/foo/bar.css.scss");

            Sut = new Asset(listItem, Isolate.Fake.Instance<AssetRuntime>(), Configuration);

            Sut.Name.Should().Be("foo/bar.css");
        }

        [Test]
        public void Name_IgnoresCaseInItemUrl()
        {
            var listItem = Isolate.Fake.Instance<SPListItem>();
            Isolate.WhenCalled(() => listItem.Url).WillReturn("style lIBRary/stylesheets/foo/bar.css.scss");
            Configuration.Paths.Add("sTyLesheets");

            Sut = new Asset(listItem, Isolate.Fake.Instance<AssetRuntime>(), Configuration);

            Sut.Name.Should().Be("foo/bar.css");
        }

        [Test]
        public void FolderName_WhenPathsAreConfigured_ExtractsNameFromItemUrl()
        {
            var listItem = Isolate.Fake.Instance<SPListItem>();
            Isolate.WhenCalled(() => listItem.Url).WillReturn("Style Library/Stylesheets/foo/bar.css.scss");
            Configuration.Paths.Add("Stylesheets");
            Configuration.Paths.Add("Javascripts");

            Sut = new Asset(listItem, Isolate.Fake.Instance<AssetRuntime>(), Configuration);

            Sut.FolderName.Should().Be("Style Library/Stylesheets");
        }

        [Test]
        public void FolderName_WhenNoPathsAreConfigured_ExtractsNameFromItemUrl()
        {
            var listItem = Isolate.Fake.Instance<SPListItem>();
            Isolate.WhenCalled(() => listItem.Url).WillReturn("Style Library/foo/bar.css.scss");

            Sut = new Asset(listItem, Isolate.Fake.Instance<AssetRuntime>(), Configuration);

            Sut.FolderName.Should().Be("Style Library");
        }
    }
}