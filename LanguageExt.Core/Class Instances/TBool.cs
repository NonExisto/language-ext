using LanguageExt.Traits;
using System.Diagnostics.Contracts;

namespace LanguageExt.ClassInstances;

/// <summary>
/// Bool class instance.  Implements
/// 
///     Eq&lt;bool&gt;
///     Ord&lt;bool&gt;
///     Bool&lt;bool&gt;
/// </summary>
public struct TBool : Ord<bool>, Bool<bool>
{
    ///<inheritdoc/>
    [Pure]
    public static bool And(bool a, bool b) =>
        a && b;

    /// <summary>
    /// Compare two values
    /// </summary>
    /// <param name="x">Left hand side of the compare operation</param>
    /// <param name="y">Right hand side of the compare operation</param>
    /// <returns>
    /// if x greater than y : 1
    /// 
    /// if x less than y    : -1
    /// 
    /// if x equals y       : 0
    /// </returns>
    public static int Compare(bool x, bool y) =>
        x.CompareTo(y);

    /// <summary>
    /// Equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool Equals(bool x, bool y) =>
        x == y;

    ///<inheritdoc/>
    [Pure]
    public static bool False() =>
        false;

    /// <summary>
    /// Get the hash-code of the provided value
    /// </summary>
    /// <returns>Hash code of x</returns>
    [Pure]
    public static int GetHashCode(bool x) =>
        x.GetHashCode();

    ///<inheritdoc/>
    [Pure]
    public static bool Not(bool a) =>
        !a;

    ///<inheritdoc/>
    [Pure]
    public static bool Or(bool a, bool b) =>
        a || b;

    ///<inheritdoc/>
    [Pure]
    public static bool True() =>
        true;

    ///<inheritdoc/>
    [Pure]
    public static bool XOr(bool a, bool b) =>
        a ^ b;

    ///<inheritdoc/>
    [Pure]
    public static bool Implies(bool a, bool b) =>
        !a || b;

    ///<inheritdoc/>
    [Pure]
    public static bool BiCondition(bool a, bool b) =>
        a == b;
}
