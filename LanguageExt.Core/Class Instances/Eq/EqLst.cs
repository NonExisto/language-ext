﻿using LanguageExt.Traits;
using System.Diagnostics.Contracts;

namespace LanguageExt.ClassInstances;

/// <summary>
/// List Equality test
/// </summary>
/// <typeparam name="EQ">Comparison type</typeparam>
/// <typeparam name="A">Element type</typeparam>
public struct EqLst<EQ, A> : Eq<Lst<A>> where EQ : Eq<A>
{
    /// <summary>
    /// Equality check
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool Equals(Lst<A> x, Lst<A> y)
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
    public static int GetHashCode(Lst<A> x) =>
        HashableLst<EQ, A>.GetHashCode(x);
}

/// <summary>
/// List Equality test with default comparison
/// </summary>
/// <typeparam name="A">Element type</typeparam>
public struct EqLst<A> : Eq<Lst<A>> 
{
    /// <summary>
    /// Equality check
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool Equals(Lst<A> x, Lst<A> y) =>
        EqLst<EqDefault<A>, A>.Equals(x, y);

    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    public static int GetHashCode(Lst<A> x) =>
        HashableLst<A>.GetHashCode(x);
}
