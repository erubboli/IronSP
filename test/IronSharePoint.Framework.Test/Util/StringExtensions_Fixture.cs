using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using IronSharePoint.Framework.Util;
using FluentAssertions;

namespace IronSharePoint.Framework.Test.Util
{
    [TestFixture]
    class StringExtensions_Fixture
    {
        [Test]
        public void ReplaceFirst_ReplacesOnlyTheFirstMatch()
        {
            "bar bar baz".ReplaceFirst("bar", "foo").Should().Be("foo bar baz");
        }
    }
}
