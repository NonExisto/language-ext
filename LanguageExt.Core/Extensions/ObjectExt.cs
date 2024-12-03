using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LanguageExt.ClassInstances;

namespace LanguageExt;

public static class ObjectExt
{
    /// <summary>
    /// Returns true if the value is null, and does so without
    /// boxing of any value-types.  Value-types will always
    /// return false.
    /// </summary>
    /// <example>
    ///     int x = 0;
    ///     string y = null;
    ///     
    ///     x.IsNull()  // false
    ///     y.IsNull()  // true
    /// </example>
    /// <returns>True if the value is null, and does so without
    /// boxing of any value-types.  Value-types will always
    /// return false.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNull<A>([NotNullWhen(false)]this A? value) =>
        value is null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsDefault<A>(A value) =>
        EqDefault<A>.Equals(value, default!);

    /// <summary>
    /// Allows to null check argument in fluent style in certain conditions. 
    /// This method allows to transparently check argument right before code would start using it.
    /// </summary>
    /// <typeparam name="T">Argument type, both reference and value type are supported.</typeparam>
    /// <param name="value">Argument value to null check.</param>
    /// <param name="reason">A reason for declining argument usage</param>
    /// <param name="paramName">Automatically generated argument name for exception</param>
    /// <returns>Not null value</returns>
    /// <exception cref="ArgumentNullException">Indicate a failure in null check with context information collected</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(reason))] //cheat
    public static T require<T>(this T? value, string reason, [CallerArgumentExpression(nameof(value))] string? paramName = null) => 
        value ?? throw new ArgumentNullException(paramName, reason);
}