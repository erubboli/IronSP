using System;

namespace IronSharePoint.Framework.Util
{
    public static class StringExtensions
    {
        public static string ReplaceFirst(this string source, string pattern, string substitute)
        {
            var index = source.IndexOf(pattern, StringComparison.InvariantCulture);
            if (index < 0) return source;

            return source.Substring(0, index) + substitute + source.Substring(index + pattern.Length);
        }
    }
}