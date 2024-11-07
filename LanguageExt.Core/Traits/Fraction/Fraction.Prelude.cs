using LanguageExt.Traits;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace LanguageExt;

public static partial class Trait
{
    /// <summary>
    /// Generates a fractional value from an integer ratio.
    /// </summary>
    /// <param name="x">The ratio to convert</param>
    /// <returns>The equivalent of x in the implementing type.</returns>
    [Pure]
    public static A fromRational<FRACTION, A>(Ratio<int> x) where FRACTION : Fraction<A> =>
        FRACTION.FromRational(x);

    /// <summary>
    /// Ratio constructor
    /// </summary>
    /// <typeparam name="A">Value type</typeparam>
    /// <param name="num">Numerator</param>
    /// <param name="den">Denominator</param>
    /// <returns>Ratio struct</returns>
    [Pure]
    public static Ratio<A> Ratio<A>(A num, A den)
        where A: unmanaged, ISignedNumber<A>
        => new (num, den);
}
