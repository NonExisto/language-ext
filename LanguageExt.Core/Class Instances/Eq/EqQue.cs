using LanguageExt.Traits;
using System.Diagnostics.Contracts;

namespace LanguageExt.ClassInstances;

/// <summary>
/// Queue Equality test
/// </summary>
/// <typeparam name="EQ">Comparison type</typeparam>
/// <typeparam name="A">Element type</typeparam>
public struct EqQue<EQ, A> : Eq<Que<A>> where EQ : Eq<A>
{
    /// <summary>
    /// Equality check
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool Equals(Que<A> x, Que<A> y)
    {
        if (x.Count != y.Count) return false;

        using var enumx = x.GetEnumerator();
        using var enumy = y.GetEnumerator();
        var count = x.Count;

        for (var i = 0; i < count; i++)
        {
            enumx.MoveNext();
            enumy.MoveNext();
            if (!EQ.Equals(enumx.Current, enumy.Current)) return false;
        }
        return true;
    }


    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    public static int GetHashCode(Que<A> x) =>
        HashableQue<EQ, A>.GetHashCode(x);
}

/// <summary>
/// Queue Equality test with default comparison
/// </summary>
/// <typeparam name="A">Element type</typeparam>
public struct EqQue<A> : Eq<Que<A>>
{
    /// <summary>
    /// Equality check
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool Equals(Que<A> x, Que<A> y) =>
        EqQue<EqDefault<A>, A>.Equals(x, y);

    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    public static int GetHashCode(Que<A> x) =>
        HashableQue<A>.GetHashCode(x);
}
