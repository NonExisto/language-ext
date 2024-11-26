using System.Collections.Generic;

namespace LanguageExt.Traits;

public static partial class FallibleExtensionsE
{
    /// <summary>
    /// Partitions a foldable of effects into successes and failures,
    /// and returns only the failures.
    /// </summary>
    /// <typeparam name="F">Foldable type</typeparam>
    /// <typeparam name="M">Fallible monadic type</typeparam>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <typeparam name="E">Failure type</typeparam>
    /// <param name="fma">Foldable of fallible monadic values</param>
    /// <returns>A collection of `E` values</returns>
    public static K<M, Seq<E>> Fails<E, F, M, A>(
        this K<F, K<M, A>> fma)
        where M : Monad<M>, Fallible<E, M>
        where F : Foldable<F> =>
        fma.Fold(M.Pure(Seq.empty<E>()),
                 ma => ms => ms.Bind(
                           s => ma.Bind(_ => M.Pure(s))
                                  .Catch((E e) => M.Pure(s.Add(e)))));
    
    /// <summary>
    /// Partitions a collection of effects into successes and failures,
    /// and returns only the failures.
    /// </summary>
    /// <typeparam name="E">Failure type</typeparam>
    /// <typeparam name="M">Fallible monadic type</typeparam>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="fma">Collection of fallible monadic values</param>
    /// <returns>A collection of `E` values</returns>
    public static K<M, Seq<E>> Fails<E, M, A>(
        this Seq<K<M, A>> fma)
        where M : Monad<M>, Fallible<E, M> =>
        fma.Kind().Fails<E, Seq, M, A>();    
    
    /// <summary>
    /// Partitions a collection of effects into successes and failures,
    /// and returns only the failures.
    /// </summary>
    /// <typeparam name="E">Failure type</typeparam>
    /// <typeparam name="M">Fallible monadic type</typeparam>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="fma">Collection of fallible monadic values</param>
    /// <returns>A collection of `E` values</returns>
    public static K<M, Seq<E>> Fails<E, M, A>(
        this Iterable<K<M, A>> fma)
        where M : Monad<M>, Fallible<E, M> =>
        fma.Kind().Fails<E, Iterable, M, A>();    
    
    /// <summary>
    /// Partitions a collection of effects into successes and failures,
    /// and returns only the failures.
    /// </summary>
    /// <typeparam name="E">Failure type</typeparam>
    /// <typeparam name="M">Fallible monadic type</typeparam>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="fma">Collection of fallible monadic values</param>
    /// <returns>A collection of `E` values</returns>
    public static K<M, Seq<E>> Fails<E, M, A>(
        this Lst<K<M, A>> fma)
        where M : Monad<M>, Fallible<E, M> =>
        fma.Kind().Fails<E, Lst, M, A>();
    
    /// <summary>
    /// Partitions a collection of effects into successes and failures,
    /// and returns only the failures.
    /// </summary>
    /// <typeparam name="E">Failure type</typeparam>
    /// <typeparam name="M">Fallible monadic type</typeparam>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="fma">Collection of fallible monadic values</param>
    /// <returns>A collection of `E` values</returns>
    public static K<M, Seq<E>> Fails<E, M, A>(
        this IEnumerable<K<M, A>> fma)
        where M : Monad<M>, Fallible<E, M> =>
        Iterable.createRange(fma).Fails<E, Iterable, M, A>();
    
    /// <summary>
    /// Partitions a collection of effects into successes and failures,
    /// and returns only the failures.
    /// </summary>
    /// <typeparam name="E">Failure type</typeparam>
    /// <typeparam name="M">Fallible monadic type</typeparam>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="fma">Collection of fallible monadic values</param>
    /// <returns>A collection of `E` values</returns>
    public static K<M, Seq<E>> Fails<E, M, A>(
        this HashSet<K<M, A>> fma)
        where M : Monad<M>, Fallible<E, M> =>
        fma.Kind().Fails<E, HashSet, M, A>();
    
    /// <summary>
    /// Partitions a collection of effects into successes and failures,
    /// and returns only the failures.
    /// </summary>
    /// <typeparam name="E">Failure type</typeparam>
    /// <typeparam name="M">Fallible monadic type</typeparam>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <param name="fma">Collection of fallible monadic values</param>
    /// <returns>A collection of `E` values</returns>
    public static K<M, Seq<E>> Fails<E, M, A>(
        this Set<K<M, A>> fma)
        where M : Monad<M>, Fallible<E, M> =>
        fma.Kind().Fails<E, Set, M, A>();    
}
