﻿using System;
using System.Linq;
using LanguageExt.UnsafeValueAccess;
using static LanguageExt.Prelude;

namespace LanguageExt.Parsec;

public enum ParserErrorTag
{
    Unknown     = 0, // numbered because the
    SysUnexpect = 1, // order is important
    Unexpect    = 2,
    Expect      = 3,
    Message     = 4
}

public class ParserError : IEquatable<ParserError>, IComparable<ParserError>
{
    public readonly ParserErrorTag Tag;
    public readonly Pos Pos;
    public readonly string Msg;
    public readonly Lst<string> Expected;
    public readonly ParserError? Inner;

    public ParserError(ParserErrorTag tag, Pos pos, string message, Lst<string> expected, ParserError? inner = null)
    {
        Tag = tag;
        Pos = pos;
        Msg = message;
        Expected = expected;
        Inner = inner;
    }

    public static ParserError Unknown(Pos pos) =>
        new(ParserErrorTag.Unknown, pos, "", List.empty<string>(), null);

    public static ParserError SysUnexpect(Pos pos, string message) =>
        new(ParserErrorTag.SysUnexpect, pos, message, List.empty<string>(), null);

    public static ParserError Unexpect(Pos pos, string message) =>
        new(ParserErrorTag.Unexpect, pos, message, List.empty<string>(), null);

    public static ParserError Expect(Pos pos, string message, string expected) =>
        new(ParserErrorTag.Expect, pos, message, List.create(expected), null);

    public static ParserError Message(Pos pos, string message) =>
        new(ParserErrorTag.Message, pos, message, List.empty<string>(), null);

    private static string FormatExpects(Lst<string> expects) =>
        expects.Count == 0
            ? ""
            : expects.Count == 1
                ? $"expecting {expects.Head().ValueUnsafe()}"
                : $"expecting {string.Join(", ", expects.Take(expects.Count - 1))} or {expects.Last().ValueUnsafe()}";

    public override string ToString() =>
        $"error at {Pos}: {ToStringNoPosition()}";

    public void Deconstruct(out string msg)
    {
        msg = Msg;
    }

    public string ToStringNoPosition() =>
        Tag == ParserErrorTag.Unexpect ? $"unexpected {Msg}"
         : Tag == ParserErrorTag.SysUnexpect ? $"unexpected {Msg}"
         : Tag == ParserErrorTag.Message ? Msg
         : Tag == ParserErrorTag.Expect ? $"unexpected {Msg}, {FormatExpects(Expected.Filter(x => !String.IsNullOrEmpty(x)).Distinct().AsIterable().ToLst())}"
                                          : "unknown error";

    public bool Equals(ParserError? other) =>
        other is not null && Tag == other.Tag && Msg == other.Msg;

    public override bool Equals(object? obj) =>
        ((obj as ParserError)?.Equals(this)).GetValueOrDefault();

    public override int GetHashCode() =>
        (Tag,Pos,Msg).GetHashCode();

    public int CompareTo(ParserError? other) =>
        Tag.CompareTo(other?.Tag);

    public static bool operator ==(ParserError lhs, ParserError rhs) =>
        isnull(lhs) && isnull(rhs) || (!isnull(lhs) && !isnull(rhs) && !lhs.Equals(rhs));

    public static bool operator !=(ParserError lhs, ParserError rhs) =>
        !(lhs == rhs);

    public static bool operator <(ParserError lhs, ParserError rhs) =>
        lhs.CompareTo(rhs) < 0;

    public static bool operator >(ParserError lhs, ParserError rhs) =>
        lhs.CompareTo(rhs) > 0;

    public static bool operator <=(ParserError lhs, ParserError rhs) =>
        lhs.CompareTo(rhs) <= 0;

    public static bool operator >=(ParserError lhs, ParserError rhs) =>
        lhs.CompareTo(rhs) >= 0;

    public static R Compare<R>(
        ParserError lhs,
        ParserError rhs,
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
