﻿using LanguageExt.Traits;
using System.Diagnostics.Contracts;

namespace LanguageExt.ClassInstances;

/// <summary>
/// Stack Equality test
/// </summary>
/// <typeparam name="EQ">Comparison type</typeparam>
/// <typeparam name="A">Element type</typeparam>
public struct EqStck<EQ, A> : Eq<Stck<A>> where EQ : Eq<A>
{
    /// <summary>
    /// Equality check
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool Equals(Stck<A> x, Stck<A> y)
    {
        if (x.Count != y.Count) return false;

        using var enumx = x.GetEnumerator();
        using var enumy = y.GetEnumerator();
        var count = x.Count;

        for (var i = 0; i < count; i++)
        {
            enumx.MoveNext();
            enumy.MoveNext();
            if (!EQ.Equals(enumx.Current, enumy.Current)) return false;
        }
        return true;
    }

    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    public static int GetHashCode(Stck<A> x) =>
        HashableStck<EQ, A>.GetHashCode(x);
}

/// <summary>
/// Equality test
/// </summary>
/// <typeparam name="A">Element type</typeparam>
public struct EqStck<A> : Eq<Stck<A>>
{
    /// <summary>
    /// Equality check
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool Equals(Stck<A> x, Stck<A> y) =>
        EqStck<EqDefault<A>, A>.Equals(x, y);

    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    public static int GetHashCode(Stck<A> x) =>
        HashableStck<A>.GetHashCode(x);
}
