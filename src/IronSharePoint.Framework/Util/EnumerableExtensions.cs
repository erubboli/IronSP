﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace IronSharePoint.Util
{
    internal static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        /// <summary>
        /// Removes all nulls from the enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static IEnumerable<T> Compact<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Where(x => !Equals(x, default(T)));
        }

        public static string StringJoin<T>(this IEnumerable<T> enumerable, string seperator = ", ")
        {
            var values = enumerable.Select(x => x == null ? "null" : x.ToString());
            return String.Join(seperator, values);
        }
    }
}
