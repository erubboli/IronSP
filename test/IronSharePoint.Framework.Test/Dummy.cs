using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace IronSharePoint.Framework.Test
{
    [TestFixture]
    class Dummy
    {
        [Test]
        public void Test()
        {
            true.Should().BeTrue("works");
        }
    }
}
