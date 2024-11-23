using System;
using System.Buffers;
using System.Security.Cryptography;

namespace LanguageExt;

public static partial class Prelude
{
    static readonly int wordTop = BitConverter.IsLittleEndian ? 3 : 0;

    /// <summary>
    /// Thread-safe cryptographically strong random number generator
    /// </summary>
    /// <param name="max">Maximum value to return + 1</param>
    /// <returns>A non-negative random number, less than the value specified.</returns>
    public static int random(int max)
    {
        using var bytes = MemoryPool<byte>.Shared.Rent(sizeof(int));
        using var rnd = RandomNumberGenerator.Create();
        var span = bytes.Memory.Span;
        rnd.GetBytes(span);
        span[wordTop] &= 0x7f;
        var value = BitConverter.ToInt32(span) % max;
        return value;
    }

    /// <summary>
    /// Thread-safe cryptographically strong random base-64 string generator
    /// </summary>
    /// <param name="bytesCount">number of bytes generated that are then 
    /// <param name="options">optional options for base64 conversion</param>
    /// returned Base64 encoded</param>
    /// <returns>Base64 encoded random string</returns>
    public static string randomBase64(int bytesCount, Base64FormattingOptions options = Base64FormattingOptions.None)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bytesCount, 1, $"The minimum value for {nameof(bytesCount)} is 1");
        
        using var bytes = MemoryPool<byte>.Shared.Rent(bytesCount);
        using var rnd = RandomNumberGenerator.Create();
        var span = bytes.Memory.Span;
        rnd.GetBytes(span);
        var r = Convert.ToBase64String(span, options);
        return r;
    }
}
