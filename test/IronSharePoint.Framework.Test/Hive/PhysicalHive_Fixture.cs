using System;
using System.IO;
using FluentAssertions;
using IronSharePoint.Framework.Hives;
using NUnit.Framework;

namespace IronSharePoint.Framework.Test.Hive
{
    [TestFixture]
    class PhysicalHive_Fixture
    {
        public PhysicalHive Sut;

        private string _assetsRoot;

        [SetUp]
        public void SetUp()
        {
            _assetsRoot = Path.Combine(Directory.GetCurrentDirectory(), "_assets");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Ctor_ArgumentException()
        {
            Sut = new PhysicalHive(@"c:\i_do_not_exist");
        }
        
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_ArgumentNullException()
        {
            Sut = new PhysicalHive(null);
        }
        
        [Test]
        public void Ctor_AssignsRoot()
        {
            Sut = new PhysicalHive(_assetsRoot);

            Sut.Root.Should().Be(_assetsRoot);
        }

        [Test]
        public void FileExists_ReturnsFalse_FileDoesntExist()
        {
            Sut = new PhysicalHive(_assetsRoot);

            Sut.FileExists("i_do_not.exist").Should().BeFalse();
        }

        [Test]
        public void FileExists_ReturnsTrue_FileDoesExist()
        {
            Sut = new PhysicalHive(_assetsRoot);

            Sut.FileExists("lorem.txt").Should().BeTrue();
        }

        [Test]
        public void DirectoryExists_ReturnsFalse_FileDoesntExist()
        {
            Sut = new PhysicalHive(_assetsRoot);

            Sut.DirectoryExists("i_do_not_exist").Should().BeFalse();
        }

        [Test]
        public void DirectoryExists_ReturnsTrue_FileDoesExist()
        {
            Sut = new PhysicalHive(_assetsRoot);

            Sut.DirectoryExists("foo").Should().BeTrue();
        }

        [Test]
        public void GetFullPath_CombinesRootAndPartialPath()
        {
            Sut = new PhysicalHive(_assetsRoot);
            var partial = "lorem.txt";
            var expected = Path.Combine(Sut.Root, partial);

            Sut.GetFullPath(partial).Should().Be(expected);
        }

        [Test]
        public void GetFullPath_ReturnsPath_Rooted()
        {
            Sut = new PhysicalHive(_assetsRoot);
            var expected = @"c:\foo.txt";

            Sut.GetFullPath(expected).Should().Be(expected);
        }

        [Test]
        public void OpenInputFileStream_OpensFile()
        {
            Sut = new PhysicalHive(_assetsRoot);
            var expected = File.ReadAllBytes(Path.Combine(_assetsRoot, "lorem.txt")).Length;

            using (var stream = Sut.OpenInputFileStream("lorem.txt"))
            {
                stream.Length.Should().Be(expected);
            }
        }

        [Test]
        public void OpenInputFileStream_CanWrite()
        {
            Sut = new PhysicalHive(_assetsRoot);

            using (var stream = Sut.OpenInputFileStream("lorem.txt"))
            {
                stream.CanWrite.Should().BeTrue();
            }
        }

        [Test]
        public void OpenOutputFileStream_OpensFile()
        {
            Sut = new PhysicalHive(_assetsRoot);
            var expected = File.ReadAllBytes(Path.Combine(_assetsRoot, "lorem.txt")).Length;

            using (var stream = Sut.OpenOutputFileStream("lorem.txt"))
            {
                stream.Length.Should().Be(expected);
            }
        }

        [Test]
        public void OpenOutputFileStream_CanWrite()
        {
            Sut = new PhysicalHive(_assetsRoot);

            using (var stream = Sut.OpenOutputFileStream("lorem.txt"))
            {
                stream.CanRead.Should().BeTrue();
            }
        }
    }
}