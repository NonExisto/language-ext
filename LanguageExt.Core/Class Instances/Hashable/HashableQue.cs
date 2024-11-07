﻿using System.Diagnostics.Contracts;
using LanguageExt.Traits;

namespace LanguageExt.ClassInstances;

/// <summary>
/// Queue hashing
/// </summary>
public struct HashableQue<HashA, A> : Hashable<Que<A>> where HashA : Hashable<A>
{
    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    public static int GetHashCode(Que<A> x) =>
        Prelude.hash<HashA, A>(x);
}

/// <summary>
/// Queue hashing
/// </summary>
public struct HashableQue<A> : Hashable<Que<A>>
{
    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    public static int GetHashCode(Que<A> x) =>
        HashableQue<EqDefault<A>, A>.GetHashCode(x);
}
