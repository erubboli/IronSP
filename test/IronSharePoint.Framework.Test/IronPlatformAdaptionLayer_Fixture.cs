using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace IronSharePoint.Framework.Test
{
    [TestFixture]
    public class IronPlatformAdaptionLayer_Fixture
    {
        public IronPlatformAdaptationLayer Sut;

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void TrimPath_RemovesLeadingRelativePath()
        {
            Sut = new IronPlatformAdaptationLayer(null);

            Sut.TrimPath("./foo").Should().Be("foo");
            Sut.TrimPath(".\\foo").Should().Be("foo");
        }

        [Test]
        public void TrimPath_RemovesLeadingFakeHiveDirectory()
        {
            Sut = new IronPlatformAdaptationLayer(null);

            string path = IronConstant.FakeHiveDirectory + "foo";
            Sut.TrimPath(path).Should().Be("foo");
            Sut.TrimPath(path.Replace('\\','/')).Should().Be("foo");
        }


        [Test]
        public void TrimPath_RemovesLeadingSlashes()
        {
            Sut = new IronPlatformAdaptationLayer(null);

            Sut.TrimPath("\\foo").Should().Be("foo");
            Sut.TrimPath("/foo").Should().Be("foo");
        }
    }
}
