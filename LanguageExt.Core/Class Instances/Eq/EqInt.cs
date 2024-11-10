using LanguageExt.Traits;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LanguageExt.ClassInstances;

/// <summary>
/// Integer equality
/// </summary>
public struct EqInt : Eq<int>
{
    /// <summary>
    /// Equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool Equals(int a, int b)  => 
        a == b;

    public static IEqualityComparer<int> Comparer => _.Default;

    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    public static int GetHashCode(int x) =>
        HashableInt.GetHashCode(x);

    private sealed class _ : IEqualityComparer<int>
    {
        public static readonly _ Default = new _();
        public bool Equals(int x, int y) => EqInt.Equals(x,y);
        public int GetHashCode([DisallowNull] int obj) => EqInt.GetHashCode(obj);
    }
}
