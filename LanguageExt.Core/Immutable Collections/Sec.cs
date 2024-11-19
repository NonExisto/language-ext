using System.Runtime.CompilerServices;
namespace LanguageExt;
internal readonly struct Sec
{
    public const int Mask = 31;
    public readonly int Offset;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Sec(int offset) =>
        Offset = offset;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Sec Next() =>
        new (Offset + 5);
}