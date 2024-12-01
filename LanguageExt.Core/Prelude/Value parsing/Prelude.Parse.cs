using System;
using System.Diagnostics.Contracts;
using System.Net;
using LanguageExt.Traits;

namespace LanguageExt;

public static partial class Prelude
{
    [Pure]
    public static Option<A> convert<A>(object? value)
    {
        if (value == null)
        {
            return None;
        }

        try
        {
            var nvalue = (A)Convert.ChangeType(value, typeof(A));
            return nvalue;
        }
        catch
        {
            return None;
        }
    }

    [Pure]
    public static K<M, A> convert<M, A>(object? value)
        where M : MonoidK<M>, Applicative<M>
    {
        if (value == null)
        {
            return M.Empty<A>();
        }

        try
        {
            var nvalue = (A)Convert.ChangeType(value, typeof(A));
            return M.Pure(nvalue);
        }
        catch
        {
            return M.Empty<A>();
        }
    }

    [Pure]
    public static Option<long> parseLong(string? value, IFormatProvider? formatProvider = null) =>
        Parse<long>(value, formatProvider);

    [Pure]
    public static Option<long> parseLong(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<long>(value, formatProvider);

    [Pure]
    public static Option<int> parseInt(string? value, IFormatProvider? formatProvider = null) =>
        Parse<int>(value, formatProvider);

    [Pure]
    public static Option<int> parseInt(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<int>(value, formatProvider);

    [Pure]
    public static Option<int> parseInt(string? value, int fromBase)
    {
        try
        {
            return Convert.ToInt32(value, fromBase);
        }
        catch
        {
            return None;
        }
    }

    [Pure]
    public static K<M, int> parseInt<M>(string? value, int fromBase)
        where M : MonoidK<M>, Applicative<M>
    {
        try
        {
            return M.Pure(Convert.ToInt32(value, fromBase));
        }
        catch
        {
            return M.Empty<int>();
        }
    }

    [Pure]
    public static Option<short> parseShort(string? value, IFormatProvider? formatProvider = null) =>
        Parse<short>(value, formatProvider);

    [Pure]
    public static Option<short> parseShort(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<short>(value, formatProvider);

    [Pure]
    public static Option<char> parseChar(string? value, IFormatProvider? formatProvider = null) =>
        Parse<char>(value, formatProvider);

    [Pure]
    public static Option<char> parseChar(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<char>(value, formatProvider);

    [Pure]
    public static Option<sbyte> parseSByte(string? value, IFormatProvider? formatProvider = null) =>
        Parse<sbyte>(value, formatProvider);

    [Pure]
    public static Option<sbyte> parseSByte(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<sbyte>(value, formatProvider);

    [Pure]
    public static Option<byte> parseByte(string? value, IFormatProvider? formatProvider = null) =>
        Parse<byte>(value, formatProvider);

    [Pure]
    public static Option<byte> parseByte(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<byte>(value, formatProvider);

    [Pure]
    public static Option<ulong> parseULong(string? value, IFormatProvider? formatProvider = null) =>
        Parse<ulong>(value, formatProvider);

    [Pure]
    public static Option<ulong> parseULong(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<ulong>(value, formatProvider);

    [Pure]
    public static Option<uint> parseUInt(string? value, IFormatProvider? formatProvider = null) =>
        Parse<uint>(value, formatProvider);

    [Pure]
    public static Option<uint> parseUInt(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<uint>(value, formatProvider);

    [Pure]
    public static Option<ushort> parseUShort(string? value, IFormatProvider? formatProvider = null) =>
        Parse<ushort>(value, formatProvider);

    [Pure]
    public static Option<ushort> parseUShort(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<ushort>(value, formatProvider);

    [Pure]
    public static Option<float> parseFloat(string? value, IFormatProvider? formatProvider = null) =>
        Parse<float>(value, formatProvider);

    [Pure]
    public static Option<float> parseFloat(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<float>(value, formatProvider);

    [Pure]
    public static Option<double> parseDouble(string? value, IFormatProvider? formatProvider = null) =>
        Parse<double>(value, formatProvider);

    [Pure]
    public static Option<double> parseDouble(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<double>(value, formatProvider);

    [Pure]
    public static Option<decimal> parseDecimal(string? value, IFormatProvider? formatProvider = null) =>
        Parse<decimal>(value, formatProvider);

    [Pure]
    public static Option<decimal> parseDecimal(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<decimal>(value, formatProvider);

    [Pure]
    public static Option<bool> parseBool(string? value, IFormatProvider? formatProvider = null) =>
        Parse<bool>(value, formatProvider);

    [Pure]
    public static Option<bool> parseBool(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<bool>(value, formatProvider);

    [Pure]
    public static Option<Guid> parseGuid(string? value, IFormatProvider? formatProvider = null) =>
        Parse<Guid>(value, formatProvider);

    [Pure]
    public static Option<Guid> parseGuid(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<Guid>(value, formatProvider);

    [Pure]
    public static Option<DateTime> parseDateTime(string? value, IFormatProvider? formatProvider = null) =>
        Parse<DateTime>(value, formatProvider);

    [Pure]
    public static Option<DateTime> parseDateTime(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<DateTime>(value, formatProvider);

    [Pure]
    public static Option<DateTimeOffset> parseDateTimeOffset(string? value, IFormatProvider? formatProvider = null) =>
        Parse<DateTimeOffset>(value, formatProvider);
    
    [Pure]
    public static Option<DateTimeOffset> parseDateTimeOffset(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<DateTimeOffset>(value, formatProvider);

    [Pure]
    public static Option<TimeSpan> parseTimeSpan(string? value, IFormatProvider? formatProvider = null) =>
        Parse<TimeSpan>(value, formatProvider);

    [Pure]
    public static Option<TimeSpan> parseTimeSpan(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<TimeSpan>(value, formatProvider);
        
    [Pure]
    public static Option<TEnum> parseEnum<TEnum>(string? value)
        where TEnum : struct =>
        Enum.TryParse<TEnum>(value, false, out var enumValue) ? Some(enumValue) : None;

    [Pure]
    public static Option<TEnum> parseEnumIgnoreCase<TEnum>(string? value)
        where TEnum : struct =>
        Enum.TryParse<TEnum>(value, true, out var enumValue) ? Some(enumValue) : None;

    [Pure]
    public static Option<IPAddress> parseIPAddress(string? value, IFormatProvider? formatProvider = null) =>
        Parse<IPAddress>(value, formatProvider);

    [Pure]
    public static Option<IPAddress> parseIPAddress(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null) =>
        Parse<IPAddress>(value, formatProvider);

    static Option<A> Parse<A>(ReadOnlySpan<char> value, IFormatProvider? formatProvider) where A: ISpanParsable<A> =>
        A.TryParse(value, formatProvider, out var result) ? Some(result) : None;

    static Option<A> Parse<A>(string? value, IFormatProvider? formatProvider) where A: ISpanParsable<A> =>
        A.TryParse(value, formatProvider, out var result) ? Some(result) : None;

    
}
