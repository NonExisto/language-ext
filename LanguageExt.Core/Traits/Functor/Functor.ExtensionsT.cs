using System;
using LanguageExt.Traits;

namespace LanguageExt;

public static partial class FunctorExtensions
{
    /// <summary>
    /// Runs a functor map operation on the nested functors 
    /// </summary>
    /// <remarks>
    /// If you're working with an inner functor that is concrete then you will first need to
    /// call `KindT` to cast the functor to a more general `K` version.  This enables the
    /// `T` variant extensions (like `BindT`, `MapT, etc.) to work without providing
    /// excessive generic arguments all the way down the chain.
    /// </remarks>
    /// <example>
    /// <code>
    ///    var mx = Seq&lt;Option&lt;int&gt;&gt;(Some(1), Some(2), Some(3));
    ///         
    ///    var ma = mx.KindT&lt;Seq, Option, Option&lt;int&gt;, int&gt;()
    ///               .BindT(a =&gt; Some(a + 1))
    ///               .MapT(a =&gt; a + 1);
    ///               .AsT&lt;Seq, Option, Option&lt;int&gt;, int&gt;();
    /// </code>
    /// </example>
    /// <param name="mna">Nested functor value</param>
    /// <param name="f">Bind function</param>
    /// <typeparam name="M">Outer functor trait</typeparam>
    /// <typeparam name="N">Inner functor trait</typeparam>
    /// <typeparam name="A">Input bound value</typeparam>
    /// <typeparam name="B">Output bound value</typeparam>
    /// <returns>Mapped value</returns>
    public static K<M, K<N, B>> MapT<M, N, A, B>(this K<M, K<N, A>> mna, Func<A, B> f)
        where M : Functor<M>
        where N : Functor<N> =>
        M.Map(na => N.Map(f, na), mna);
}
