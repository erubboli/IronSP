using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.SharePoint;
using NUnit.Framework;
using TypeMock.ArrangeActAssert;
using IronSharePoint.Framework.Hives;

namespace IronSharePoint.Framework.Test.Hive
{
    [TestFixture]
    class SPDocumentHive_Fixture
    {
        public SPDocumentHive Sut;

        [SetUp]
        public void SetUp()
        {
            FakeNextSPSite();
            Sut = new SPDocumentHive(Guid.Empty);
        }

        [Test]
        public void Ctor_CreatesSPSiteWithId()
        {
            // TODO dunno how to fake
            //var guid = Guid.NewGuid();
            //Isolate.WhenCalled(() => new SPSite(guid)).WithExactArguments().DoInstead((ctx) =>
            //    {
            //        return _site;
            //    });
            //Sut = new SPDocumentHive(guid);
        }

        [Test]
        public void Ctor_CachesFiles()
        {
            Sut.CachedFiles.Should().BeEquivalentTo(new[]
                {
                    "foo.txt",
                    "bar/baz.txt",
                    "bar/baz/qux.txt"
                });
        }

        [Test]
        public void Ctor_CachesDirs()
        {
            Sut.CachedDirs.Should().BeEquivalentTo(new[]
                {
                    "bar",
                    "bar/baz"
                });
        }

        [Test]
        public void FileExists_WhenFileExists_ReturnsTrue()
        {
            Sut.FileExists("foo.txt").Should().BeTrue();
        }

        [Test]
        public void FileExists_WhenFileIsMissing_ReturnsFalse()
        {
            Sut.FileExists("i_do_not.exist").Should().BeFalse();
        }

        [Test]
        public void FileExists_WithExistingAbsoluteFile_ReturnsTrue()
        {
            Sut.FileExists("http://foo.com/sites/IronSP/_catalogs/IronHive/foo.txt").Should().BeTrue();
        }

        [Test]
        public void DirectoryExists_WithExistingDirectory_ReturnsTrue()
        {
            Sut.DirectoryExists("bar").Should().BeTrue();
        }

        [Test]
        public void DirectoryExists_WithMissingDirectory_ReturnsFalse()
        {
            Sut.DirectoryExists("i_do_not_exist").Should().BeFalse();
        }

        [Test]
        public void DirectoryExists_WithExistingAbsoluteDirectory_ReturnsTrue()
        {
            Sut.DirectoryExists("http://foo.com/sites/IronSP/_catalogs/IronHive/bar").Should().BeTrue();
        }

        [Test]
        public void GetFullPath_WithPartialPath_ReturnsFullPath()
        {
            Sut.GetFullPath("bar/baz.txt").Should().Be("http://foo.com/sites/IronSP/_catalogs/IronHive/bar/baz.txt");
        }

        [Test]
        public void GetFullPath_WithAbsolutePath_ReturnsFullPath()
        {
            Sut.GetFullPath("http://foo.com/sites/IronSP/_catalogs/IronHive/bar/baz.txt").Should().Be("http://foo.com/sites/IronSP/_catalogs/IronHive/bar/baz.txt");
        }

        [Test]
        public void IsAbsolutePath_WithPartialPath_ReturnsFalse()
        {
            Sut.IsAbsolutePath("bar/baz.txt").Should().BeFalse();
        }

        [Test]
        public void IsAbsolutePath_WithAbsolutePath_ReturnsTrue()
        {
            Sut.IsAbsolutePath("http://foo.com/sites/IronSP/_catalogs/IronHive/bar/baz.txt").Should().BeTrue();
        }

        [Test]
        public void IsAbsolutePath_WithAbsolutePathOnOtherDomain_ReturnsTrue()
        {
            Sut.IsAbsolutePath("http://bar.com/sites/IronSP/_catalogs/IronHive/bar/baz.txt").Should().BeTrue();
        }

        [Test]
        public void CombinePath_ReturnsCombinedPath()
        {
            Sut.CombinePath("http://foo.com", "sites").Should().Be("http://foo.com/sites");
        }

        [Test]
        public void CombinePath_TrimsRedundantSlashes()
        {
            Sut.CombinePath("http://foo.com/", "/sites").Should().Be("http://foo.com/sites");
        }

