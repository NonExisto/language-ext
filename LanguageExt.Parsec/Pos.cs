using System;

namespace LanguageExt.Parsec
{
    /// <summary>
    /// Represents a parser source position
    /// </summary>
    public class Pos : IEquatable<Pos>, IComparable<Pos>
    {
        public readonly int Line;
        public readonly int Column;

        public Pos(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public static readonly Pos Zero = new(0, 0);

        public bool Equals(Pos? other) =>
            other is not null &&
            Line == other.Line &&
            Column == other.Column;

        public override bool Equals(object? obj) =>
            obj is Pos other && Equals(other);

        public override int GetHashCode() =>
            Tuple.Create(Line, Column).GetHashCode();

        public override string ToString() =>
            $"(line {Line + 1}, column {Column + 1})";

        public int CompareTo(Pos? other) =>
            other is null ? 1 :
            Line.CompareTo(other.Line) switch { 
                    0 => Column.CompareTo(other.Column),
                    var v => v };

        public static bool operator ==(Pos lhs, Pos rhs) =>
            lhs.Equals(rhs);

        public static bool operator !=(Pos lhs, Pos rhs) =>
            !lhs.Equals(rhs);

        public static bool operator < (Pos lhs, Pos rhs) =>
            lhs.CompareTo(rhs) < 0;

        public static bool operator >(Pos lhs, Pos rhs) =>
            lhs.CompareTo(rhs) > 0;

        public static bool operator <=(Pos lhs, Pos rhs) =>
            lhs.CompareTo(rhs) <= 0;

        public static bool operator >=(Pos lhs, Pos rhs) =>
            lhs.CompareTo(rhs) >= 0;

        public static R Compare<R>(
            Pos lhs,
            Pos rhs,
            Func<R> EQ,
            Func<R> GT,
            Func<R> LT
            )
        {
            var res = lhs.CompareTo(rhs);
            return res switch
            {
                < 0 => LT(),
                > 0 => GT(),
                _ => EQ()
            };
        }
    }
}
