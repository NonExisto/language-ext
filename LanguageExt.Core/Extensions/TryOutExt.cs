﻿using System.Collections.Generic;
using static LanguageExt.Prelude;
using System.Diagnostics.Contracts;

namespace LanguageExt;

public static class OutExtensions
{
    /// <summary>
    /// Get a value out of a dictionary as Some, otherwise None.
    /// </summary>
    /// <typeparam name="K">Key type</typeparam>
    /// <typeparam name="V">Value type</typeparam>
    /// <param name="self">Dictionary</param>
    /// <param name="Key">Key</param>
    /// <returns>OptionT filled Some(value) or None if value does not exist or is null</returns>
    [Pure]
    public static Option<V> TryGetValue<K, V>(this IDictionary<K, V> self, K Key) =>
        self.TryGetValue(Key, out var value)
            ? Optional(value)
            : None;

    /// <summary>
    /// Get a value out of a dictionary as Some, otherwise None.
    /// </summary>
    /// <typeparam name="K">Key type</typeparam>
    /// <typeparam name="V">Value type</typeparam>
    /// <param name="self">Dictionary</param>
    /// <param name="ReadOnlyKey">Key</param>
    /// <returns>OptionT filled Some(value) or None if value does not exist or is null</returns>
    [Pure]
    public static Option<V> TryGetValue<K, V>(this IReadOnlyDictionary<K, V> self, K ReadOnlyKey) =>
        self.TryGetValue(ReadOnlyKey, out var value)
            ? Optional(value)
            : None;
}
