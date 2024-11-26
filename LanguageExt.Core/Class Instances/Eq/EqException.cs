using System;
using System.Diagnostics.Contracts;
using LanguageExt.Traits;

namespace LanguageExt.ClassInstances;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public struct EqException : Eq<Exception>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    [Pure]
    public static int GetHashCode(Exception x) =>
        HashableException.GetHashCode(x);

    [Pure]
    public static bool Equals(Exception x, Exception y) =>
        x.GetType() == y.GetType();
}