        [Test]
        public void OpenInputFileStream_WhenFileExists_ReturnsFilestream()
        {
            var web = FakeNextSPSite().RootWeb;
            var spFile = Isolate.Fake.Instance<SPFile>();
            var stream = Isolate.Fake.Instance<Stream>();
            Isolate.WhenCalled(() => web.GetFile("foo.txt")).WithExactArguments().WillReturn(spFile);
            Isolate.WhenCalled(() => spFile.Exists).WillReturn(true);
            Isolate.WhenCalled(() => spFile.OpenBinaryStream()).WillReturn(stream);

            Sut.OpenInputFileStream("foo.txt").Should().BeSameAs(stream);
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void OpenInputFileStream_WhenFileIsMissing_ThrowsFileNotFoundException()
        {
            var web = FakeNextSPSite().RootWeb;
            var spFile = Isolate.Fake.Instance<SPFile>();
            Isolate.WhenCalled(() => web.GetFile("foo.txt")).WithExactArguments().WillReturn(spFile);
            Isolate.WhenCalled(() => spFile.Exists).WillReturn(false);

            Sut.OpenInputFileStream("foo.txt");
        }

        [Test]
        public void OpenOutputFileStream_WhenFileExists_ReturnsFilestream()
        {
            var web = FakeNextSPSite().RootWeb;
            var spFile = Isolate.Fake.Instance<SPFile>();
            var stream = Isolate.Fake.Instance<Stream>();
            Isolate.WhenCalled(() => web.GetFile("foo.txt")).WithExactArguments().WillReturn(spFile);
            Isolate.WhenCalled(() => spFile.Exists).WillReturn(true);
            Isolate.WhenCalled(() => spFile.OpenBinaryStream()).WillReturn(stream);

            Sut.OpenOutputFileStream("foo.txt").Should().BeSameAs(stream);
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void OpenOutputFileStream_WhenFileIsMissing_ThrowsFileNotFoundException()
        {
            var web = FakeNextSPSite().RootWeb;
            var spFile = Isolate.Fake.Instance<SPFile>();
            Isolate.WhenCalled(() => web.GetFile("foo.txt")).WithExactArguments().WillReturn(spFile);
            Isolate.WhenCalled(() => spFile.Exists).WillReturn(false);

            Sut.OpenOutputFileStream("foo.txt");
        }

        [Test]
        public void GetFiles_AbsolutePaths()
        {
            Sut.GetFiles("", "*", true).Should().Contain("http://foo.com/sites/IronSP/_catalogs/IronHive/foo.txt");
        }

        [Test]
        public void GetFiles_OnRoot_ContainsOnlyOneFile()
        {
            Sut.GetFiles("", "*.txt").Should().BeEquivalentTo(new object[]{"foo.txt"});
        }

        [Test]
        public void GetFiles_OnSubdir_ContainsOnlyOneFile()
        {
            Sut.GetFiles("bar", "*.txt").Should().BeEquivalentTo(new object[] { "bar/baz.txt" });
        }

        [Test]
        public void GetDirectories_AbsolutePaths()
        {
            Sut.GetDirectories(".", "*", true).Should().Contain("http://foo.com/sites/IronSP/_catalogs/IronHive/bar");
        }

        [Test]
        public void GetDirectories_OnRoot_ContainsDirectory()
        {
            Sut.GetDirectories(".", "*").Should().Contain("bar");
        }

        [Test]
        public void GetDirectories_OnRoot_DoesNotContainFile()
        {
            Sut.GetDirectories(".", "*").Should().NotContain("foo.txt");
        }

        [Test]
        public void GeDirectories_OnSubdir_ContainsDirectory()
        {
            Sut.GetDirectories("bar", "*").Should().BeEquivalentTo(new object[] { "bar/baz" });
        }


        private SPSite FakeNextSPSite()
        {
            var site = Isolate.Fake.NextInstance<SPSite>(Members.MustSpecifyReturnValues);
            var web = Isolate.Fake.Instance<SPWeb>(Members.MustSpecifyReturnValues);
            var folder = Isolate.Fake.Instance<SPFolder>(Members.MustSpecifyReturnValues);
            var lib = Isolate.Fake.Instance<SPDocumentLibrary>(Members.MustSpecifyReturnValues);
            var spListItems = new[]
                {
                    "/sites/IronSP/_catalogs/IronHive/foo.txt",
                    "/sites/IronSP/_catalogs/IronHive/bar/baz.txt",
                    "/sites/IronSP/_catalogs/IronHive/bar/baz/qux.txt"
                }.Select(x =>
                {
                    var spItem = Isolate.Fake.Instance<SPListItem>();
                    Isolate.WhenCalled(() => spItem["FileRef"]).WillReturn(x);
                    return spItem;
                });

            Isolate.WhenCalled(() => site.RootWeb).WillReturn(web);
            Isolate.WhenCalled(() => web.GetFolder(IronConstant.HiveLibraryPath)).WillReturn(folder);
            Isolate.WhenCalled(() => web.ServerRelativeUrl).WillReturn("/sites/IronSP");
            Isolate.WhenCalled(() => folder.DocumentLibrary).WillReturn(lib);
            Isolate.WhenCalled(() => lib.ParentWeb).WillReturn(web);
            Isolate.WhenCalled(() => lib.ParentWebUrl).WillReturn("http://foo.com/sites/IronSP");
            Isolate.WhenCalled(() => lib.GetItems((SPQuery) null)).WillReturnCollectionValuesOf(spListItems);

            return site;
        }
    }
}
