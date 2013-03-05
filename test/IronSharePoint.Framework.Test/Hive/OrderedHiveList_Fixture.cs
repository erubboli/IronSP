﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using IronSharePoint.Framework.Hives;
using Moq;
using NUnit.Framework;

namespace IronSharePoint.Framework.Test.Hive
{
    [TestFixture]
    class OrderedHiveList_Fixture
    {
        private Mock<IHive> _hiveMock1;
        private Mock<IHive> _hiveMock2;

        public OrderedHiveList Sut;

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
            Sut = new OrderedHiveList(Hive1, null, Hive2);

            Sut.Should().NotContainNulls();
        }

        [Test]
        public void Ctor_PreservesOrder()
        {
            Sut = new OrderedHiveList(Hive1, Hive2);

            Sut.Should().BeEquivalentTo(new object[] {Hive1, Hive2});
        }

        [Test]
        public void Prepend_AddsHiveAsFirstItem()
        {
            Sut = new OrderedHiveList(Hive1);

            Sut.Prepend(Hive2);

            Sut.First().Should().BeSameAs(Hive2);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prepend_ArgumentNullException()
        {
            Sut = new OrderedHiveList();

            Sut.Prepend(null);
        }

        [Test]
        public void Append_AddsHiveAsLastItem()
        {
            Sut = new OrderedHiveList(Hive1);

            Sut.Append(Hive2);

            Sut.Last().Should().BeSameAs(Hive2);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void Append_ArgumentNullException()
        {
            Sut = new OrderedHiveList();

            Sut.Prepend(null);
        }

        [Test]
        public void FileExists_ReturnsFalse_NoHiveReturnsTrue()
        {
            var path = "foo.txt";
            _hiveMock1.Setup(x => x.FileExists(path)).Returns(false);
            _hiveMock2.Setup(x => x.FileExists(path)).Returns(false);
            Sut = new OrderedHiveList(Hive1, Hive2);

            Sut.FileExists(path).Should().BeFalse();
        }

        [Test]
        public void FileExists_ReturnsTrue_AnyHiveReturnsTrue()
        {
            var path = "foo.txt";
            _hiveMock1.Setup(x => x.FileExists(path)).Returns(true);
            _hiveMock2.Setup(x => x.FileExists(path)).Returns(false);
            Sut = new OrderedHiveList(Hive1, Hive2);

            Sut.FileExists(path).Should().BeTrue();
        }

        [Test]
        public void DirectoryExists_ReturnsFalse_NoHiveReturnsTrue()
        {
            var path = "foo";
            _hiveMock1.Setup(x => x.DirectoryExists(path)).Returns(false);
            _hiveMock2.Setup(x => x.DirectoryExists(path)).Returns(false);
            Sut = new OrderedHiveList(Hive1, Hive2);

            Sut.DirectoryExists(path).Should().BeFalse();
        }

        [Test]
        public void DirectoryExists_ReturnsTrue_AnyHiveReturnsTrue()
        {
            var path = "foo";
            _hiveMock1.Setup(x => x.DirectoryExists(path)).Returns(false);
            _hiveMock2.Setup(x => x.DirectoryExists(path)).Returns(true);
            Sut = new OrderedHiveList(Hive1, Hive2);

            Sut.DirectoryExists(path).Should().BeTrue();
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void GetFullPath_FileNotFound()
        {
            var path = "foo.txt";
            _hiveMock1.Setup(x => x.FileExists(path)).Returns(false);
            Sut = new OrderedHiveList(Hive1);

            Sut.GetFullPath(path);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetFullPath_ArgumentException()
        {
            var path = "   ";
            Sut = new OrderedHiveList();

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
            Sut = new OrderedHiveList(Hive1, Hive2);

            Sut.GetFullPath(path).Should().Be(expected);

            _hiveMock1.VerifyAll();
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void OpenInputFileStream_FileNotFound()
        {
            var path = "foo.txt";
            _hiveMock1.Setup(x => x.FileExists(path)).Returns(false);
            Sut = new OrderedHiveList(Hive1);

            Sut.OpenInputFileStream(path);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void OpenInputFileStream_ArgumentException()
        {
            var path = "   ";
            Sut = new OrderedHiveList();

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
                Sut = new OrderedHiveList(Hive1, Hive2);

                Sut.OpenInputFileStream(path).Should().Be(expected);

                _hiveMock1.VerifyAll();
            }
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void OpenOutputFileStream_FileNotFound()
        {
            var path = "foo.txt";
            _hiveMock1.Setup(x => x.FileExists(path)).Returns(false);
            Sut = new OrderedHiveList(Hive1);

            Sut.OpenOutputFileStream(path);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void OpenOutputFileStream_ArgumentException()
        {
            var path = "   ";
            Sut = new OrderedHiveList();

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
                Sut = new OrderedHiveList(Hive1, Hive2);

                Sut.OpenOutputFileStream(path).Should().Be(expected);

                _hiveMock1.VerifyAll();
            }
        }
    }
}
