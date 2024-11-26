﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace LanguageExt
{
    public static class CollectionFormat
    {
        /// <summary>
        /// Application wide setting for the maximum number of items 
        /// shown in a call to the `ToString` method of any LanguageExt 
        /// collection type.
        /// </summary>
        public const int MaxShortItems = 50;

        internal static string ToShortString<A>(IEnumerable<A> ma, string separator = ", ")
        {
            var items = ma.Take(MaxShortItems).ToList();

            return items.Count < MaxShortItems
                ? $"{string.Join(separator, items)}"
                : $"{string.Join(separator, items)} ...";
        }

        internal static string ToShortString<A>(IEnumerable<A> ma, int count, string separator = ", ") =>
            count <= MaxShortItems
                ? $"{string.Join(separator, ma)}"
                : $"{string.Join(separator, ma.Take(MaxShortItems))} ... {count - MaxShortItems} more";

        internal static string ToShortArrayString<A>(IEnumerable<A> ma, string separator = ", ") =>
            $"[{ToShortString(ma, separator)}]";

        internal static string ToShortArrayString<A>(IEnumerable<A> ma, int count, string separator = ", ") =>
            $"[{ToShortString(ma, count, separator)}]";

        internal static string ToFullString<A>(IEnumerable<A> ma, string separator = ", ") =>
            $"{string.Join(separator, ma)}";

        internal static string ToFullArrayString<A>(IEnumerable<A> ma, string separator = ", ") =>
            $"[{ToFullString(ma, separator)}]";
    }
}
