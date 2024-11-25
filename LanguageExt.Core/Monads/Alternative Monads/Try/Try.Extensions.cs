using System;
using static LanguageExt.Prelude;
using System.Diagnostics.Contracts;
using LanguageExt.Traits;

namespace LanguageExt;

/// <summary>
/// Either monad extensions
/// </summary>
public static partial class TryExtensions
{
    public static Try<A> As<A>(this K<Try, A> ma) =>
        (Try<A>)ma;

    /// <summary>
    /// Maps the bound value
    /// </summary>
    /// <param name="self">Try</param>
    /// <param name="f">Mapping transducer</param>
    /// <typeparam name="A">Source bound value type</typeparam>
    /// <typeparam name="B">Target bound value type</typeparam>
    /// <returns>`TryT`</returns>
    [Pure]
    public static Try<B> Select<A, B>(this Try<A> self, Func<A, B> f) =>
        new(() => self.runTry().Map(f));
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    //  SelectMany
    //
    [Pure]
    public static Try<B> SelectMany<A, B>(this Try<A> ma, Func<A, Try<B>> f) => 
        ma.Bind(x => f(x));

    /// <summary>
    /// Monad bind operation
    /// </summary>
    /// <param name="self">Try</param>
    /// <param name="bind">Monadic bind function</param>
    /// <param name="project">Projection function</param>
    /// <typeparam name="A">Source bound value type</typeparam>
    /// <typeparam name="B">Intermediate bound value type</typeparam>
    /// <typeparam name="C">Target bound value type</typeparam>
    /// <returns>`TryT`</returns>
    [Pure]
    public static Try<C> SelectMany<A, B, C>(this Try<A> self, Func<A, Try<B>> bind, Func<A, B, C> project) =>
        self.Bind(x => bind(x).Map(y => project(x, y)));

    /// <summary>
    /// Monad bind operation
    /// </summary>
    /// <param name="self">Try</param>
    /// <param name="bind">Monadic bind function</param>
    /// <param name="project">Projection function</param>
    /// <typeparam name="A">Source bound value type</typeparam>
    /// <typeparam name="B">Intermediate bound value type</typeparam>
    /// <typeparam name="C">Target bound value type</typeparam>
    /// <returns>`TryT`</returns>
    [Pure]
    public static Try<C> SelectMany<A, B, C>(this Try<A> self, Func<A, K<Try, B>> bind, Func<A, B, C> project) =>
        SelectMany(self, x => bind(x).As(), project);

    

    /// <summary>
    /// Monad bind operation
    /// </summary>
    /// <param name="self">Try</param>
    /// <param name="bind">Monadic bind function</param>
    /// <param name="project">Projection function</param>
    /// <typeparam name="A">Source bound value type</typeparam>
    /// <typeparam name="B">Intermediate bound value type</typeparam>
    /// <typeparam name="C">Target bound value type</typeparam>
    /// <returns>`TryT`</returns>
    [Pure]
    public static Try<C> SelectMany<A, B, C>(this Try<A> self, Func<A, Fin<B>> bind, Func<A, B, C> project) =>
        SelectMany(self, x => Try<B>.Lift(bind(x)), project);

    /// <summary>
    /// Monad bind operation
    /// </summary>
    /// <param name="self">Try</param>
    /// <param name="bind">Monadic bind function</param>
    /// <param name="project">Projection function</param>
    /// <typeparam name="A">Source bound value type</typeparam>
    /// <typeparam name="B">Intermediate bound value type</typeparam>
    /// <typeparam name="C">Target bound value type</typeparam>
    /// <returns>`TryT`</returns>
    [Pure]
    public static Try<C> SelectMany<A, B, C>(this Try<A> self, Func<A, Pure<B>> bind, Func<A, B, C> project) =>
        self.Map(x => project(x, bind(x).Value));

    /// <summary>
    /// Run the `Try`
    /// </summary>
    public static Fin<A> Run<A>(this K<Try, A> ma)
    {
        try
        {
            return ma.As().runTry();
        }
        catch (Exception e)
        {
            return Fin<A>.Fail(e);
        }
    }

    /// <summary>
    /// Monadic join
    /// </summary>
    [Pure]
    public static Try<A> Flatten<A>(this K<Try, Try<A>> mma) =>
        new(() =>
                mma.Run() switch
                {
                    Fin.Succ<Try<A>> (var succ) => succ.Run(),
                    Fin.Fail<Try<A>> (var fail) => FinFail<A>(fail),
                    _                           => throw new NotSupportedException()
                });

    /// <summary>
    /// Monadic join
    /// </summary>
    [Pure]
    public static Try<A> Flatten<A>(this K<Try, K<Try, A>> mma) =>
        new(() =>
                mma.Run() switch
                {
                    Fin.Succ<K<Try, A>> (var succ) => succ.Run(),
                    Fin.Fail<K<Try, A>> (var fail) => FinFail<A>(fail),
                    _                              => throw new NotSupportedException()
                });
}
