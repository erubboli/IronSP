using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronSharePoint.Util;
using NUnit.Framework;
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

        [Test]
        public void ReplaceStart_ReplacesPatternWithSubstitute()
        {
            "foobar".ReplaceStart("foo", "baz").Should().Be("bazbar");
        }

        [Test]
        public void ReplaceStart_ReplacesOnlyWhenStartMatchesPattern()
        {
            "foobar".ReplaceStart("bar", "qux").Should().Be("foobar");
        }
    }
}
