﻿#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt.ClassInstances;
using LanguageExt.Common;
using LanguageExt.TypeClasses;

namespace LanguageExt.Transducers;

public static partial class Transducer
{
    /// <summary>
    /// Lift a value into the `Transducer` space
    /// </summary>
    /// <param name="value">Value to lift</param>
    /// <typeparam name="A">Value type</typeparam>
    /// <returns>`Transducer` from `Unit` to `A`</returns>
    public static Transducer<Unit, A> Pure<A>(A value) =>
        constant<Unit, A>(value);

    /// <summary>
    /// Fail transducer
    /// </summary>
    /// <remarks>
    /// Consider this the `throw` of transducers.  It is the nuclear option to get out of a reduce.
    /// </remarks>
    /// <param name="error">Error to raise</param>
    /// <returns>A transducer that always fails</returns>
    public static Transducer<A, B> Fail<A, B>(Error error) =>
        new FailTransducer<A, B>(error);
    
    /// <summary>
    /// Identity transducer
    /// </summary>
    public static Transducer<A, A> identity<A>() =>
        IdentityTransducer<A>.Default;

    /// <summary>
    /// Constant transducer
    /// </summary>
    /// <remarks>
    /// Takes any value, ignores it and yields the value provided.
    /// </remarks>
    /// <param name="value">Constant value to yield</param>
    /// <typeparam name="A">Input value type</typeparam>
    /// <typeparam name="B">Constant value type</typeparam>
    /// <returns>`Transducer` from `A` to `B`</returns>
    public static Transducer<A, B> constant<A, B>(B value) =>
        new ConstantTransducer<A, B>(value);
    
    public static Transducer<A, B> lift<A, B>(Func<A, TResult<B>> f) =>
        new LiftTransducer1<A, B>(f);
    
    public static Transducer<Unit, A> lift<A>(Func<TResult<A>> f) =>
        new LiftTransducer2<A>(f);
    
    public static Transducer<A, B> lift<A, B>(Func<A, B> f) =>
        new LiftTransducer3<A, B>(f);
    
    public static Transducer<Unit, A> lift<A>(Func<A> f) =>
        new LiftTransducer4<A>(f);
    
    public static Transducer<A, B> liftIO<A, B>(Func<CancellationToken, A, Task<B>> f) =>
        new LiftIOTransducer3<A, B>(f);
    
    public static Transducer<A, B> liftIO<A, B>(Func<CancellationToken, A, Task<TResult<B>>> f) =>
        new LiftIOTransducer1<A, B>(f);
    
    public static Transducer<Unit, A> liftIO<A>(Func<CancellationToken, Task<A>> f) =>
        new LiftIOTransducer4<A>(f);
    
    public static Transducer<Unit, A> liftIO<A>(Func<CancellationToken, Task<TResult<A>>> f) =>
        new LiftIOTransducer2<A>(f);
    
    /// <summary>
    /// Transducer composition.  The output of one transducer is fed as the input to the next.
    ///
    /// Resulting im a single transducer that captures the composition
    /// </summary>
    /// <returns>Transducer that captures the composition</returns>
    public static Transducer<A, C> compose<A, B, C>(
        Transducer<A, B> f, 
        Transducer<B, C> g) =>
        new ComposeTransducer<A, B, C>(f, g);
    
    /// <summary>
    /// Transducer composition.  The output of one transducer is fed as the input to the next.
    ///
    /// Resulting im a single transducer that captures the composition
    /// </summary>
    /// <returns>Transducer that captures the composition</returns>
    public static Transducer<A, D> compose<A, B, C, D>(
        Transducer<A, B> f, 
        Transducer<B, C> g, 
        Transducer<C, D> h) =>
        new ComposeTransducer<A, B, C, D>(f, g, h);
    
    /// <summary>
    /// Transducer composition.  The output of one transducer is fed as the input to the next.
    ///
    /// Resulting im a single transducer that captures the composition
    /// </summary>
    /// <returns>Transducer that captures the composition</returns>
    public static Transducer<A, E> compose<A, B, C, D, E>(
        Transducer<A, B> f, 
        Transducer<B, C> g, 
        Transducer<C, D> h,
        Transducer<D, E> i) =>
        new ComposeTransducer<A, B, C, D, E>(f, g, h, i);
    
