﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LanguageExt.Traits;

namespace LanguageExt;

public static partial class Prelude
{
    /// <summary>
    /// Either constructor
    /// Constructs an Either in a Right state
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="value">Right value</param>
    /// <returns>A new Either instance</returns>
    [Pure]
    public static Either<L, R> Right<L, R>(R value) =>
        new Either.Right<L, R>(value);

    /// <summary>
    /// Constructs an EitherRight which can be implicitly cast to an 
    /// Either&lt;_, R&gt;
    /// </summary>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="value">Right value</param>
    /// <returns>A new EitherRight instance</returns>
    [Pure]
    public static Pure<R> Right<R>(R value) =>
        new (value);
    
    /// <summary>
    /// Either constructor
    /// Constructs an Either in a Left state
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="value">Left value</param>
    /// <returns>A new Either instance</returns>
    [Pure]
    public static Either<L, R> Left<L, R>(L value) =>
        new Either.Left<L, R>(value);

    /// <summary>
    /// Constructs an EitherLeft which can be implicitly cast to an 
    /// Either&lt;L, _&gt;
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <param name="value">Right value</param>
    /// <returns>A new EitherLeft instance</returns>
    [Pure]
    public static Fail<L> Left<L>(L value) =>
        new (value);

    /// <summary>
    /// Monadic join
    /// </summary>
    [Pure]
    public static Either<L, R> flatten<L, R>(Either<L, Either<L, R>> ma) =>
        ma.Bind(x => x);

    /// <summary>
    /// Add the bound values of x and y, uses an Add trait to provide the add
    /// operation for type A.  For example `x.Add&lt;TInteger,int&gt;(y)`
    /// </summary>
    /// <typeparam name="NUM">Num of A</typeparam>
    /// <typeparam name="L">Left bound value type</typeparam>
    /// <typeparam name="R">Right bound value type</typeparam>
    /// <param name="x">Left hand side of the operation</param>
    /// <param name="y">Right hand side of the operation</param>
    /// <returns>An option with y added to x</returns>
    [Pure]
    public static Either<L, R> plus<NUM, L, R>(Either<L, R> x, Either<L, R> y) where NUM : Arithmetic<R> =>
        from a in x
        from b in y
        select NUM.Add(a, b);

    /// <summary>
    /// Find the subtract between the two bound values of x and y, uses a Subtract trait 
    /// to provide the subtract operation for type A.  For example `x.Subtract&lt;TInteger,int&gt;(y)`
    /// </summary>
    /// <typeparam name="NUM">Num of A</typeparam>
    /// <typeparam name="L">Left bound value type</typeparam>
    /// <typeparam name="R">Right bound value type</typeparam>
    /// <param name="x">Left hand side of the operation</param>
    /// <param name="y">Right hand side of the operation</param>
    /// <returns>An option with the subtract between x and y</returns>
    [Pure]
    public static Either<L, R> subtract<NUM, L, R>(Either<L, R> x, Either<L, R> y) where NUM : Arithmetic<R> =>
        from a in x
        from b in y
        select NUM.Subtract(a, b);

    /// <summary>
    /// Find the product between the two bound values of x and y, uses a Product trait 
    /// to provide the product operation for type A.  For example `x.Product&lt;TInteger,int&gt;(y)`
    /// </summary>
    /// <typeparam name="NUM">Num of A</typeparam>
    /// <typeparam name="L">Left bound value type</typeparam>
    /// <typeparam name="R">Right bound value type</typeparam>
    /// <param name="x">Left hand side of the operation</param>
    /// <param name="y">Right hand side of the operation</param>
    /// <returns>An option with the product of x and y</returns>
    [Pure]
    public static Either<L, R> product<NUM, L, R>(Either<L, R> x, Either<L, R> y) where NUM : Arithmetic<R> =>
        from a in x
        from b in y
        select NUM.Multiply(a, b);

    /// <summary>
    /// Divide the two bound values of x and y, uses a Divide trait to provide the divide
    /// operation for type A.  For example `x.Divide&lt;TDouble,double&gt;(y)`
    /// </summary>
    /// <typeparam name="NUM">Num of A</typeparam>
    /// <typeparam name="L">Left bound value type</typeparam>
    /// <typeparam name="R">Right bound value type</typeparam>
    /// <param name="x">Left hand side of the operation</param>
    /// <param name="y">Right hand side of the operation</param>
    /// <returns>An option x / y</returns>
    [Pure]
    public static Either<L, R> divide<NUM, L, R>(Either<L, R> x, Either<L, R> y) where NUM : Num<R> =>
        from a in x
        from b in y
        select NUM.Divide(a, b);

    /// <summary>
    /// Returns the state of the Either provided
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="value">Either to check</param>
    /// <returns>True if the Either is in a Right state</returns>
    [Pure]
    public static bool isRight<L, R>(Either<L, R> value) =>
        value.IsRight;

