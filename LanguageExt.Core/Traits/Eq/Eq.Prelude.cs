﻿using LanguageExt.ClassInstances;
using LanguageExt.Traits;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LanguageExt;

public static partial class Prelude
{
    /// <summary>
    /// Structural equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool equals<EQ, A>(A x, A y) where EQ : Eq<A> =>
        EQ.Equals(x, y);

    /// <summary>
    /// Structural equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool equals<EQ, A>(Option<A> x, Option<A> y) where EQ : Eq<A> =>
        x.Equals<EQ>(y);

    /// <summary>
    /// Structural equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool equals<EQ, L, R>(Either<L, R> x, Either<L, R> y) where EQ : Eq<R> =>
        x.Equals<EQ>(y);

    /// <summary>
    /// Structural equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool equals<EQA, EQB, A, B>(Either<A, B> x, Either<A, B> y)
        where EQA : Eq<A>
        where EQB : Eq<B> =>
        x.Equals<EQA, EQB>(y);

    /// <summary>
    /// Structural equality test
    /// </summary>
    /// <param name="mx">The left hand side of the equality operation</param>
    /// <param name="my">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool equals<EQ, A>(A? mx, A? my)
        where EQ : Eq<A>
        where A : struct =>
        (mx, my) switch
        {
            (null, null) => true,
            (_, null)    => false,
            (null, _)    => false,
            var (x, y)   => EQ.Equals(x.Value, y.Value)
        };

    /// <summary>
    /// Structural equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool equals<EQ, A>(Lst<A> x, Lst<A> y) where EQ : Eq<A> =>
        EqLst<EQ, A>.Equals(x, y);

    /// <summary>
    /// Structural equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool equals<EQ, A>(HashSet<A> x, HashSet<A> y) where EQ : Eq<A> =>
        EqHashSet<EQ, A>.Equals(x, y);

    /// <summary>
    /// Structural equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool equals<EQ, A>(Que<A> x, Que<A> y) where EQ : Eq<A> =>
        EqQue<EQ, A>.Equals(x, y);

    /// <summary>
    /// Structural equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool equals<EQ, A>(Set<A> x, Set<A> y) where EQ : Eq<A> =>
        EqSet<EQ, A>.Equals(x, y);

    /// <summary>
    /// Structural equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool equals<EQ, A>(Arr<A> x, Arr<A> y) where EQ : Eq<A> =>
        EqArr<EQ, A>.Equals(x, y);

    /// <summary>
    /// Structural equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool equals<EQ, A>(A[] x, A[] y) where EQ : Eq<A> =>
        EqArray<EQ, A>.Equals(x, y);

    /// <summary>
    /// Structural equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool equals<EQ, A>(IEnumerable<A> x, IEnumerable<A> y) where EQ : Eq<A> =>
        EqEnumerable<EQ, A>.Equals(x, y);

    /// <summary>
    /// Structural equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool equals<EQ, A>(Seq<A> x, Seq<A> y) where EQ : Eq<A> =>
        EqSeq<EQ, A>.Equals(x, y);

    public static bool collectionEquals<T, EqA>(this IReadOnlyCollection<T> left, IReadOnlyCollection<T> right, bool ignoreHashCheck = false)
        where EqA : Eq<T>
    {
        if (ReferenceEquals(left, right)) return true;
        if (right is null) return false;
        if(left.Count != right.Count) return false;

        // If the hash code has been calculated on both sides then 
        // check for differences
        if (!ignoreHashCheck && left.GetHashCode() != right.GetHashCode())
        {
            return false;
        }

        // Iterate through both sides
        using var iterA = left.GetEnumerator();
        using var iterB = right.GetEnumerator();
        while (iterA.MoveNext() && iterB.MoveNext())
        {
            if (!EqA.Equals(iterA.Current, iterB.Current))
            {
                return false;
            }
        }

        return true;
    }

    public static bool collectionEquals<T>(this IReadOnlyCollection<T> left, IReadOnlyCollection<T>? right, IEqualityComparer<T> equalityComparer, bool ignoreHashCheck = false)
    {
        if (ReferenceEquals(left, right)) return true;
        if (right is null) return false;
        if(left.Count != right.Count) return false;

        // If the hash code has been calculated on both sides then 
        // check for differences
        if (!ignoreHashCheck && left.GetHashCode() != right.GetHashCode())
        {
            return false;
        }

        // Iterate through both sides
        using var iterA = left.GetEnumerator();
        using var iterB = right.GetEnumerator();
        while (iterA.MoveNext() && iterB.MoveNext())
        {
            if (!equalityComparer.Equals(iterA.Current, iterB.Current))
            {
                return false;
            }
        }

        return true;
    }
}
