
using System.Numerics;

namespace LanguageExt;

/// <summary>
/// A ratio between two values.
/// </summary>
/// <remarks>
/// This is used in the definition of Fractional.
/// </remarks>
public readonly struct Ratio<A> 
    where A: unmanaged, ISignedNumber<A>
{
    /// <summary>
    /// The numerator of the ratio, in non-reduced form.
    /// </summary>
    public readonly A Numerator;

    /// <summary>
    /// The denominator of the ratio, in non-reduced form.
    /// </summary>
    public readonly A Denominator;

    public Ratio(A num, A den)
    {
        Numerator = num;
        Denominator = den;
    }

    public override bool Equals(object? obj) => 
        obj is Ratio<A> ratio && Equals(ratio);

    public override int GetHashCode() => 
        FNV32.Next(Numerator.GetHashCode(), Denominator.GetHashCode());

    public static bool operator ==(Ratio<A> left, Ratio<A> right) => 
        left.Equals(right);

    public static bool operator !=(Ratio<A> left, Ratio<A> right) => 
        !(left == right);
}
