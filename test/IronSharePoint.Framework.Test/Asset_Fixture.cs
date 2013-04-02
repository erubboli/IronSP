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

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void Name_RemovesAllButFirstExtension()
        {
            var listItem = Isolate.Fake.Instance<SPListItem>();
            Isolate.WhenCalled(() => listItem.Url).WillReturn("Style Library/Stylesheets/foo/bar.css.scss");

            Sut = new Asset(listItem, Isolate.Fake.Instance<AssetRuntime>());

            Sut.Name.Should().Be("bar.css");
        }

        [Test]
        public void SourceName_FileNameOfItem()
        {
            var listItem = Isolate.Fake.Instance<SPListItem>();
            Isolate.WhenCalled(() => listItem.Url).WillReturn("Style Library/foo/bar.css.scss");

            Sut = new Asset(listItem, Isolate.Fake.Instance<AssetRuntime>());

            Sut.SourceName.Should().Be("bar.css.scss");
        }

        [Test]
        public void FolderName_FolderNameOfItem()
        {
            var listItem = Isolate.Fake.Instance<SPListItem>();
            Isolate.WhenCalled(() => listItem.Url).WillReturn("Style Library/foo/bar.css.scss");

            Sut = new Asset(listItem, Isolate.Fake.Instance<AssetRuntime>());

            Sut.FolderName.Should().Be("Style Library/foo");
        }
    }
}