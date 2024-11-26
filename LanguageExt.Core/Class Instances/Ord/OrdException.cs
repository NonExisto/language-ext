using System;
using System.Diagnostics.Contracts;
using LanguageExt.Traits;

namespace LanguageExt.ClassInstances;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public struct OrdException : Ord<Exception>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    [Pure]
    public static int GetHashCode(Exception x) =>
        HashableException.GetHashCode(x);

    [Pure]
    public static bool Equals(Exception x, Exception y) =>
        EqException.Equals(x, y);

    [Pure]
    public static int Compare(Exception x, Exception y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;
        return string.Compare(x.GetType().FullName, y.Message.GetType().FullName, StringComparison.Ordinal);
    }
}
