using System;

namespace LanguageExt.Traits;

public static class KExtensions
{
    /// <summary>
    /// Get the base kind type.  Avoids casts mid-expression
    /// </summary>
    public static K<F, A> Kind<F, A>(this K<F, A> fa) =>
        fa;
    
    public static FA SafeCast<FA, F, A>(this K<F, A> fa)
        where FA : K<F, A>
        where F : Functor<F>
    {
        try
        {
            return (FA)fa;
        }
        catch(InvalidCastException)
        {
            return (FA)F.Map(x => x, fa);
        }
    }
    
    /// <summary>
    /// `KindT` converts a nested Kind type (inherits <see cref="K{M,A}"/>), where the inner
    /// type is a concrete type and not `K&lt;N, A&gt;` to the more general version - which
    /// allows the other `T` variant methods to work seamlessly.
    /// </summary>
    /// <remarks>
    /// The casting of nested types is especially problematic for C#'s type-system, 
    /// so even though this isn't ideal, it does allow for a truly generic system
    /// of working with any nested types as long as there's a `Functor` implementation
    /// for the outer type.
    /// </remarks>
    /// <example>
    ///
    ///     var mx = Seq&lt;Option&lt;int&gt;&gt;(Some(1), Some(2), Some(3));
    ///         
    ///     var ma = mx.KindT&lt;Seq, Option, Option&lt;int&gt;, int&gt;()
    ///                .BindT(a =&gt; Some(a + 1))
    ///                .MapT(a =&gt; a + 1);
    ///                .AsT&lt;Seq, Option, Option&lt;int&gt;, int&gt;();
    ///
    /// </example>
    /// <param name="mna">Nested functor value</param>
    /// <typeparam name="M">Outer functor trait (i.e. `Seq`)</typeparam>
    /// <typeparam name="N">Inner trait (i.e. `Option`)</typeparam>
    /// <typeparam name="NA">Concrete nested type (i.e. `Option&lt;int&gt;`)</typeparam>
    /// <typeparam name="A">Concrete bound value type (i.e. `int`)</typeparam>
    /// <returns>More general version of the type that can be used with other `T` extensions, like `BindT`</returns>
    public static K<M, K<N, A>> KindT<M, N, NA, A>(this K<M, NA> mna)
        where NA : K<N, A>
        where M : Functor<M> =>
        M.Map(na => (K<N, A>)na, mna);

    /// <summary>
    /// `AsT` converts a nested Kind type (inherits <see cref="K{M,A}"/>), where the inner type
    /// is a general type (`K&lt;N, A&gt;`) to its downcast concrete version.
    /// </summary>
    /// <remarks>
    /// The casting of nested types is especially problematic for C#'s type-system, 
    /// so even though this isn't ideal, it does allow for a truly generic system
    /// of working with any nested types as long as there's a `Functor` implementation
    /// for the outer type.
    /// </remarks>
    /// <example>
    ///
    ///     var mx = Seq&lt;Option&lt;int&gt;&gt;(Some(1), Some(2), Some(3));
    ///         
    ///     var ma = mx.KindT&lt;Seq, Option, Option&lt;int&gt;, int&gt;()
    ///                .BindT(a =&gt; Some(a + 1))
    ///                .MapT(a =&gt; a + 1);
    ///                .AsT&lt;Seq, Option, Option&lt;int&gt;, int&gt;();
    ///
    /// </example>
    /// <param name="mna">Nested functor value</param>
    /// <typeparam name="M">Outer functor trait (i.e. `Seq`)</typeparam>
    /// <typeparam name="N">Inner trait (i.e. `Option`)</typeparam>
    /// <typeparam name="NA">Concrete nested type (i.e. `Option&lt;int&gt;`)</typeparam>
    /// <typeparam name="A">Concrete nested-monad bound value type (i.e. `int`)</typeparam>
    /// <returns>Concrete version of the general type.</returns>
    public static K<M, NA> AsT<M, N, NA, A>(this K<M, K<N, A>> mna)
        where NA : K<N, A>
        where M : Functor<M> =>
        M.Map(na => (NA)na, mna);    
}