    /// <summary>
    /// Returns the state of the Either provided
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="value">Either to check</param>
    /// <returns>True if the Either is in a Left state</returns>
    [Pure]
    public static bool isLeft<L, R>(Either<L, R> value) =>
        value.IsLeft;

    /// <summary>
    /// Executes the Left function if the Either is in a Left state.
    /// Returns the Right value if the Either is in a Right state.
    /// </summary>
    /// <param name="either">Either</param>
    /// <param name="Left">Function to generate a Right value if in the Left state</param>
    /// <returns>Returns an unwrapped Right value</returns>
    [Pure]
    public static R ifLeft<L, R>(Either<L, R> either, Func<R> Left) =>
        either.IfLeft(Left);

    /// <summary>
    /// Executes the leftMap function if the Either is in a Left state.
    /// Returns the Right value if the Either is in a Right state.
    /// </summary>
    /// <param name="either">Either</param>
    /// <param name="leftMap">Function to generate a Right value if in the Left state</param>
    /// <returns>Returns an unwrapped Right value</returns>
    [Pure]
    public static R ifLeft<L, R>(Either<L, R> either, Func<L, R> leftMap) =>
        either.IfLeft(leftMap);

    /// <summary>
    /// Returns the rightValue if the Either is in a Left state.
    /// Returns the Right value if the Either is in a Right state.
    /// </summary>
    /// <param name="either">Either</param>
    /// <param name="rightValue">Value to return if in the Left state</param>
    /// <returns>Returns an unwrapped Right value</returns>
    [Pure]
    public static R ifLeft<L, R>(Either<L, R> either, R rightValue) =>
        either.IfLeft(rightValue);

    /// <summary>
    /// Executes the Left action if the Either is in a Left state.
    /// </summary>
    /// <param name="either">Either</param>
    /// <param name="Left">Function to generate a Right value if in the Left state</param>
    /// <returns>Returns an unwrapped Right value</returns>
    [Pure]
    public static Unit ifLeft<L, R>(Either<L, R> either, Action<L> Left) =>
        either.IfLeft(Left);

    /// <summary>
    /// Invokes the Right action if the Either is in a Right state, otherwise does nothing
    /// </summary>
    /// <param name="either">Either</param>
    /// <param name="Right">Action to invoke</param>
    /// <returns>Unit</returns>
    [Pure]
    public static Unit ifRight<L, R>(Either<L, R> either, Action<R> Right) =>
        either.IfRight(Right);

    /// <summary>
    /// Returns the leftValue if the Either is in a Right state.
    /// Returns the Left value if the Either is in a Left state.
    /// </summary>
    /// <param name="either">Either</param>
    /// <param name="leftValue">Value to return if in the Left state</param>
    /// <returns>Returns an unwrapped Left value</returns>
    [Pure]
    public static L ifRight<L, R>(Either<L, R> either, L leftValue) =>
        either.IfRight(leftValue);

    /// <summary>
    /// Returns the result of Left() if the Either is in a Right state.
    /// Returns the Left value if the Either is in a Left state.
    /// </summary>
    /// <param name="either">Either</param>
    /// <param name="Left">Function to generate a Left value if in the Right state</param>
    /// <returns>Returns an unwrapped Left value</returns>
    [Pure]
    public static L ifRight<L, R>(Either<L, R> either, Func<L> Left) =>
        either.IfRight(Left);

    /// <summary>
    /// Returns the result of leftMap if the Either is in a Right state.
    /// Returns the Left value if the Either is in a Left state.
    /// </summary>
    /// <param name="either">Either</param>
    /// <param name="leftMap">Function to generate a Left value if in the Right state</param>
    /// <returns>Returns an unwrapped Left value</returns>
    [Pure]
    public static L ifRight<L, R>(Either<L, R> either, Func<R, L> leftMap) =>
        either.IfRight(leftMap);

    /// <summary>
    /// Invokes the Right or Left function depending on the state of the Either provided
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <typeparam name="Ret">Return type</typeparam>
    /// <param name="either">Either to match</param>
    /// <param name="Right">Function to invoke if in a Right state</param>
    /// <param name="Left">Function to invoke if in a Left state</param>
    /// <returns>The return value of the invoked function</returns>
    [Pure]
    public static Ret match<L, R, Ret>(Either<L, R> either, Func<L, Ret> Left, Func<R, Ret> Right) =>
        either.Match(Left, Right);

    /// <summary>
    /// Invokes the Right or Left action depending on the state of the Either provided
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="either">Either to match</param>
    /// <param name="Right">Action to invoke if in a Right state</param>
    /// <param name="Left">Action to invoke if in a Left state</param>
    /// <returns>Unit</returns>
    public static Unit match<L, R>(Either<L, R> either, Action<L> Left, Action<R> Right) =>
        either.Match(Left, Right);

