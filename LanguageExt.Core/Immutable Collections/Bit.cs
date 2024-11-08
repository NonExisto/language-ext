using System.Runtime.CompilerServices;

namespace LanguageExt;

internal static class Bit
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Set(uint value, uint bit, bool flag) =>
        flag
            ? value | bit
            : value & ~bit;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Get(uint value, uint bit) =>
        (value & bit) == bit;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Get(uint data, Sec section) =>
        (int)((data & (uint)(Sec.Mask << section.Offset)) >> section.Offset);

    /// <summary>
    /// Counts the number of 1-bits in bitmap
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Count(int bits)
    {
        var c2 = bits - ((bits >> 1) & 0x55555555);
        var c4 = (c2                 & 0x33333333) + ((c2 >> 2) & 0x33333333);
        var c8 = (c4 + (c4                                >> 4)) & 0x0f0f0f0f;
        return (c8 * 0x01010101) >> 24;
    }
}
