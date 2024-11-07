﻿using System.Diagnostics.Contracts;
using LanguageExt.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt.ClassInstances;

/// <summary>
/// HashSet hash
/// </summary>
public struct HashableHashSet<HashA, A> : Hashable<HashSet<A>> where HashA : Hashable<A>
{
    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    public static int GetHashCode(HashSet<A> x) =>
        hash<HashA, A>(x);
}

/// <summary>
/// HashSet hash
/// </summary>
public struct HashableHashSet<A> : Hashable<HashSet<A>>
{
    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    public static int GetHashCode(HashSet<A> x) =>
        HashableHashSet<HashableDefault<A>, A>.GetHashCode(x);
}
