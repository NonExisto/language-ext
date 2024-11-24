﻿using LanguageExt.Traits;
using System.Diagnostics.Contracts;

namespace LanguageExt.ClassInstances;

/// <summary>
/// Equality and ordering
/// </summary>
public struct OrdArr<OrdA, A> : Ord<Arr<A>>
    where OrdA : Ord<A>
{
    /// <summary>
    /// Equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool Equals(Arr<A> x, Arr<A> y) =>
        EqArr<OrdA, A>.Equals(x, y);

    /// <summary>
    /// Compare two values
    /// </summary>
    /// <param name="mx">Left hand side of the compare operation</param>
    /// <param name="my">Right hand side of the compare operation</param>
    /// <returns>
    /// if x greater than y : 1
    /// if x less than y    : -1
    /// if x equals y       : 0
    /// </returns>
    [Pure]
    public static int Compare(Arr<A> mx, Arr<A> my) => 
        mx.CompareTo(my);

    /// <summary>
    /// Get the hash-code of the provided value
    /// </summary>
    /// <returns>Hash code of x</returns>
    [Pure]
    public static int GetHashCode(Arr<A> x) =>
        x.GetHashCode();
}

/// <summary>
/// Equality and ordering
/// </summary>
public struct OrdArr<A> : Ord<Arr<A>>
{
    /// <summary>
    /// Equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool Equals(Arr<A> x, Arr<A> y) =>
        EqArr<A>.Equals(x, y);

    /// <summary>
    /// Compare two values
    /// </summary>
    /// <param name="mx">Left hand side of the compare operation</param>
    /// <param name="my">Right hand side of the compare operation</param>
    /// <returns>
    /// if x greater than y : 1
    /// if x less than y    : -1
    /// if x equals y       : 0
    /// </returns>
    [Pure]
    public static int Compare(Arr<A> mx, Arr<A> my) =>
        OrdArr<OrdDefault<A>, A>.Compare(mx, my);

    /// <summary>
    /// Get the hash-code of the provided value
    /// </summary>
    /// <returns>Hash code of x</returns>
    [Pure]
    public static int GetHashCode(Arr<A> x) =>
        OrdArr<OrdDefault<A>, A>.GetHashCode(x);
}
