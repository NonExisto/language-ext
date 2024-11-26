using System;

namespace LanguageExt.Traits;

/// <summary>
/// Functor trait
/// </summary>
/// <remarks>
/// `Map` is used to apply a function of type <see cref="Func{A,B}"/> to a value of type <see cref="K{F,A}"/>
/// where `F` is a functor, to produce a value of type `K&lt;F, B&gt;`.
///
/// Note that for any type with more than one parameter (e.g., `Either`), only the
/// last type parameter can be modified with `Map` (e.g. `R` in <see cref="Either{L,R}"/>).
/// 
/// Some types two generic parameters or more have a `Bifunctor` instance that allows both
/// the last and the penultimate parameters to be mapped over.
/// </remarks>
/// <typeparam name="F">Self referring type</typeparam>
public interface Functor<F>  
    where F : Functor<F>
{
    /// <summary>
    /// Functor map operation
    /// </summary>
    /// <remarks>
    /// Unwraps the value within the functor, passes it to the map function `f` provided, and
    /// then takes the mapped value and wraps it back up into a new functor.
    /// </remarks>
    /// <param name="f">Mapping function</param>
    /// <param name="ma">Functor to map</param>
    /// <returns>Mapped functor</returns>
    public static abstract K<F, B> Map<A, B>(Func<A, B> f, K<F, A> ma);
}
