﻿using System;
using System.Collections.Generic;
using static LanguageExt.Prelude;
using System.Diagnostics.Contracts;
using LanguageExt.Traits;
using LanguageExt.Common;

namespace LanguageExt;

/// <summary>
/// Extension methods for Either
/// </summary>
public static partial class EitherExtensions
{
    public static Either<L, R> As<L, R>(this K<Either<L>, R> ma) =>
        (Either<L, R>)ma;
 
    /// <summary>
    /// Monadic join
    /// </summary>
    [Pure]
    public static Either<L, R> Flatten<L, R>(this K<Either<L>, Either<L, R>> ma) =>
        ma.As().Bind(x => x);
 
    /// <summary>
    /// Monadic join
    /// </summary>
    [Pure]
    public static Either<L, R> Flatten<L, R>(this K<Either<L>, K<Either<L>, R>> ma) =>
        ma.As().Bind(x => x);

    /// <summary>
    /// Extracts from a list of 'Either' all the 'Left' elements.
    /// All the 'Left' elements are extracted in order.
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="self">Either list</param>
    /// <returns>An enumerable of L</returns>
    [Pure]
    public static IEnumerable<L> Lefts<L, R>(this IEnumerable<Either<L, R>> self)
    {
        foreach (var item in self)
        {
            if (item.IsLeft)
            {
                yield return item.LeftValue;
            }
        }
    }

    /// <summary>
    /// Extracts from a list of 'Either' all the 'Left' elements.
    /// All the 'Left' elements are extracted in order.
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="self">Either list</param>
    /// <returns>An enumerable of L</returns>
    [Pure]
    public static Seq<L> Lefts<L, R>(this Seq<Either<L, R>> self) =>
        Lefts(self.AsEnumerable()).AsIterable().ToSeq();

    /// <summary>
    /// Extracts from a list of 'Either' all the 'Right' elements.
    /// All the 'Right' elements are extracted in order.
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="self">Either list</param>
    /// <returns>An enumerable of L</returns>
    [Pure]
    public static IEnumerable<R> Rights<L, R>(this IEnumerable<Either<L, R>> self)
    {
        foreach (var item in self)
        {
            if (item.IsRight)
            {
                yield return item.RightValue;
            }
        }
    }

    /// <summary>
    /// Extracts from a list of 'Either' all the 'Right' elements.
    /// All the 'Right' elements are extracted in order.
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="self">Either list</param>
    /// <returns>An enumerable of L</returns>
    [Pure]
    public static Seq<R> Rights<L, R>(this Seq<Either<L, R>> self) =>
        Rights(self.AsEnumerable()).AsIterable().ToSeq();

    /// <summary>
    /// Partitions a list of 'Either' into two lists.
    /// All the 'Left' elements are extracted, in order, to the first
    /// component of the output.  Similarly the 'Right' elements are extracted
    /// to the second component of the output.
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="self">Either list</param>
    /// <returns>A tuple containing the an enumerable of L and an enumerable of R</returns>
    [Pure]
    public static (IEnumerable<L> Lefts, IEnumerable<R> Rights) Partition<L, R>(this IEnumerable<Either<L, R>> self)
    {
        var ls = new List<L>();
        var rs = new List<R>();
        foreach (var item in self)
        {
            if (item.IsRight) rs.Add(item.RightValue);
            if (item.IsLeft) ls.Add(item.LeftValue);
        }
        return (ls, rs);
    }

    /// <summary>
    /// Partitions a list of 'Either' into two lists.
    /// All the 'Left' elements are extracted, in order, to the first
    /// component of the output.  Similarly the 'Right' elements are extracted
    /// to the second component of the output.
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="self">Either list</param>
    /// <returns>A tuple containing the an enumerable of L and an enumerable of R</returns>
    [Pure]
    public static (Seq<L> Lefts, Seq<R> Rights) Partition<L, R>(this Seq<Either<L, R>> self)
    {
        var (l, r) = self.AsEnumerable().Partition();
        return (l.AsIterable().ToSeq(), r.AsIterable().ToSeq());
    }

    [Pure]
    public static Validation<L, R> ToValidation<L, R>(this Either<L, R> ma)
        where L : Monoid<L> =>
        ma switch
        {
            Either.Right<L, R>       => Pure(ma.RightValue),
            Either.Left<L, R> => Fail(ma.LeftValue),
            _                 => throw new BottomException()
        };

    /// <summary>
    /// Convert to an Eff
    /// </summary>
    /// <returns>Eff monad</returns>
    [Pure]
    public static Eff<R> ToEff<R>(this Either<Error, R> ma) =>
        ma switch
        {
            Either.Right<Error, R> => Pure(ma.RightValue),
            Either.Left<Error, R>  => Fail(ma.LeftValue),
            _                      => throw new BottomException()
        };

    /// <summary>
    /// Convert to an Eff
    /// </summary>
    /// <returns>Eff monad</returns>
    [Pure]
    public static Eff<R> ToEff<R>(this Either<Exception, R> ma) =>
        ma switch
        {
            Either.Right<Exception, R> => Pure(ma.RightValue),
            Either.Left<Exception, R>  => Fail<Error>(ma.LeftValue),
            _                          => throw new BottomException()
        };

    /// <summary>
    /// Convert to an Eff
    /// </summary>
    /// <returns>Eff monad</returns>
    [Pure]
    public static Eff<R> ToEff<R>(this Either<string, R> ma) =>
        ma switch
        {
            Either.Right<string, R> => Pure(ma.RightValue),
            Either.Left<string, R>  => Fail(Error.New(ma.LeftValue)),
            _                       => throw new BottomException()
        };
}