    /// <summary>
    /// Transducer composition.  The output of one transducer is fed as the input to the next.
    ///
    /// Resulting im a single transducer that captures the composition
    /// </summary>
    /// <returns>Transducer that captures the composition</returns>
    public static Transducer<A, F> compose<A, B, C, D, E, F>(
        Transducer<A, B> f, 
        Transducer<B, C> g, 
        Transducer<C, D> h,
        Transducer<D, E> i,
        Transducer<E, F> j) =>
        new ComposeTransducer<A, B, C, D, E, F>(f, g, h, i, j);
    
    /// <summary>
    /// Transducer composition.  The output of one transducer is fed as the input to the next.
    ///
    /// Resulting im a single transducer that captures the composition
    /// </summary>
    /// <returns>Transducer that captures the composition</returns>
    public static Transducer<A, G> compose<A, B, C, D, E, F, G>(
        Transducer<A, B> f, 
        Transducer<B, C> g, 
        Transducer<C, D> h,
        Transducer<D, E> i,
        Transducer<E, F> j,
        Transducer<F, G> k) =>
        new ComposeTransducer<A, B, C, D, E, F, G>(f, g, h, i, j, k);

    /// <summary>
    /// Take nested transducers and flatten them
    /// </summary>
    /// <param name="ff">Nested transducers</param>
    /// <returns>Flattened transducers</returns>
    public static Transducer<A, B> flatten<A, B>(Transducer<A, Transducer<A, B>> ff) =>
        new FlattenTransducer1<A, B>(ff);

    /// <summary>
    /// Take nested transducers and flatten them
    /// </summary>
    /// <param name="ff">Nested transducers</param>
    /// <returns>Flattened transducers</returns>
    public static Transducer<A, B> flatten<A, B>(Transducer<A, Transducer<Unit, B>> ff) =>
        new FlattenTransducer2<A, B>(ff);
    
    /// <summary>
    /// Take nested transducers and flatten them
    /// </summary>
    /// <param name="ff">Nested transducers</param>
    /// <returns>Flattened transducers</returns>
    public static Transducer<Env, Sum<X, A>> flatten<Env, X, Y, A>(Transducer<Env, Sum<X, Transducer<Env, Sum<X, A>>>> ff) =>
        new FlattenSumTransducer1<Env, X, A>(ff);

    /*
    /// <summary>
    /// Take nested transducers and flatten them
    /// </summary>
    /// <param name="ff">Nested transducers</param>
    /// <returns>Flattened transducers</returns>
    public static Transducer<A, Sum<Y, B>> flatten<X, Y, A, B>(Transducer<A, Transducer<Sum<X, A>, Sum<Y, B>>> ff) =>
        new FlattenSumTransducer5<X, Y, A, B>(ff);

    /// <summary>
    /// Take nested transducers and flatten them
    /// </summary>
    /// <param name="ff">Nested transducers</param>
    /// <returns>Flattened transducers</returns>
    public static Transducer<A, Sum<Y, B>> flatten<X, Y, A, B>(Transducer<A, Transducer<Sum<X, Unit>, Sum<Y, B>>> ff) =>
        new FlattenSumTransducer6<X, Y, A, B>(ff);

    /// <summary>
    /// Take nested transducers and flatten them
    /// </summary>
    /// <param name="ff">Nested transducers</param>
    /// <returns>Flattened transducers</returns>
    public static Transducer<Sum<X, A>, Sum<Y, B>> flatten<X, Y, A, B>(Transducer<Sum<X, A>, Sum<Y, Transducer<A, B>>> ff) =>
        new FlattenSumTransducer7<X, Y, A, B>(ff);

    /// <summary>
    /// Take nested transducers and flatten them
    /// </summary>
    /// <param name="ff">Nested transducers</param>
    /// <returns>Flattened transducers</returns>
    public static Transducer<Sum<X, A>, Sum<Y, B>> flatten<X, Y, A, B>(Transducer<Sum<X, A>, Sum<Y, Transducer<Unit, B>>> ff) =>
        new FlattenSumTransducer8<X, Y, A, B>(ff);
        */

