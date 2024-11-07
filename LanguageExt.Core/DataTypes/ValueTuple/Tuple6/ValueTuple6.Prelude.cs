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
    public static (A, B, C, D, E, F, G) add<A, B, C, D, E, F, G>((A, B, C, D, E, F) self, G seventh) =>
        (self.Item1, self.Item2, self.Item3, self.Item4, self.Item5, self.Item6, seventh);

    /// <summary>
    /// Take the first item
    /// </summary>
    [Pure]
    public static A head<A, B, C, D, E, F>((A, B, C, D, E, F) self) =>
        self.Item1;

    /// <summary>
    /// Take the last item
    /// </summary>
    [Pure]
    public static F last<A, B, C, D, E, F>((A, B, C, D, E, F) self) =>
        self.Item6;

    /// <summary>
    /// Take the second item onwards and build a new tuple
    /// </summary>
    [Pure]
    public static (B, C, D, E, F) tail<A, B, C, D, E, F>((A, B, C, D, E, F) self) =>
        (self.Item2, self.Item3, self.Item4, self.Item5, self.Item6);

    /// <summary>
    /// One of the items matches the value passed
    /// </summary>
    [Pure]
    public static bool contains<EQ, A>((A, A, A, A, A, A) self, A value)
        where EQ : Eq<A> =>
        EQ.Equals(self.Item1, value) ||
        EQ.Equals(self.Item2, value) ||
        EQ.Equals(self.Item3, value) ||
        EQ.Equals(self.Item4, value) ||
        EQ.Equals(self.Item5, value) ||
        EQ.Equals(self.Item6, value);

    /// <summary>
    /// Map
    /// </summary>
    [Pure]
    public static R map<A, B, C, D, E, F, R>(ValueTuple<A, B, C, D, E, F> self, Func<ValueTuple<A, B, C, D, E, F>, R> map) =>
        map(self);

    /// <summary>
    /// Map
    /// </summary>
    [Pure]
    public static R map<A, B, C, D, E, F, R>(ValueTuple<A, B, C, D, E, F> self, Func<A, B, C, D, E, F, R> map) =>
        map(self.Item1, self.Item2, self.Item3, self.Item4, self.Item5, self.Item6);

    /// <summary>
    /// Tri-map to tuple
    /// </summary>
    [Pure]
    public static (U, V, W, X, Y, Z) map<A, B, C, D, E, F, U, V, W, X, Y, Z>((A, B, C, D, E, F) self, Func<A, U> firstMap, Func<B, V> secondMap, Func<C, W> thirdMap, Func<D, X> fourthMap, Func<E, Y> fifthMap, Func<F, Z> sixthMap) =>
        (firstMap(self.Item1), secondMap(self.Item2), thirdMap(self.Item3), fourthMap(self.Item4), fifthMap(self.Item5), sixthMap(self.Item6));

    /// <summary>
    /// First item-map to tuple
    /// </summary>
    [Pure]
    public static (R1, B, C, D, E, F) mapFirst<A, B, C, D, E, F, R1>((A, B, C, D, E, F) self, Func<A, R1> firstMap) =>
        (firstMap(self.Item1), self.Item2, self.Item3, self.Item4, self.Item5, self.Item6);

    /// <summary>
    /// Second item-map to tuple
    /// </summary>
    [Pure]
    public static (A, R2, C, D, E, F) mapSecond<A, B, C, D, E, F, R2>((A, B, C, D, E, F) self, Func<B, R2> secondMap) =>
        (self.Item1, secondMap(self.Item2), self.Item3, self.Item4, self.Item5, self.Item6);

    /// <summary>
    /// Third item-map to tuple
    /// </summary>
    [Pure]
    public static (A, B, R3, D, E, F) mapThird<A, B, C, D, E, F, R3>((A, B, C, D, E, F) self, Func<C, R3> thirdMap) =>
        (self.Item1, self.Item2, thirdMap(self.Item3), self.Item4, self.Item5, self.Item6);

    /// <summary>
    /// Fourth item-map to tuple
    /// </summary>
    [Pure]
    public static (A, B, C, R4, E, F) mapFourth<A, B, C, D, E, F, R4>((A, B, C, D, E, F) self, Func<D, R4> fourthMap) =>
        (self.Item1, self.Item2, self.Item3, fourthMap(self.Item4), self.Item5, self.Item6);

    /// <summary>
    /// Fifth item-map to tuple
    /// </summary>
    [Pure]
    public static (A, B, C, D, R5, F) mapFifth<A, B, C, D, E, F, R5>((A, B, C, D, E, F) self, Func<E, R5> fifthMap) =>
        (self.Item1, self.Item2, self.Item3, self.Item4, fifthMap(self.Item5), self.Item6);

    /// <summary>
    /// Sixth item-map to tuple
    /// </summary>
    [Pure]
    public static (A, B, C, D, E, R6) mapSixth<A, B, C, D, E, F, R6>((A, B, C, D, E, F) self, Func<F, R6> sixthMap) =>
        (self.Item1, self.Item2, self.Item3, self.Item4, self.Item5, sixthMap(self.Item6));

    /// <summary>
    /// Iterate
    /// </summary>
    public static Unit iter<A, B, C, D, E, F>((A, B, C, D, E, F) self, Action<A, B, C, D, E, F> func)
    {
        func(self.Item1, self.Item2, self.Item3, self.Item4, self.Item5, self.Item6);
        return default;
    }

    /// <summary>
    /// Iterate
    /// </summary>
    public static Unit iter<A, B, C, D, E, F>((A, B, C, D, E, F) self, Action<A> first, Action<B> second, Action<C> third, Action<D> fourth, Action<E> fifth, Action<F> sixth)
    {
        first(self.Item1);
        second(self.Item2);
        third(self.Item3);
        fourth(self.Item4);
        fifth(self.Item5);
        sixth(self.Item6);
        return default;
    }

    /// <summary>
    /// Fold
    /// </summary>
    [Pure]
    public static S fold<A, B, C, D, E, F, S>((A, B, C, D, E, F) self, S state, Func<S, A, B, C, D, E, F, S> fold) =>
        fold(state, self.Item1, self.Item2, self.Item3, self.Item4, self.Item5, self.Item6);

    /// <summary>
    /// Fold
    /// </summary>
    [Pure]
    public static S sextFold<A, B, C, D, E, F, S>((A, B, C, D, E, F) self, S state, Func<S, A, S> firstFold, Func<S, B, S> secondFold, Func<S, C, S> thirdFold, Func<S, D, S> fourthFold, Func<S, E, S> fifthFold, Func<S, F, S> sixthFold) =>
        sixthFold(fifthFold(fourthFold(thirdFold(secondFold(firstFold(state, self.Item1), self.Item2), self.Item3), self.Item4), self.Item5), self.Item6);

    /// <summary>
    /// Fold back
    /// </summary>
    [Pure]
    public static S sextFoldBack<A, B, C, D, E, F, S>((A, B, C, D, E, F) self, S state, Func<S, F, S> firstFold, Func<S, E, S> secondFold, Func<S, D, S> thirdFold, Func<S, C, S> fourthFold, Func<S, B, S> fifthFold, Func<S, A, S> sixthFold) =>
        sixthFold(fifthFold(fourthFold(thirdFold(secondFold(firstFold(state, self.Item6), self.Item5), self.Item4), self.Item3), self.Item2), self.Item1);
}
