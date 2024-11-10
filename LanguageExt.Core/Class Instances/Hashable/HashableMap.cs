﻿using LanguageExt.Traits;
using System.Diagnostics.Contracts;

namespace LanguageExt.ClassInstances;

public struct HashableMap<K, V> : Hashable<Map<K, V>>
{
    /// <summary>
    /// Get the hash-code of the provided value
    /// </summary>
    /// <returns>Hash code of `x`</returns>
    [Pure]
    public static int GetHashCode(Map<K, V> x) =>
        x.GetHashCode();
}