    /// <summary>
    /// <para>
    /// Either types are like lists of 0 or 1 items, and therefore follow the 
    /// same rules when folding.
    /// </para><para>
    /// In the case of lists, 'Fold', when applied to a binary
    /// operator, a starting value(typically the left-identity of the operator),
    /// and a list, reduces the list using the binary operator, from left to
    /// right:
    /// </para>
    /// </summary>
    /// <typeparam name="S">Aggregate state type</typeparam>
    /// <typeparam name="L">Left bound value type</typeparam>
    /// <typeparam name="R">Right bound value type</typeparam>
    /// <param name="either">Either</param>
    /// <param name="state">Initial state</param>
    /// <param name="Right">Folder function, applied if Either is in a Right state</param>
    /// <param name="Left">Folder function, applied if Either is in a Left state</param>
    /// <returns>The aggregate state</returns>
    [Pure]
    public static S bifold<L, R, S>(Either<L, R> either, S state, Func<S, L, S> Left, Func<S, R, S> Right) =>
        either.BiFold(state, Left, Right);

    /// <summary>
    /// Invokes a predicate on the value of the Either if it's in the Right state
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="either">Either to forall</param>
    /// <param name="pred">Predicate</param>
    /// <returns>True if the Either is in a Left state.  
    /// True if the Either is in a Right state and the predicate returns True.  
    /// False otherwise.</returns>
    [Pure]
    public static bool forall<L, R>(Either<L, R> either, Func<R, bool> pred) =>
        either.ForAll(pred);

    /// <summary>
    /// Invokes a predicate on the value of the Either if it's in the Right state
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="either">Either to forall</param>
    /// <param name="Right">Right predicate</param>
    /// <param name="Left">Left predicate</param>
    /// <returns>True if the predicate returns True.  True if the Either is in a bottom state.</returns>
    [Pure]
    public static bool biforall<L, R>(Either<L, R> either, Func<L, bool> Left, Func<R, bool> Right) =>
        either.BiForAll(Left, Right);

    /// <summary>
    /// Counts the Either
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="either">Either to count</param>
    /// <returns>1 if the Either is in a Right state, 0 otherwise.</returns>
    [Pure]
    public static int count<L, R>(Either<L, R> either) =>
        either.Count();

    /// <summary>
    /// Invokes a predicate on the value of the Either if it's in the Right state
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="either">Either to check existence of</param>
    /// <param name="pred">Predicate</param>
    /// <returns>True if the Either is in a Right state and the predicate returns True.  False otherwise.</returns>
    [Pure]
    public static bool exists<L, R>(Either<L, R> either, Func<R, bool> pred) =>
        either.Exists(pred);

    /// <summary>
    /// Maps the value in the Either if it's in a Right state
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <typeparam name="Ret">Mapped Either type</typeparam>
    /// <param name="either">Either to map</param>
    /// <param name="mapper">Map function</param>
    /// <returns>Mapped Either</returns>
    [Pure]
    public static Either<L, Ret> map<L, R, Ret>(Either<L, R> either, Func<R, Ret> mapper) =>
        either.Map(mapper);

    /// <summary>
    /// Bi-maps the value in the Either if it's in a Right state
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <typeparam name="LRet">Left return</typeparam>
    /// <typeparam name="RRet">Right return</typeparam>
    /// <param name="either">Either to map</param>
    /// <param name="Right">Right map function</param>
    /// <param name="Left">Left map function</param>
    /// <returns>Mapped Either</returns>
    [Pure]
    public static Either<LRet, RRet> bimap<L, R, LRet, RRet>(Either<L, R> either, Func<L, LRet> Left, Func<R, RRet> Right) =>
        either.BiMap(Left, Right);

    /// <summary>
    /// Monadic bind function
    /// https://en.wikipedia.org/wiki/Monad_(functional_programming)
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <typeparam name="Ret"></typeparam>
    /// <param name="either"></param>
    /// <param name="binder"></param>
    /// <returns>Bound Either</returns>
    [Pure]
    public static Either<L, Ret> bind<L, R, Ret>(Either<L, R> either, Func<R, Either<L, Ret>> binder) =>
        either.Bind(binder);

    /// <summary>
    /// Match over a sequence of Eithers
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <typeparam name="Ret">Mapped type</typeparam>
    /// <param name="list">Sequence to match over</param>
    /// <param name="Right">Right match function</param>
    /// <param name="Left">Left match function</param>
    /// <returns>Sequence of mapped values</returns>
    [Pure]
    public static Iterable<Ret> Match<L, R, Ret>(
        this IEnumerable<Either<L, R>> list,
        Func<R, Ret> Right,
        Func<L, Ret> Left) =>
        match(list, Right, Left);