    /// <summary>
    /// Applicative apply
    /// </summary>
    /// <remarks>
    /// Gets a lifted function and a lifted argument, invokes the function with the argument and re-lifts the result.
    /// </remarks>
    /// <returns>Result of applying the function to the argument</returns>
    public static Transducer<A, C> apply<A, B, C>(
        Transducer<A, Func<B, C>> ff,
        Transducer<A, B> fa) =>
        new ApplyTransducer<A, B, C>(ff, fa);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public static Transducer<A, C> bind<A, B, C>(
        Transducer<A, B> m,
        Transducer<B, Transducer<A, C>> f) =>
        new BindTransducer1<A, B, C>(m, f);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public static Transducer<A, C> bind<A, B, C>(
        Transducer<A, B> m,
        Transducer<B, Func<A, C>> f) =>
        new BindTransducer2<A, B, C>(m, f);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public static Transducer<A, C> bind<A, B, C>(
        Transducer<A, B> m,
        Func<B, Transducer<A, C>> f) =>
        new BindTransducer3<A, B, C>(m, f);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public static Transducer<A, Sum<X, C>> bind<X, A, B, C>(
        Transducer<A, Sum<X, B>> m,
        Func<B, Transducer<A, Sum<X, C>>> f) =>
        new BindTransducerSum<X, A, B, C>(m, lift(f));

    /// <summary>
    /// Monadic bind
    /// </summary>
    public static Transducer<A, Sum<X, C>> bind<X, A, B, C>(
        Transducer<A, Sum<X, B>> m,
        Transducer<B, Transducer<A, Sum<X, C>>> f) =>
        new BindTransducerSum<X, A, B, C>(m, f);

    /// <summary>
    /// Lifts a unit accepting transducer, ignores the input value.
    /// </summary>
    public static Transducer<A, B> ignore<A, B>(Transducer<Unit, B> m) =>
        new IgnoreTransducer<A, B>(m);

    /// <summary>
    /// Functor bi-map
    /// </summary>
    /// <param name="First">First transducer to run</param>
    /// <param name="Left">Left mapping transducer</param>
    /// <param name="Right">Right mapping transducer</param>
    /// <returns>Composition of first, left, and right transducers</returns>
    public static Transducer<E, Sum<Y, B>> bimap<E, X, Y, A, B>(
        Transducer<E, Sum<X, A>> First,
        Transducer<X, Y> Left,
        Transducer<A, B> Right) =>
        new BiMap<E, X, Y, A, B>(First, Left, Right);

    /// <summary>
    /// Functor bi-map
    /// </summary>
    /// <param name="First">First transducer to run</param>
    /// <param name="Left">Left mapping transducer</param>
    /// <param name="Right">Right mapping transducer</param>
    /// <returns>Composition of first, left, and right transducers</returns>
    public static Transducer<Sum<X, A>, Sum<Z, C>> bimap<X, Y, Z, A, B, C>(
        Transducer<Sum<X, A>, Sum<Y, B>> First,
        Transducer<Y, Z> Left,
        Transducer<B, C> Right) =>
        new BiMap2<X, Y, Z, A, B, C>(First, Left, Right);

    /// <summary>
    /// Functor bi-map map left
    /// </summary>
    /// <param name="First">First transducer to run</param>
    /// <param name="Left">Left mapping transducer</param>
    /// <returns>Composition of first and left transducers</returns>
    public static Transducer<E, Sum<Y, A>> mapLeft<E, X, Y, A>(
        Transducer<E, Sum<X, A>> First,
        Transducer<X, Y> Left) =>
        new MapLeft<E, X, Y, A>(First, Left);

    /// <summary>
    /// Functor bi-map map left
    /// </summary>
    /// <param name="First">First transducer to run</param>
    /// <param name="Left">Left mapping transducer</param>
    /// <returns>Composition of first and left</returns>
    public static Transducer<Sum<X, A>, Sum<Z, B>> mapLeft<X, Y, Z, A, B>(
        Transducer<Sum<X, A>, Sum<Y, B>> First,
        Transducer<Y, Z> Left) =>
        new MapLeft2<X, Y, Z, A, B>(First, Left);

    /// <summary>
    /// Functor bi-map map right
    /// </summary>
    /// <param name="First">First transducer to run</param>
    /// <param name="Right">Right mapping transducer</param>
    /// <returns>Composition of first and right transducers</returns>
    public static Transducer<E, Sum<X, B>> mapRight<E, X, A, B>(
        Transducer<E, Sum<X, A>> First,
        Transducer<A, B> Right) =>
        new MapRight<E, X, A, B>(First, Right);

    /// <summary>
    /// Functor bi-map map right
    /// </summary>
    /// <param name="First">First transducer to run</param>
    /// <param name="Right">Right mapping transducer</param>
    /// <returns>Composition of first and right transducers</returns>
    public static Transducer<Sum<X, A>, Sum<Y, C>> mapRight<X, Y, A, B, C>(
        Transducer<Sum<X, A>, Sum<Y, B>> First,
        Transducer<B, C> Right) =>
        new MapRight2<X, Y, A, B, C>(First, Right);

