using System;
using System.IO;
using FluentAssertions;
using IronSharePoint.Hives;
using NUnit.Framework;

namespace IronSharePoint.Framework.Test.Hive
{
    [TestFixture]
    class DirectoryHive_Fixture
    {
        public DirectoryHive Sut;

        private string _assetsRoot;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _assetsRoot = Path.Combine(Directory.GetCurrentDirectory(), "_assets");
        }

        [SetUp]
        public void SetUp()
        {
            if (File.Exists(Path.Combine(_assetsRoot, "i_do_not.exist")))
            {
                File.Delete(Path.Combine(_assetsRoot, "i_do_not.exist"));
            }
        }


        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Ctor_WhenDirectoryIsMissing_ThrowArgumentException()
        {
            Sut = new DirectoryHive(@"c:\i_do_not_exist");
        }
        
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_WithNull_ThrowsArgumentNullException()
        {
            Sut = new DirectoryHive(null);
        }
        
        [Test]
        public void Ctor_AssignsRoot()
        {
            Sut = new DirectoryHive(_assetsRoot);

            Sut.Root.Should().Be(_assetsRoot);
        }

        [Test]
        public void FileExists_WhenFileIsMissing_ReturnsFalse()
        {
            Sut = new DirectoryHive(_assetsRoot);

            Sut.FileExists("i_do_not.exist").Should().BeFalse();
        }

        [Test]
        public void FileExists_WhenFileExists_ReturnsTrue()
        {
            Sut = new DirectoryHive(_assetsRoot);

            Sut.FileExists("lorem.txt").Should().BeTrue();
        }

        [Test]
        public void FileExists_WithExistingAbsolutePath_ReturnsTrue()
        {
            Sut = new DirectoryHive(_assetsRoot);

            Sut.FileExists(Path.Combine(_assetsRoot, "lorem.txt")).Should().BeTrue();
        }

        [Test]
        public void DirectoryExists_WhenDirectoryIsMissing_ReturnsFalse()
        {
            Sut = new DirectoryHive(_assetsRoot);

            Sut.DirectoryExists("i_do_not_exist").Should().BeFalse();
        }

        [Test]
        public void DirectoryExists_WhenDirectoryIsMissing_ReturnsTrue()
        {
            Sut = new DirectoryHive(_assetsRoot);

            Sut.DirectoryExists("foo").Should().BeTrue();
        }

        [Test]
        public void GetFullPath_WithPartialPath_ReturnsFullPath()
        {
            Sut = new DirectoryHive(_assetsRoot);
            var partial = "lorem.txt";
            var expected = Path.Combine(Sut.Root, partial);

            Sut.GetFullPath(partial).Should().Be(expected);
        }

        [Test]
        public void GetFullPath_WithFullPath_ReturnsFullPath()
        {
            Sut = new DirectoryHive(_assetsRoot);
            var expected = @"c:\foo.txt";

            Sut.GetFullPath(expected).Should().Be(expected);
        }

        [Test]
        public void IsAbsolutePath_WithAbsolutePath_ReturnsTrue()
        {
            Sut.IsAbsolutePath(Path.Combine(_assetsRoot, "lorem.txt")).Should().BeTrue();
        }

        [Test]
        public void IsAbsolutePath_WithPartialPath_ReturnsFalse()
        {
            Sut.IsAbsolutePath("lorem.txt").Should().BeFalse();
        }

        [Test]
        public void IsAbsolutePath_WithAbsolutePathOnOtherRoot_ReturnsTrue()
        {
            Sut.IsAbsolutePath(@"c:\whatever\foo.txt").Should().BeTrue();
        }

        [Test]
        public void OpenInputFileStream_OpensCorrectFile()
        {
            Sut = new DirectoryHive(_assetsRoot);
            var expected = File.ReadAllBytes(Path.Combine(_assetsRoot, "lorem.txt")).Length;

            using (var stream = Sut.OpenInputFileStream("lorem.txt"))
            {
                stream.Length.Should().Be(expected);
            }
        }

        [Test]
        public void OpenInputFileStream_CanWriteToStream()
        {
            Sut = new DirectoryHive(_assetsRoot);

            using (var stream = Sut.OpenInputFileStream("lorem.txt"))
            {
                stream.CanRead.Should().BeTrue();
            }
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void OpenInputFileStream_WhenFileIsMissing_ThrowsFileNotFoundException()
        {
            Sut = new DirectoryHive(_assetsRoot);

            using (Sut.OpenInputFileStream("i_do_not.exist"))
            {
                // do nothing
            }
        }

        [Test]
        public void OpenOutputFileStream_OpensCorrectFile()
        {
            Sut = new DirectoryHive(_assetsRoot);
            var expected = File.ReadAllBytes(Path.Combine(_assetsRoot, "lorem.txt")).Length;

            using (var stream = Sut.OpenOutputFileStream("lorem.txt"))
            {
                stream.Length.Should().Be(expected);
            }
        }

        [Test]
        public void OpenOutputFileStream_CanReadFromStream()
        {
            Sut = new DirectoryHive(_assetsRoot);

            using (var stream = Sut.OpenOutputFileStream("lorem.txt"))
            {
                stream.CanWrite.Should().BeTrue();
            }
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void OpenOutputFileStream_WhenFileIsMissing_ThrowsFileNotFoundException()
        {
            Sut = new DirectoryHive(_assetsRoot);

            using (Sut.OpenOutputFileStream("i_do_not.exist"))
            {
               // do nothing
            }
        }

        [Test]
        public void GetFiles_AbsolutePaths()
        {
            Sut = new DirectoryHive(_assetsRoot);

            Sut.GetFiles(".", "*", true).Should().Contain(Path.Combine(_assetsRoot, "lorem.txt"));
        }

        [Test]
        public void GetFiles_OnRoot_ContainsFile()
        {
            Sut = new DirectoryHive(_assetsRoot);

            Sut.GetFiles(".", "*").Should().Contain("lorem.txt");
        }

        [Test]
        public void GetFiles_OnRoot_DoesNotContainDirectory()
        {
            Sut = new DirectoryHive(_assetsRoot);

            Sut.GetFiles(".", "*").Should().NotContain("bar");
        }

        [Test]
        public void GetFiles_InMissingDirectory_DoesNotThrow()
        {
            Sut = new DirectoryHive(_assetsRoot);

            Assert.DoesNotThrow(() => Sut.GetFiles("c:\\i_do_not_exist", "*"));
        }

        [Test]
        public void GetDirectories_AbsolutePaths()
        {
            Sut = new DirectoryHive(_assetsRoot);

            Sut.GetDirectories(".", "*", true).Should().Contain(Path.Combine(_assetsRoot, "foo"));
        }

        [Test]
        public void GetDirectories_OnRoot_ContainsDirectory()
        {
            Sut = new DirectoryHive(_assetsRoot);

            Sut.GetDirectories(".", "*").Should().Contain("foo");
        }

        [Test]
        public void GetDirectories_OnRoot_DoesNotContainFile()
        {
            Sut = new DirectoryHive(_assetsRoot);

            Sut.GetDirectories(".", "*").Should().NotContain("lorem.txt");
        }

        [Test]
        public void GetDirectories_InMissingDirectory_DoesNotThrow()
        {
            Sut = new DirectoryHive(_assetsRoot);

            Assert.DoesNotThrow(() => Sut.GetDirectories("c:\\i_do_not_exist", "*"));
        }
    }
}