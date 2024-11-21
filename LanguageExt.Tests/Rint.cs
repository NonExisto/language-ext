using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace LanguageExt.Tests;

/// <summary>
/// Simple wrapper around number but reverse ordering is applied
/// </summary>
/// <typeparam name="T">number type to wrap</typeparam>
/// <param name="Value">number value to wrap </param>
public readonly record struct ReverseNumber<T>(T Value) : INumber<ReverseNumber<T>> where T : INumber<T>
{
		///<inheritdoc/>
    public static ReverseNumber<T> One => new(T.One);
		///<inheritdoc/>
    public static int Radix => T.Radix;
		///<inheritdoc/>
    public static ReverseNumber<T> Zero => new(T.Zero);
		///<inheritdoc/>
    public static ReverseNumber<T> AdditiveIdentity => new(T.AdditiveIdentity);
		///<inheritdoc/>
    public static ReverseNumber<T> MultiplicativeIdentity => new(T.MultiplicativeIdentity);
		///<inheritdoc/>
    static ReverseNumber<T> INumberBase<ReverseNumber<T>>.Zero => new(T.Zero);
		///<inheritdoc/>
    public static ReverseNumber<T> Abs(ReverseNumber<T> value) => new(T.Abs(value.Value));
		///<inheritdoc/>
    public static bool IsCanonical(ReverseNumber<T> value) => T.IsCanonical(value.Value);
		///<inheritdoc/>
    public static bool IsComplexNumber(ReverseNumber<T> value) => T.IsComplexNumber(value.Value);
		///<inheritdoc/>
    public static bool IsEvenInteger(ReverseNumber<T> value) => T.IsEvenInteger(value.Value);
		///<inheritdoc/>
    public static bool IsFinite(ReverseNumber<T> value) => T.IsFinite(value.Value);
		///<inheritdoc/>
    public static bool IsImaginaryNumber(ReverseNumber<T> value) => T.IsImaginaryNumber(value.Value);
		///<inheritdoc/>
    public static bool IsInfinity(ReverseNumber<T> value) => T.IsInfinity(value.Value);
		///<inheritdoc/>
    public static bool IsInteger(ReverseNumber<T> value) => T.IsInteger(value.Value);
		///<inheritdoc/>
    public static bool IsNaN(ReverseNumber<T> value) => T.IsNaN(value.Value);
		///<inheritdoc/>
    public static bool IsNegative(ReverseNumber<T> value) => T.IsNegative(value.Value);
		///<inheritdoc/>
    public static bool IsNegativeInfinity(ReverseNumber<T> value) => T.IsNegativeInfinity(value.Value);
		///<inheritdoc/>
    public static bool IsNormal(ReverseNumber<T> value) => T.IsNormal(value.Value);
		///<inheritdoc/>
    public static bool IsOddInteger(ReverseNumber<T> value) => T.IsOddInteger(value.Value);
		///<inheritdoc/>
    public static bool IsPositive(ReverseNumber<T> value) => T.IsPositive(value.Value);
		///<inheritdoc/>
    public static bool IsPositiveInfinity(ReverseNumber<T> value) => T.IsPositiveInfinity(value.Value);
		///<inheritdoc/>
    public static bool IsRealNumber(ReverseNumber<T> value) => T.IsRealNumber(value.Value);
		///<inheritdoc/>
    public static bool IsSubnormal(ReverseNumber<T> value) => T.IsSubnormal(value.Value);
		///<inheritdoc/>
    public static bool IsZero(ReverseNumber<T> value) => T.IsZero(value.Value);
		///<inheritdoc/>
    public static ReverseNumber<T> MaxMagnitude(ReverseNumber<T> x, ReverseNumber<T> y) => new(T.MaxMagnitude(x.Value, y.Value));
		///<inheritdoc/>
    public static ReverseNumber<T> MaxMagnitudeNumber(ReverseNumber<T> x, ReverseNumber<T> y) => new(T.MaxMagnitudeNumber(x.Value, y.Value));
		///<inheritdoc/>
    public static ReverseNumber<T> MinMagnitude(ReverseNumber<T> x, ReverseNumber<T> y) => new(T.MinMagnitude(x.Value, y.Value));
		///<inheritdoc/>
    public static ReverseNumber<T> MinMagnitudeNumber(ReverseNumber<T> x, ReverseNumber<T> y) => new(T.MinMagnitudeNumber(x.Value, y.Value));
		///<inheritdoc/>
    public static ReverseNumber<T> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => new(T.Parse(s, style, provider));
		///<inheritdoc/>
    public static ReverseNumber<T> Parse(string s, NumberStyles style, IFormatProvider? provider) => new(T.Parse(s, style, provider));
		///<inheritdoc/>
    public static ReverseNumber<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => new(T.Parse(s, provider));
		///<inheritdoc/>
    public static ReverseNumber<T> Parse(string s, IFormatProvider? provider) => new(T.Parse(s, provider));
		///<inheritdoc/>
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out ReverseNumber<T> result)
		{
			if(T.TryParse(s, style, provider, out var r))
			{
				result = new(r);
				return true;
			}

			result = default;
			return false;
		}
			
		///<inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out ReverseNumber<T> result)
		{
			if(T.TryParse(s, style, provider, out var r))
			{
				result = new(r);
				return true;
			}

			result = default;
			return false;
		}
		///<inheritdoc/>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out ReverseNumber<T> result)
		{
			if(T.TryParse(s, provider, out var r))
			{
				result = new(r);
				return true;
			}

			result = default;
			return false;
		}
		///<inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ReverseNumber<T> result)
		{
			if(T.TryParse(s, provider, out var r))
			{
				result = new(r);
				return true;
			}

			result = default;
			return false;
		}
		///<inheritdoc/>
    static ReverseNumber<T> INumberBase<ReverseNumber<T>>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => 
			new(T.Parse(s, style, provider));
		///<inheritdoc/>
    static ReverseNumber<T> INumberBase<ReverseNumber<T>>.Parse(string s, NumberStyles style, IFormatProvider? provider) => 
			new(T.Parse(s, style, provider));
		///<inheritdoc/>
    static ReverseNumber<T> ISpanParsable<ReverseNumber<T>>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => 
			new(T.Parse(s, provider));
		///<inheritdoc/>
    static ReverseNumber<T> IParsable<ReverseNumber<T>>.Parse(string s, IFormatProvider? provider) => 
			new(T.Parse(s, provider));
		///<inheritdoc/>
    static bool INumberBase<ReverseNumber<T>>.TryConvertFromChecked<TOther>(TOther value, out ReverseNumber<T> result) 
		{
			if(T.TryConvertFromChecked(value, out var r)){
				result = new(r);
				return true;
			}
			result = default;
			return false;
		}
		///<inheritdoc/>
    static bool INumberBase<ReverseNumber<T>>.TryConvertFromSaturating<TOther>(TOther value, out ReverseNumber<T> result)
		{
			if(T.TryConvertFromSaturating(value, out var r)){
				result = new(r);
				return true;
			}
			result = default;
			return false;
		}
		///<inheritdoc/>
    static bool INumberBase<ReverseNumber<T>>.TryConvertFromTruncating<TOther>(TOther value, out ReverseNumber<T> result)
		{
			if(T.TryConvertFromTruncating(value, out var r)){
				result = new(r);
				return true;
			}
			result = default;
			return false;
		}
    ///<inheritdoc/>
    static bool INumberBase<ReverseNumber<T>>.TryConvertToChecked<TOther>(ReverseNumber<T> value, [MaybeNullWhen(false)]out TOther result) => 
			T.TryConvertToChecked(value.Value, out result);
    ///<inheritdoc/>
    static bool INumberBase<ReverseNumber<T>>.TryConvertToSaturating<TOther>(ReverseNumber<T> value, [MaybeNullWhen(false)]out TOther result) => 
			T.TryConvertToSaturating(value.Value, out result);
		///<inheritdoc/>
    static bool INumberBase<ReverseNumber<T>>.TryConvertToTruncating<TOther>(ReverseNumber<T> value, [MaybeNullWhen(false)]out TOther result) => 
			T.TryConvertToTruncating(value.Value, out result);
	   
		///<inheritdoc/>
    public int CompareTo(object? obj) => obj is ReverseNumber<T> r ? CompareTo(r) : 1;
		///<inheritdoc/>
    public int CompareTo(ReverseNumber<T> other) => other.Value.CompareTo(Value);
		///<inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);
		///<inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => 
			Value.TryFormat(destination, out charsWritten, format, provider);
    
		///<inheritdoc/>
    public bool Equals(ReverseNumber<T> other) => Value.Equals(other.Value);
		///<inheritdoc/>
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);
		///<inheritdoc/>
    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => 
			Value.TryFormat(destination, out charsWritten, format, provider);
		///<inheritdoc/>
    public static ReverseNumber<T> operator +(ReverseNumber<T> value) => value;
		///<inheritdoc/>
    public static ReverseNumber<T> operator +(ReverseNumber<T> left, ReverseNumber<T> right) => new(left.Value + right.Value);
		///<inheritdoc/>
    public static ReverseNumber<T> operator -(ReverseNumber<T> value) => new(-value.Value);
		///<inheritdoc/>
    public static ReverseNumber<T> operator -(ReverseNumber<T> left, ReverseNumber<T> right) => new(left.Value - right.Value);
		///<inheritdoc/>
    public static ReverseNumber<T> operator ++(ReverseNumber<T> value) => new(value.Value + T.One);
		///<inheritdoc/>
    public static ReverseNumber<T> operator --(ReverseNumber<T> value) => new(value.Value - T.One);
		///<inheritdoc/>
    public static ReverseNumber<T> operator *(ReverseNumber<T> left, ReverseNumber<T> right) => new(left.Value * right.Value);
		///<inheritdoc/>
    public static ReverseNumber<T> operator /(ReverseNumber<T> left, ReverseNumber<T> right) => new(left.Value / right.Value);
		///<inheritdoc/>
    public static ReverseNumber<T> operator %(ReverseNumber<T> left, ReverseNumber<T> right) => new(left.Value % right.Value);
		///<inheritdoc/>
    static bool IEqualityOperators<ReverseNumber<T>, ReverseNumber<T>, bool>.operator ==(ReverseNumber<T> left, ReverseNumber<T> right) => left.Value == right.Value;
		///<inheritdoc/>
    static bool IEqualityOperators<ReverseNumber<T>, ReverseNumber<T>, bool>.operator !=(ReverseNumber<T> left, ReverseNumber<T> right) => left.Value != right.Value;
		///<inheritdoc/>
    public static bool operator <(ReverseNumber<T> left, ReverseNumber<T> right) => left.Value > right.Value;
		///<inheritdoc/>
    public static bool operator >(ReverseNumber<T> left, ReverseNumber<T> right) => left.Value < right.Value;
		///<inheritdoc/>
    public static bool operator <=(ReverseNumber<T> left, ReverseNumber<T> right) => left.Value >= right.Value;
		///<inheritdoc/>
    public static bool operator >=(ReverseNumber<T> left, ReverseNumber<T> right) => left.Value <= right.Value;
    public override int GetHashCode() => -Value.GetHashCode();

    public static implicit operator ReverseNumber<T> (T number) => new (number);
}
