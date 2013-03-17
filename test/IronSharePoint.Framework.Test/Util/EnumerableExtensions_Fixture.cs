﻿using FluentAssertions;
using IronSharePoint.Util;
using NUnit.Framework;

namespace IronSharePoint.Framework.Test.Util
{
    [TestFixture]
    public class EnumerableExtensions_Fixture
    {
        [Test]
        public void Compact_RemovesAllNulls()
        {
            new[] {"Foo", null, "Bar", null}.Compact().Should().ContainInOrder(new[] {"Foo", "Bar"});
        }
    }
}