    /// <summary>
    /// Match over a sequence of Eithers
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <typeparam name="Ret">Mapped type</typeparam>
    /// <param name="list">Sequence to match over</param>
    /// <param name="Right">Right match function</param>
    /// <param name="Left">Left match function</param>
    /// <returns>Sequence of mapped values</returns>
    [Pure]
    public static Iterable<Ret> match<L, R, Ret>(
        IEnumerable<Either<L, R>> list,
        Func<R, Ret> Right,
        Func<L, Ret> Left)
    {
        return Iterable(Go());
        IEnumerable<Ret> Go()
        {
            foreach (var item in list)
            {
                if (item.IsLeft) yield return Left(item.LeftValue);
                if (item.IsRight) yield return Right(item.RightValue);
            }
        }
    }

    /// <summary>
    /// Project the Either into a Lst R
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="either">Either to project</param>
    /// <returns>If the Either is in a Right state, a Lst of R with one item.  A zero length Lst R otherwise</returns>
    [Pure]
    [Obsolete(Change.UseToListInstead)]
    public static Lst<R> rightToList<L, R>(Either<L, R> either) =>
        either.RightToList();

    /// <summary>
    /// Project the Either into an ImmutableArray R
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="either">Either to project</param>
    /// <returns>If the Either is in a Right state, a ImmutableArray of R with one item.  A zero length ImmutableArray of R otherwise</returns>
    [Pure]
    [Obsolete(Change.UseToArrayInstead)]
    public static Arr<R> rightToArray<L, R>(Either<L, R> either) =>
        either.RightToArray();

    /// <summary>
    /// Project the Either into a Lst L
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="either">Either to project</param>
    /// <returns>If the Either is in a Left state, a Lst of L with one item.  A zero length Lst L otherwise</returns>
    [Pure]
    public static Lst<L> leftToList<L, R>(Either<L, R> either) =>
        either.LeftToList();

    /// <summary>
    /// Project the Either into an array of L
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="either">Either to project</param>
    /// <returns>If the Either is in a Right state, an array of L with one item.  A zero length array of L otherwise</returns>
    [Pure]
    public static Arr<L> leftToArray<L, R>(Either<L, R> either) =>
        either.LeftToArray();

    /// <summary>
    /// Extracts from a list of 'Either' all the 'Left' elements.
    /// All the 'Left' elements are extracted in order.
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="self">Either list</param>
    /// <returns>An enumerable of L</returns>
    [Pure]
    public static Iterable<L> lefts<L, R>(IEnumerable<Either<L, R>> self)
    {
        return Iterable(Go());

        IEnumerable<L> Go()
        {
            foreach (var item in self)
            {
                if (item.IsLeft) yield return item.LeftValue;
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
    public static Seq<L> lefts<L, R>(Seq<Either<L, R>> self) =>
        lefts(self.AsEnumerable()).ToSeq();

    /// <summary>
    /// Extracts from a list of 'Either' all the 'Right' elements.
    /// All the 'Right' elements are extracted in order.
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="self">Either list</param>
    /// <returns>An enumerable of L</returns>
    [Pure]
    public static Iterable<R> rights<L, R>(IEnumerable<Either<L, R>> self)
    {
        return Iterable(Go());
        IEnumerable<R> Go()
        {
            foreach (var item in self)
            {
                if (item.IsRight) yield return item.RightValue;
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
    public static Seq<R> rights<L, R>(Seq<Either<L, R>> self) =>
        rights(self.AsEnumerable()).ToSeq();

    /// <summary>
    /// Partitions a list of 'Either' into two lists.
    /// All the 'Left' elements are extracted, in order, to the first
    /// component of the output.  Similarly, the 'Right' elements are extracted
    /// to the second component of the output.
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="self">Either list</param>
    /// <returns>A tuple containing the iterables of L and of R</returns>
    [Pure]
    public static (Iterable<L> Lefts, Iterable<R> Rights) partition<L, R>(IEnumerable<Either<L, R>> self)
    {
        var ls   = new List<L>();
        var rs   = new List<R>();
        foreach (var item in self)
        {
            if (item.IsLeft) ls.Add(item.LeftValue);
            if (item.IsRight) rs.Add(item.RightValue);
        }
        return (Iterable(ls), Iterable(rs));
    }

    /// <summary>
    /// Partitions a list of 'Either' into two lists.
    /// All the 'Left' elements are extracted, in order, to the first
    /// component of the output.  Similarly, the 'Right' elements are extracted
    /// to the second component of the output.
    /// </summary>
    /// <typeparam name="L">Left</typeparam>
    /// <typeparam name="R">Right</typeparam>
    /// <param name="self">Either list</param>
    /// <returns>A tuple containing the iterables of L and R</returns>
    [Pure]
    public static (Seq<L> Lefts, Seq<R> Rights) partition<L, R>(Seq<Either<L, R>> self) => 
        self.Partition();
}
