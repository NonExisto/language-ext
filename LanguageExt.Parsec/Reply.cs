using System;
using static LanguageExt.Prelude;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace LanguageExt.Parsec
{
    public enum ReplyTag
    {
        OK,
        Error
    }

    public static partial class Reply
    {
        public static Reply<T> OK<T>(T result, PString remaining, ParserError? error = null) =>
            new(result, remaining, error);

        public static Reply<T> Error<T>(ParserErrorTag tag, Pos pos, string message, Lst<string> expected) =>
            new(new ParserError(tag, pos, message, expected, null));

        public static Reply<T> Error<T>(ParserError error) =>
            new(error);
    }

    public class Reply<T>
    {
        public readonly ReplyTag Tag;
        public readonly T? Result;
        public readonly PString State;
        public readonly ParserError? Error;

        [MemberNotNullWhen(true, nameof(Error))]
        [MemberNotNullWhen(false, nameof(Result))]
        public bool IsFaulted => Tag == ReplyTag.Error;

        internal Reply([DisallowNull]ParserError error)
        {
            Debug.Assert(error is not null);

            Tag = ReplyTag.Error;
            Error = error;
            State = PString.Zero;
        }

        internal Reply(T result, PString state, ParserError? error = null)
        {
            Debug.Assert(notnull(result));

            Tag = ReplyTag.OK;
            State = state;
            Result = result;
            Error = error;
        }

        internal Reply(ReplyTag tag, T? result, PString state, ParserError? error)
        {
            Tag = tag;
            Result = result;
            State = state;
            Error = error;
        }

        public Reply<U> Project<S, U>(S s, Func<S, T, U> project) =>
            IsFaulted
                ? Reply.Error<U>(Error)
                : Reply.OK(project(s, Result), State, Error);

        public Reply<U> Select<U>(Func<T, U> map) =>
            IsFaulted
                ? Reply.Error<U>(Error)
                : Reply.OK(map(Result), State, Error);

        internal Reply<T> SetEndIndex(int endIndex) =>
            new(Tag, Result, State.SetEndIndex(endIndex), Error);
    }
}
