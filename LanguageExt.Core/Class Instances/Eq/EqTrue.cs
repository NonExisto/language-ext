using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using LanguageExt.Traits;

namespace LanguageExt.ClassInstances;

/// <summary>
/// Always returns true for equality checks
/// </summary>
public struct EqTrue<A> : Eq<A>
{
    [Pure]
    public static bool Equals(A x, A y) =>
        true;

    internal static IEqualityComparer<A> Comparer => _.Default;

    [Pure]
    public static int GetHashCode(A x) =>
        EqDefault<A>.GetHashCode(x);

    private class _ : IEqualityComparer<A>
    {
        public static IEqualityComparer<A> Default => new _();
        public bool Equals(A? x, A? y) => true;
        public int GetHashCode([DisallowNull] A obj) => throw new System.NotSupportedException();
    }
}