    /// <summary>
    /// Choice transducer
    /// </summary>
    /// <remarks>
    /// Tries a sequence of transducers until one succeeds (results in a `Sum.Right`).  If the sequence
    /// is exhausted then the transducer completes.
    /// </remarks>
    /// <param name="transducers">Sequence of transducers</param>
    /// <returns>Transducer that encapsulates the choice</returns>
    public static Transducer<A, Sum<X, B>> choice<X, A, B>(Seq<Transducer<A, Sum<X, B>>> transducers) =>
        new ChoiceTransducer<X, A, B>(transducers);

    /// <summary>
    /// Choice transducer
    /// </summary>
    /// <remarks>
    /// Tries a sequence of transducers until one succeeds (results in a `Sum.Right`).  If the sequence
    /// is exhausted then the transducer completes.
    /// </remarks>
    /// <param name="transducers">Sequence of transducers</param>
    /// <returns>Transducer that encapsulates the choice</returns>
    public static Transducer<A, Sum<X, B>> choice<X, A, B>(params Transducer<A, Sum<X, B>>[] transducers) =>
        new ChoiceTransducer<X, A, B>(transducers.ToSeq());

    /// <summary>
    /// Caches the result of the transducer computation for each value flowing through.
    /// </summary>
    /// <remarks>
    /// This works within the context of a single `Invoke` operation, so it only makes
    /// sense if you're using the transducer as part of a stream process.  This allows
    /// each item coming through to have its result cached. So you never repeat the
    /// process for each `A` value.
    /// </remarks>
    /// <param name="transducer"></param>
    /// <typeparam name="EqA"></typeparam>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <returns></returns>
    public static Transducer<A, B> memoStream<A, B>(Transducer<A, B> transducer) =>
        memoStream<EqDefault<A>, A, B>(transducer);

    /// <summary>
    /// Caches the result of the transducer computation for each value flowing through.
    /// </summary>
    /// <remarks>
    /// This works within the context of a single `Invoke` operation, so it only makes
    /// sense if you're using the transducer as part of a stream process.  This allows
    /// each item coming through to have its result cached. So you never repeat the
    /// process for each `A` value.
    /// </remarks>
    /// <param name="transducer"></param>
    /// <typeparam name="EqA"></typeparam>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <returns></returns>
    public static Transducer<A, B> memoStream<EqA, A, B>(Transducer<A, B> transducer)
        where EqA : struct, Eq<A> =>
        new MemoTransducer<EqA, A, B>(transducer);

    /// <summary>
    /// Caches the result of the transducer computation for each value flowing through.
    /// </summary>
    /// <remarks>
    /// Unlike `MemoStream` -  which only caches values for the duration of the the call
    /// to `Invoke` - this caches values for the duration of the life of the `Transducer`
    /// instance.  
    /// </remarks>
    /// <remarks>
    /// NOTE: Transducers use both a _value_ and a _state_ as inputs to its transformation
    /// and reduce process.  For memoisation the state is ignored.  In a non-memoisation
    /// scenario a different state and value pair could produce different results; and
    /// so this should be considered when deciding to apply a memo to a transducer: it
    /// only checks _value_ equality.
    /// </remarks>
    /// <param name="transducer">Transducer to memoise</param>
    /// <returns>Memoised transducer</returns>
    public static Transducer<A, B> memo<A, B>(Transducer<A, B> transducer) =>
        memo<EqDefault<A>, A, B>(transducer);

    /// <summary>
    /// Caches the result of the transducer computation for each value flowing through.
    /// </summary>
    /// <remarks>
    /// Unlike `MemoStream` -  which only caches values for the duration of the the call
    /// to `Invoke` - this caches values for the duration of the life of the `Transducer`
    /// instance.  
    /// </remarks>
    /// <remarks>
    /// NOTE: Transducers use both a _value_ and a _state_ as inputs to its transformation
    /// and reduce process.  For memoisation the state is ignored.  In a non-memoisation
    /// scenario a different state and value pair could produce different results; and
    /// so this should be considered when deciding to apply a memo to a transducer: it
    /// only checks _value_ equality.
    /// </remarks>
    /// <param name="transducer">Transducer to memoise</param>
    /// <returns>Memoised transducer</returns>
    public static Transducer<A, B> memo<EqA, A, B>(Transducer<A, B> transducer)
        where EqA : struct, Eq<A> =>
        new Memo1Transducer<EqA, A, B>(transducer);    
}
