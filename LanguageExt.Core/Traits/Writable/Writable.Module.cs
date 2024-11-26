﻿using System;

namespace LanguageExt.Traits;

public static partial class Writable
{
    /// <summary>
    /// Tell is an action that produces the writer output
    /// </summary>
    /// <param name="item">Item to tell</param>
    /// <typeparam name="M">Writer type</typeparam>
    /// <typeparam name="W">Monoid type</typeparam>
    /// <returns>Structure with the told item</returns>
    public static K<M, Unit> tell<M, W>(W item)
        where M : Writable<M, W>
        where W : Monoid<W> =>
        M.Tell(item);

    /// <summary>
    /// Writes an item and returns a value at the same time
    /// </summary>
    public static K<M, A> write<W, M, A>((A, W) item)
        where M : Writable<M, W>, Monad<M>
        where W : Monoid<W> =>
        M.Bind(M.Tell(item.Item2), _ => M.Pure(item.Item1));

    /// <summary>
    /// Writes an item and returns a value at the same time
    /// </summary>
    public static K<M, A> write<W, M, A>(A value, W item)
        where M : Writable<M, W>, Monad<M>
        where W : Monoid<W> =>
        M.Bind(M.Tell(item), _ => M.Pure(value));

    /// <summary>
    /// `pass` is an action that executes the `action`, which returns a value and a
    /// function; it then returns the value with the output having been applied to
    /// the function.
    /// </summary>
    public static K<M, A> pass<W, M, A>(K<M, (A Value, Func<W, W> Function)> action)
        where M : Writable<M, W>
        where W : Monoid<W> =>
        M.Pass(action);

    /// <summary>
    /// Writes an item and returns a value at the same time
    /// </summary>
    public static K<M, (A Value, W Output)> listen<W, M, A>(K<M, A> ma)
        where M : Writable<M, W>
        where W : Monoid<W> =>
        M.Listen(ma);

    /// <summary>
    /// `listens` executes the action `ma` and adds the result of applying `f` to the
    /// output to the value of the computation.
    /// </summary>
    public static K<M, (A Value, B Output)> listens<W, M, A, B>(Func<W, B> f, K<M, A> ma)
        where M : Writable<M, W>, Monad<M>
        where W : Monoid<W> =>
        M.Bind(M.Listen(ma), aw => M.Pure((aw.Value, f(aw.Output))));

    /// <summary>
    /// `censor` executes the action `ma` and applies the function `f` to its output,
    /// leaving the return value unchanged.
    /// </summary>
    public static K<M, A> censor<W, M, A>(Func<W, W> f, K<M, A> ma)
        where M : Writable<M, W>, Monad<M>
        where W : Monoid<W> =>
        M.Pass(M.Bind(ma, a => M.Pure((a, f))));
}
