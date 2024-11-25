using System;
using LanguageExt.Traits;

namespace LanguageExt;

/// <summary>
/// State put
/// </summary>
/// <remarks>
/// This is a convenience type that is created by the Prelude `put` function.  It avoids
/// the need for lots of generic parameters when used in `WriterT` and `State` based
/// monads.
/// </remarks>
/// <typeparam name="W">Writer value type</typeparam>
public record Tell<W>(W Value) where W : Monoid<W>
{
    /// <summary>
    /// Convert with `Writable`
    /// </summary>
    public K<M, Unit> ToWritable<M>()
        where M : Writable<M, W> =>
        Writable.tell<M, W>(Value);

    /// <summary>
    /// Convert to a `WriterT`
    /// </summary>
    public WriterT<W, M, Unit> ToWriterT<M>()
        where M : Monad<M>, Choice<M> =>
        Writable.tell<WriterT<W, M>, W>(Value).As();
    
    /// <summary>
    /// Convert to a `WriterT`
    /// </summary>
    public Writer<W, Unit> ToWriter() =>
        Writable.tell<Writer<W>, W>(Value).As();

    /// <summary>
    /// Monadic bind with `WriterT`
    /// </summary>
    public WriterT<W, M, C> SelectMany<M, B, C>(Func<Unit, WriterT<W, M, B>> bind, Func<Unit, B, C> project)
        where M : Monad<M>, Choice<M> =>
        ToWriterT<M>().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind with `WriterT`
    /// </summary>
    public WriterT<W, M, C> SelectMany<M, B, C>(Func<Unit, K<WriterT<W, M>, B>> bind, Func<Unit, B, C> project)
        where M : Monad<M>, Choice<M> =>
        ToWriterT<M>().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind with `Writer`
    /// </summary>
    public Writer<W, C> SelectMany<B, C>(Func<Unit, Writer<W, B>> bind, Func<Unit, B, C> project) =>
        ToWriter().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind with `Writer`
    /// </summary>
    public Writer<W, C> SelectMany<B, C>(Func<Unit, K<Writer<W>, B>> bind, Func<Unit, B, C> project) =>
        ToWriter().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind with `RWST`
    /// </summary>
    public RWST<R, W, S, M, C> SelectMany<R, S, M, B, C>(Func<Unit, RWST<R, W, S, M, B>> bind, Func<Unit, B, C> project)
        where M : Writable<M, W>, Monad<M>, Choice<M> =>
        ToWritable<M>().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind with `RWST`
    /// </summary>
    public RWST<R, W, S, M, C> SelectMany<R, S, M, B, C>(Func<Unit, K<RWST<R, W, S, M>, B>> bind, Func<Unit, B, C> project)
        where M : Writable<M, W>, Monad<M>, Choice<M> =>
        ToWritable<M>().SelectMany(bind, project);
}
