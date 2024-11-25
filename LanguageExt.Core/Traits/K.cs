namespace LanguageExt.Traits;

/// <summary>
/// Arrow kind: `* -> *` used to represent higher-kinded types.
/// </summary>
/// <remarks>
/// `K&lt;F, A&gt;` should be thought of as `F&lt;A&gt;` (where both `F` an `A` are parametric).  It currently
/// can't be represented in C#, so this allows us to define higher-kinded types and pass them
/// around.  We can then build traits that expected a `K` where the trait is tied to the `F`.
///
/// For example:
///
///     K&lt;F, A&gt; where F : Functor&lt;F&gt;
///     K&lt;M, A&gt; where M : Monad&lt;M&gt;
///
/// That means we can write generic functions that work with monads, functors, etc.
/// </remarks>
/// <typeparam name="F">Trait type</typeparam>
/// <typeparam name="A">Bound value type</typeparam>
public interface K<in F, A>;

/// <summary>
/// Arrow kind: `* -> * -> *` used to represent higher-kinded types.
/// </summary>
/// <typeparam name="F">Trait type</typeparam>
/// <typeparam name="P">Alternative value type</typeparam>
/// <typeparam name="A">Bound value type</typeparam>
public interface K<in F, P, A>;
