﻿using LanguageExt.Traits;
using System;
using System.Diagnostics.Contracts;

namespace LanguageExt.ClassInstances;

/// <summary>
/// DateTime equality
/// </summary>
public struct EqDateTime : Eq<DateTime>
{
    /// <summary>
    /// Equality test
    /// </summary>
    /// <param name="a">The left hand side of the equality operation</param>
    /// <param name="b">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool Equals(DateTime a, DateTime b)  => 
        a == b;

    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    public static int GetHashCode(DateTime x) =>
        HashableDateTime.GetHashCode(x);
}
