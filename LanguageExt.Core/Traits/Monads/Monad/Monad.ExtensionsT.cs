﻿using System;
using LanguageExt.Traits;

namespace LanguageExt;

/// <summary>
/// Monad module
/// </summary>
public static partial class MonadExtensions
{
    /// <summary>
    /// Runs a monadic bind operation on the nested monads 
    /// </summary>
    /// <remarks>
    /// If you're working with an inner monad that is concrete then you will first need to
    /// call `KindT` to cast the monad to a more general `K` version.  This enables the
    /// `T` variant extensions (like `BindT`, `MapT, etc.) to work without providing
    /// excessive generic arguments all the way down the chain.
    /// </remarks>
    /// <example>
    ///
    ///    var mx = Seq&lt;Option&lt;int&gt;&gt;(Some(1), Some(2), Some(3));
    ///         
    ///    var ma = mx.KindT&lt;Seq, Option, Option&lt;int&gt;, int&gt;()
    ///               .BindT(a =&gt; Some(a + 1))
    ///               .MapT(a =&gt; a + 1);
    ///               .AsT&lt;Seq, Option, Option&lt;int&gt;, int&gt;();
    ///
    /// </example>
    /// <param name="mna">Nested monadic value</param>
    /// <param name="f">Bind function</param>
    /// <typeparam name="M">Outer monad trait</typeparam>
    /// <typeparam name="N">Inner monad trait</typeparam>
    /// <typeparam name="A">Input bound value</typeparam>
    /// <typeparam name="B">Output bound value</typeparam>
    /// <returns>Mapped value</returns>
    public static K<M, K<N, B>> BindT<M, N, A, B>(this K<M, K<N, A>> mna, Func<A, K<N, B>> f)
        where M : Functor<M>
        where N : Monad<N> =>
        M.Map(na => N.Bind(na, f), mna);
    
    /// <summary>
    /// Runs a monadic bind operation on the nested monads 
    /// </summary>
    /// <remarks>
    /// If you're working with an inner monad that is concrete then you will first need to
    /// call `KindT` to cast the monad to a more general `K` version.  This enables the
    /// `T` variant extensions (like `BindT`, `MapT, etc.) to work without providing
    /// excessive generic arguments all the way down the chain.
    /// </remarks>
    /// <example>
    ///
    ///    var mx = Seq&lt;Option&lt;int&gt;&gt;(Some(1), Some(2), Some(3));
    ///         
    ///    var ma = mx.KindT&lt;Seq, Option, Option&lt;int&gt;, int&gt;()
    ///               .BindT(a =&gt; Seq(Some(a + 1)))
    ///               .MapT(a =&gt; a + 1);
    ///               .AsT&lt;Seq, Option, Option&lt;int&gt;, int&gt;();
    ///
    /// </example>
    /// <param name="mna">Nested monadic value</param>
    /// <param name="f">Bind function</param>
    /// <typeparam name="M">Outer monad trait</typeparam>
    /// <typeparam name="N">Inner monad trait</typeparam>
    /// <typeparam name="A">Input bound value</typeparam>
    /// <typeparam name="B">Output bound value</typeparam>
    /// <returns>Mapped value</returns>
    public static K<M, K<N, B>> BindT<M, N, A, B>(this K<M, K<N, A>> mna, Func<A, K<M, K<N, B>>> f)
        where M : Monad<M>
        where N : Monad<N>, Traversable<N> =>
        mna.Bind(na => na.Map(f).SequenceM()).Map(nna => nna.Flatten());
}
