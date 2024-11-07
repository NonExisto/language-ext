﻿using System.Runtime.CompilerServices;

namespace LanguageExt;

public static partial class Prelude
{
    /// <summary>
    /// Unit constructor
    /// </summary>
    public static Unit unit =>
        default;

    /// <summary>
    /// Takes any value, ignores it, returns a unit
    /// </summary>
    /// <param name="anything">Value to ignore</param>
    /// <returns>Unit</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit ignore<A>(A anything) =>
        default;

    /// <summary>
    /// Takes any value, ignores it, returns a unit
    /// </summary>
    /// <param name="anything">Value to ignore</param>
    /// <returns>Unit</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit Ignore<A>(this A anything) =>
        default;
}
