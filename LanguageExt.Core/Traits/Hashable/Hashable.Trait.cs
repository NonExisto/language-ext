﻿using System.Diagnostics.Contracts;
using LanguageExt.Attributes;

namespace LanguageExt.Traits;

/// <summary>
/// Hashable trait
/// </summary>
/// <typeparam name="A">
/// The type for which GetHashCode is defined
/// </typeparam>
[Trait("Hashable*")]
public interface Hashable<in A> : Trait
{
    /// <summary>
    /// Get the hash-code of the provided value
    /// </summary>
    /// <returns>Hash code of x</returns>
    [Pure]
    public static abstract int GetHashCode(A x);
}
