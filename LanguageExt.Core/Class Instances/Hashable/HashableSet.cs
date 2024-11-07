﻿using System.Diagnostics.Contracts;
using LanguageExt.Traits;

namespace LanguageExt.ClassInstances;

/// <summary>
/// Set<T> hashing
/// </summary>
public struct HashableSet<HashA, A> : Hashable<Set<A>> where HashA : Hashable<A>
{
    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    public static int GetHashCode(Set<A> x) =>
        Prelude.hash<HashA, A>(x);
}

/// <summary>
/// Set<T> hashing
/// </summary>
public struct HashableSet<A> : Hashable<Set<A>>
{
    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    public static int GetHashCode(Set<A> x) =>
        HashableSet<HashableDefault<A>, A>.GetHashCode(x);
}
