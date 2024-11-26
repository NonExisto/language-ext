﻿using System;
using System.Diagnostics.Contracts;
using LanguageExt.Traits;

namespace LanguageExt;
public static class ValueTuple5Extensions
{
    /// <summary>
    /// Append an extra item to the tuple
    /// </summary>
    [Pure]
    public static (A, B, C, D, E, F) Add<A, B, C, D, E, F>(this (A, B, C, D, E) self, F sixth) =>
        (self.Item1, self.Item2, self.Item3, self.Item4, self.Item5, sixth);

    /// <summary>
    /// Take the first item
    /// </summary>
    [Pure]
    public static A Head<A, B, C, D, E>(this (A, B, C, D, E) self) =>
        self.Item1;

    /// <summary>
    /// Take the last item
    /// </summary>
    [Pure]
    public static E Last<A, B, C, D, E>(this (A, B, C, D, E) self) =>
        self.Item5;

    /// <summary>
    /// Take the second item onwards and build a new tuple
    /// </summary>
    [Pure]
    public static (B, C, D, E) Tail<A, B, C, D, E>(this(A, B, C, D, E) self) =>
        (self.Item2, self.Item3, self.Item4, self.Item5);

    /// <summary>
    /// One of the items matches the value passed
    /// </summary>
    [Pure]
    public static bool Contains<EQ, A>(this (A, A, A, A, A) self, A value)
        where EQ : Eq<A> =>
        EQ.Equals(self.Item1, value) ||
        EQ.Equals(self.Item2, value) ||
        EQ.Equals(self.Item3, value) ||
        EQ.Equals(self.Item4, value) ||
        EQ.Equals(self.Item5, value);

    /// <summary>
    /// Map
    /// </summary>
    [Pure]
    public static R Map<A, B, C, D, E, R>(this ValueTuple<A, B, C, D, E> self, Func<ValueTuple<A, B, C, D, E>, R> map) =>
        map(self);

    /// <summary>
    /// Map
    /// </summary>
    [Pure]
    public static R Map<A, B, C, D, E, R>(this ValueTuple<A, B, C, D, E> self, Func<A, B, C, D, E, R> map) =>
        map(self.Item1, self.Item2, self.Item3, self.Item4, self.Item5);

    /// <summary>
    /// Tri-map to tuple
    /// </summary>
    [Pure]
    public static (V, W, X, Y, Z) Map<A, B, C, D, E, V, W, X, Y, Z>(this(A, B, C, D, E) self, Func<A, V> firstMap, Func<B, W> secondMap, Func<C, X> thirdMap, Func<D, Y> fourthMap, Func<E, Z> fifthMap) =>
        (firstMap(self.Item1), secondMap(self.Item2), thirdMap(self.Item3), fourthMap(self.Item4), fifthMap(self.Item5));

    /// <summary>
    /// First item-map to tuple
    /// </summary>
    [Pure]
    public static (R1, B, C, D, E) MapFirst<A, B, C, D, E, R1>(this (A, B, C, D, E) self, Func<A, R1> firstMap) =>
        (firstMap(self.Item1), self.Item2, self.Item3, self.Item4, self.Item5);

    /// <summary>
    /// Second item-map to tuple
    /// </summary>
    [Pure]
    public static (A, R2, C, D, E) MapSecond<A, B, C, D, E, R2>(this (A, B, C, D, E) self, Func<B, R2> secondMap) =>
        (self.Item1, secondMap(self.Item2), self.Item3, self.Item4, self.Item5);

    /// <summary>
    /// Third item-map to tuple
    /// </summary>
    [Pure]
    public static (A, B, R3, D, E) MapThird<A, B, C, D, E, R3>(this (A, B, C, D, E) self, Func<C, R3> thirdMap) =>
        (self.Item1, self.Item2, thirdMap(self.Item3), self.Item4, self.Item5);

    /// <summary>
    /// Fourth item-map to tuple
    /// </summary>
    [Pure]
    public static (A, B, C, R4, E) MapFourth<A, B, C, D, E, R4>(this(A, B, C, D, E) self, Func<D, R4> fourthMap) =>
        (self.Item1, self.Item2, self.Item3, fourthMap(self.Item4), self.Item5);

    /// <summary>
    /// Fifth item-map to tuple
    /// </summary>
    [Pure]
    public static (A, B, C, D, R5) MapFifth<A, B, C, D, E, R5>(this(A, B, C, D, E) self, Func<E, R5> fifthMap) =>
        (self.Item1, self.Item2, self.Item3, self.Item4, fifthMap(self.Item5));

    /// <summary>
    /// Map to tuple
    /// </summary>
    [Pure]
    public static (V, W, X, Y, Z) Select<A, B, C, D, E, V, W, X, Y, Z>(this (A, B, C, D, E) self, Func<(A, B, C, D, E), (V, W, X, Y, Z)> map) =>
        map(self);

    /// <summary>
    /// Iterate
    /// </summary>
    public static Unit Iter<A, B, C, D, E>(this (A, B, C, D, E) self, Action<A, B, C, D, E> func)
    {
        func(self.Item1, self.Item2, self.Item3, self.Item4, self.Item5);
        return default;
    }

    /// <summary>
    /// Iterate
    /// </summary>
    public static Unit Iter<A, B, C, D, E>(this (A, B, C, D, E) self, Action<A> first, Action<B> second, Action<C> third, Action<D> fourth, Action<E> fifth)
    {
        first(self.Item1);
        second(self.Item2);
        third(self.Item3);
        fourth(self.Item4);
        fifth(self.Item5);
        return default;
    }

    /// <summary>
    /// Fold
    /// </summary>
    [Pure]
    public static S Fold<A, B, C, D, E, S>(this (A, B, C, D, E) self, S state, Func<S, A, B, C, D, E, S> fold) =>
        fold(state, self.Item1, self.Item2, self.Item3, self.Item4, self.Item5);

    /// <summary>
    /// Fold
    /// </summary>
    [Pure]
    public static S QuintFold<A, B, C, D, E, S>(this(A, B, C, D, E) self, S state, Func<S, A, S> firstFold, Func<S, B, S> secondFold, Func<S, C, S> thirdFold, Func<S, D, S> fourthFold, Func<S, E, S> fifthFold) =>
        fifthFold(fourthFold(thirdFold(secondFold(firstFold(state, self.Item1), self.Item2), self.Item3), self.Item4), self.Item5);

    /// <summary>
    /// Fold back
    /// </summary>
    [Pure]
    public static S QuintFoldBack<A, B, C, D, E, S>(this (A, B, C, D, E) self, S state, Func<S, E, S> firstFold, Func<S, D, S> secondFold, Func<S, C, S> thirdFold, Func<S, B, S> fourthFold, Func<S, A, S> fifthFold) =>
        fifthFold(fourthFold(thirdFold(secondFold(firstFold(state, self.Item5), self.Item4), self.Item3), self.Item2), self.Item1);
}
