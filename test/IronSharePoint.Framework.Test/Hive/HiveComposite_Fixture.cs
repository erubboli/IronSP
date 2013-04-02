using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using IronSharePoint.Hives;
using Moq;
using NUnit.Framework;

namespace IronSharePoint.Framework.Test.Hive
{
    [TestFixture]
    class HiveComposite_Fixture
    {
        private Mock<IHive> _hiveMock1;
        private Mock<IHive> _hiveMock2;

        public HiveComposite Sut;

        IHive Hive1 { get { return _hiveMock1.Object; } }
        IHive Hive2 { get { return _hiveMock2.Object; } }

        [SetUp]
        public void SetUp()
        {
            _hiveMock1 = new Mock<IHive>();
            _hiveMock2 = new Mock<IHive>();
        }

        [Test]
        public void Ctor_RemovesNulls()
        {
            Sut = new HiveComposite(Hive1, null, Hive2);

            Sut.Should().NotContainNulls();
        }

        [Test]
        public void Ctor_PreservesOrder()
        {
            Sut = new HiveComposite(Hive1, Hive2);

            Sut.Should().BeEquivalentTo(new object[] {Hive1, Hive2});
        }

        [Test]
        public void Prepend_AddsHiveAsFirstItem()
        {
            Sut = new HiveComposite(Hive1);

            Sut.Prepend(Hive2);

            Sut.First().Should().BeSameAs(Hive2);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prepend_WithNull_ThrowsArgumentNullException()
        {
            Sut = new HiveComposite();

            Sut.Prepend(null);
        }

        [Test]
        public void Append_AddsHiveAsLastItem()
        {
            Sut = new HiveComposite(Hive1);

            Sut.Append(Hive2);

            Sut.Last().Should().BeSameAs(Hive2);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void Append_WithNull_ThrowsArgumentNullException()
        {
            Sut = new HiveComposite();

            Sut.Prepend(null);
        }

        [Test]
        public void FileExists_WhenNotInAnyHive_ReturnsFalse()
        {
            var path = "foo.txt";
            _hiveMock1.Setup(x => x.FileExists(path)).Returns(false);
            _hiveMock2.Setup(x => x.FileExists(path)).Returns(false);
            Sut = new HiveComposite(Hive1, Hive2);

            Sut.FileExists(path).Should().BeFalse();
        }

        [Test]
        public void FileExists_WhenInAnyHive_ReturnsTrue()
        {
            var path = "foo.txt";
            _hiveMock1.Setup(x => x.FileExists(path)).Returns(true);
            _hiveMock2.Setup(x => x.FileExists(path)).Returns(false);
            Sut = new HiveComposite(Hive1, Hive2);

            Sut.FileExists(path).Should().BeTrue();
        }

        [Test]
        public void DirectoryExists_WhenNotInAnyHive_ReturnsFalse()
        {
            var path = "foo";
            _hiveMock1.Setup(x => x.DirectoryExists(path)).Returns(false);
            _hiveMock2.Setup(x => x.DirectoryExists(path)).Returns(false);
            Sut = new HiveComposite(Hive1, Hive2);

            Sut.DirectoryExists(path).Should().BeFalse();
        }

        [Test]
        public void DirectoryExists_WhenInAnyHive_ReturnsTrue()
        {
            var path = "foo";
            _hiveMock1.Setup(x => x.DirectoryExists(path)).Returns(false);
            _hiveMock2.Setup(x => x.DirectoryExists(path)).Returns(true);
            Sut = new HiveComposite(Hive1, Hive2);

            Sut.DirectoryExists(path).Should().BeTrue();
        }

        [Test]
        public void GetFullPath_WhenPathNotInAnyHive_ReturnsNull()
        {
            var path = "foo.txt";
            _hiveMock1.Setup(x => x.FileExists(path)).Returns(false);
            Sut = new HiveComposite(Hive1);

            Sut.GetFullPath(path).Should().Be(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetFullPath_WhenPathBlank_ThrowsArgumentException()
        {
            var path = "   ";
            Sut = new HiveComposite();

            Sut.GetFullPath(path);
        }

        [Test]
        public void GetFullPath_DelegatesToFirstMatch()
        {
            var path = "foo.txt";
            var expected = @"c:\foo.txt";

            _hiveMock1.Setup(x => x.FileExists(path)).Returns(true);
            _hiveMock1.Setup(x => x.GetFullPath(path)).Returns(expected).Verifiable();
            _hiveMock2.Setup(x => x.FileExists(path)).Returns(true);
            Sut = new HiveComposite(Hive1, Hive2);

            Sut.GetFullPath(path).Should().Be(expected);

            _hiveMock1.VerifyAll();
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void OpenInputFileStream_WhenNotInAnyPath_ThrowsFileNotFound()
        {
            var path = "foo.txt";
            _hiveMock1.Setup(x => x.FileExists(path)).Returns(false);
            Sut = new HiveComposite(Hive1);

            Sut.OpenInputFileStream(path);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void OpenInputFileStream_WhenPathIsBlank_ThrowsArgumentException()
        {
            var path = "   ";
            Sut = new HiveComposite();

            Sut.OpenInputFileStream(path);
        }

        [Test]
        public void OpenInputFileStream_DelegatesToFirstMatch()
        {
            var path = "foo.txt";
            using (var expected = new MemoryStream())
            {
                _hiveMock1.Setup(x => x.FileExists(path)).Returns(true);
                _hiveMock1.Setup(x => x.OpenInputFileStream(path)).Returns(expected).Verifiable();
                _hiveMock2.Setup(x => x.FileExists(path)).Returns(true);
                Sut = new HiveComposite(Hive1, Hive2);

                Sut.OpenInputFileStream(path).Should().Be(expected);

                _hiveMock1.VerifyAll();
            }
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void OpenOutputFileStream_WhenNotInAnyPath_ThrowsFileNotFound()
        {
            var path = "foo.txt";
            _hiveMock1.Setup(x => x.FileExists(path)).Returns(false);
            Sut = new HiveComposite(Hive1);

            Sut.OpenOutputFileStream(path);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void OpenOutputFileStream_WhenPathIsBlank_ThrowsArgumentException()
        {
            var path = "   ";
            Sut = new HiveComposite();

            Sut.OpenOutputFileStream(path);
        }

        [Test]
        public void OpenOutputFileStream_DelegatesToFirstMatch()
        {
            var path = "foo.txt";
            using (var expected = new MemoryStream())
            {
                _hiveMock1.Setup(x => x.FileExists(path)).Returns(true);
                _hiveMock1.Setup(x => x.OpenOutputFileStream(path)).Returns(expected).Verifiable();
                _hiveMock2.Setup(x => x.FileExists(path)).Returns(true);
                Sut = new HiveComposite(Hive1, Hive2);

                Sut.OpenOutputFileStream(path).Should().Be(expected);

                _hiveMock1.VerifyAll();
            }
        }

        [Test]
        public void IsAbsolutePath_WhenAbsolutePathInAnyHive_ReturnsTrue()
        {
            var path = "c:\foo.txt";
            _hiveMock1.Setup(x => x.IsAbsolutePath(path)).Returns(false);
            _hiveMock2.Setup(x => x.IsAbsolutePath(path)).Returns(true);

            Sut = new HiveComposite(Hive1, Hive2);

            Sut.IsAbsolutePath(path).Should().BeTrue();
        }

        [Test]
        public void IsAbsolutePath_WhenNoAbsolutePathInAnyHive_ReturnsFalse()
        {
            var path = "c:\foo.txt";
            _hiveMock1.Setup(x => x.IsAbsolutePath(path)).Returns(false);
            _hiveMock2.Setup(x => x.IsAbsolutePath(path)).Returns(false);

            Sut = new HiveComposite(Hive1, Hive2);

            Sut.IsAbsolutePath(path).Should().BeFalse();
        }

        [Test]
        public void GetFiles_RetunsUnionOfAllHives()
        {
            _hiveMock1.Setup(x => x.GetFiles("","", false)).Returns(new[]{"file1", "file2"});
            _hiveMock2.Setup(x => x.GetFiles("","", false)).Returns(new[]{"file2", "file3"});

            Sut = new HiveComposite(Hive1, Hive2);

            Sut.GetFiles("","").Should().BeEquivalentTo(new object[]{"file1", "file2", "file3"});
        }

        [Test]
        public void GetFiles_PassesSearchArgumentsToHives()
        {
            _hiveMock1.Setup(x => x.GetFiles(".", "*", true)).Returns(new string[0]).Verifiable();

            Sut = new HiveComposite(Hive1);

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
            Sut.GetFiles(".", "*", true).ToList(); // evaluate the enumerable
// ReSharper restore ReturnValueOfPureMethodIsNotUsed

            _hiveMock1.VerifyAll();
        }

        [Test]
        public void TEST()
        {
            System.Console.WriteLine(Path.IsPathRooted("foo.txt"));
        }
    }
}
