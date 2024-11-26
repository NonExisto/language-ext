﻿using System;
using LanguageExt.Traits;

namespace LanguageExt.ClassInstances;

/// <summary>
/// String comparison
/// </summary>
public struct OrdString : Ord<string>
{
    /// <summary>
    /// Compare two values
    /// </summary>
    /// <param name="a">Left hand side of the compare operation</param>
    /// <param name="b">Right hand side of the compare operation</param>
    /// <returns>
    /// if a greater than b : 1
    /// if a less than b    : -1
    /// if a equals b       : 0
    /// </returns>
    public static int Compare(string a, string b) =>
        string.Compare(a, b, StringComparison.Ordinal);

    /// <summary>
    /// Equality test
    /// </summary>
    /// <param name="a">The left hand side of the equality operation</param>
    /// <param name="b">The right hand side of the equality operation</param>
    /// <returns>True if a and b are equal</returns>
    public static bool Equals(string a, string b) =>
        EqString.Equals(a, b);

    /// <summary>
    /// Get the hash-code of the provided value
    /// </summary>
    /// <returns>Hash code of x</returns>
    public static int GetHashCode(string x) =>
        HashableString.GetHashCode(x);
}

/// <summary>
/// String comparison (ordinal, ignore case)
/// </summary>
public struct OrdStringOrdinalIgnoreCase : Ord<string>
{
    /// <summary>
    /// Compare two values
    /// </summary>
    /// <param name="a">Left hand side of the compare operation</param>
    /// <param name="b">Right hand side of the compare operation</param>
    /// <returns>
    /// if a greater than b : 1
    /// if a less than b    : -1
    /// if a equals b       : 0
    /// </returns>
    public static int Compare(string a, string b) =>
        StringComparer.OrdinalIgnoreCase.Compare(a, b);

    /// <summary>
    /// Equality test
    /// </summary>
    /// <param name="a">The left hand side of the equality operation</param>
    /// <param name="b">The right hand side of the equality operation</param>
    /// <returns>True if a and b are equal</returns>
    public static bool Equals(string a, string b) =>
        EqStringOrdinalIgnoreCase.Equals(a, b);

    /// <summary>
    /// Get the hash-code of the provided value
    /// </summary>
    /// <returns>Hash code of x</returns>
    public static int GetHashCode(string x) =>
        HashableStringOrdinalIgnoreCase.GetHashCode(x);
}

/// <summary>
/// String comparison (ordinal)
/// </summary>
public struct OrdStringOrdinal : Ord<string>
{
    /// <summary>
    /// Compare two values
    /// </summary>
    /// <param name="a">Left hand side of the compare operation</param>
    /// <param name="b">Right hand side of the compare operation</param>
    /// <returns>
    /// if a greater than b : 1
    /// if a less than b    : -1
    /// if a equals b       : 0
    /// </returns>
    public static int Compare(string a, string b) =>
        StringComparer.Ordinal.Compare(a, b);

    /// <summary>
    /// Equality test
    /// </summary>
    /// <param name="a">The left hand side of the equality operation</param>
    /// <param name="b">The right hand side of the equality operation</param>
    /// <returns>True if a and b are equal</returns>
    public static bool Equals(string a, string b) =>
        EqStringOrdinal.Equals(a, b);

    /// <summary>
    /// Get the hash-code of the provided value
    /// </summary>
    /// <returns>Hash code of x</returns>
    public static int GetHashCode(string x) =>
        HashableStringOrdinal.GetHashCode(x);
}

/// <summary>
/// String comparison (current culture, ignore case)
/// </summary>
public struct OrdStringCurrentCultureIgnoreCase : Ord<string>
{
    /// <summary>
    /// Compare two values
    /// </summary>
    /// <param name="a">Left hand side of the compare operation</param>
    /// <param name="b">Right hand side of the compare operation</param>
    /// <returns>
    /// if a greater than b : 1
    /// if a less than b    : -1
    /// if a equals b       : 0
    /// </returns>
    public static int Compare(string a, string b) =>
        StringComparer.CurrentCultureIgnoreCase.Compare(a, b);

    /// <summary>
    /// Equality test
    /// </summary>
    /// <param name="a">The left hand side of the equality operation</param>
    /// <param name="b">The right hand side of the equality operation</param>
    /// <returns>True if a and b are equal</returns>
    public static bool Equals(string a, string b) =>
        EqStringCurrentCultureIgnoreCase.Equals(a, b);

    /// <summary>
    /// Get the hash-code of the provided value
    /// </summary>
    /// <returns>Hash code of x</returns>
    public static int GetHashCode(string x) =>
        HashableStringCurrentCultureIgnoreCase.GetHashCode(x);
}

/// <summary>
/// String comparison (current culture)
/// </summary>
public struct OrdStringCurrentCulture : Ord<string>
{
    /// <summary>
    /// Compare two values
    /// </summary>
    /// <param name="a">Left hand side of the compare operation</param>
    /// <param name="b">Right hand side of the compare operation</param>
    /// <returns>
    /// if a greater than b : 1
    /// if a less than b    : -1
    /// if a equals b       : 0
    /// </returns>
    public static int Compare(string a, string b) =>
        StringComparer.CurrentCulture.Compare(a, b);

    /// <summary>
    /// Equality test
    /// </summary>
    /// <param name="a">The left hand side of the equality operation</param>
    /// <param name="b">The right hand side of the equality operation</param>
    /// <returns>True if a and b are equal</returns>
    public static bool Equals(string a, string b) =>
        EqStringCurrentCulture.Equals(a, b);

    /// <summary>
    /// Get the hash-code of the provided value
    /// </summary>
    /// <returns>Hash code of x</returns>
    public static int GetHashCode(string x) =>
        HashableStringCurrentCulture.GetHashCode(x);
}
