using System;
using System.Diagnostics.Contracts;
using LanguageExt.Traits;

namespace LanguageExt;

public static partial class Prelude
{
    /// <summary>
    /// Append an extra item to the tuple
    /// </summary>
    [Pure]
    public static ValueTuple<T1, T2, T3> ad<T1, T2, T3>(ValueTuple<T1, T2> self, T3 third) =>
        self.Add(third);

    /// <summary>
    /// Take the first item
    /// </summary>
    [Pure]
    public static T1 head<T1, T2>(ValueTuple<T1, T2> self) =>
        self.Item1;

    /// <summary>
    /// Take the last item
    /// </summary>
    [Pure]
    public static T2 last<T1, T2>(ValueTuple<T1, T2> self) =>
        self.Item2;

    /// <summary>
    /// Take the second item onwards and build a new tuple
    /// </summary>
    [Pure]
    public static ValueTuple<T2> tail<T1, T2>(ValueTuple<T1, T2> self) =>
        new(self.Item2);

    /// <summary>
    /// Sum of the items
    /// </summary>
    [Pure]
    public static A sum<NUM, A>(this ValueTuple<A, A> self)
        where NUM : Num<A> =>
        NUM.Add(self.Item1, self.Item2);

    /// <summary>
    /// Product of the items
    /// </summary>
    [Pure]
    public static A product<NUM, A>(this ValueTuple<A, A> self)
        where NUM : Num<A> =>
        NUM.Multiply(self.Item1, self.Item2);

    /// <summary>
    /// One of the items matches the value passed
    /// </summary>
    [Pure]
    public static bool contains<EQ, A>(this ValueTuple<A, A> self, A value)
        where EQ : Eq<A> =>
        EQ.Equals(self.Item1, value) ||
        EQ.Equals(self.Item2, value);

    /// <summary>
    /// Map
    /// </summary>
    [Pure]
    public static R map<A, B, R>(ValueTuple<A, B> self, Func<ValueTuple<A, B>, R> map) =>
        map(self);

    /// <summary>
    /// Map
    /// </summary>
    [Pure]
    public static R map<A, B, R>(ValueTuple<A, B> self, Func<A, B, R> map) =>
        map(self.Item1, self.Item2);

    /// <summary>
    /// Bi-map to tuple
    /// </summary>
    [Pure]
    public static ValueTuple<R1, R2> bimap<T1, T2, R1, R2>(ValueTuple<T1, T2> self, Func<T1, R1> firstMap, Func<T2, R2> secondMap) =>
        self.BiMap(firstMap, secondMap);

    /// <summary>
    /// Iterate
    /// </summary>
    public static Unit iter<T1, T2>(ValueTuple<T1, T2> self, Action<T1> first, Action<T2> second) =>
        self.Iter(first, second);

    /// <summary>
    /// Iterate
    /// </summary>
    public static Unit iter<T1, T2>(ValueTuple<T1, T2> self, Action<T1, T2> func)
    {
        func(self.Item1, self.Item2);
        return default;
    }

    /// <summary>
    /// Fold
    /// </summary>
    [Pure]
    public static S fold<T1, T2, S>(ValueTuple<T1, T2> self, S state, Func<S, T1, T2, S> fold) =>
        self.Fold(state, fold);

    /// <summary>
    /// Bi-fold
    /// </summary>
    [Pure]
    public static S bifold<T1, T2, S>(ValueTuple<T1, T2> self, S state, Func<S, T1, S> firstFold, Func<S, T2, S> secondFold) =>
        self.BiFold(state, firstFold, secondFold);

    /// <summary>
    /// Bi-fold back
    /// </summary>
    [Pure]
    public static S bifoldBack<T1, T2, S>(ValueTuple<T1, T2> self, S state, Func<S, T2, S> firstFold, Func<S, T1, S> secondFold) =>
        self.BiFoldBack(state, firstFold, secondFold);
}
