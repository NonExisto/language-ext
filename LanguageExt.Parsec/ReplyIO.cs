using System;
using static LanguageExt.Prelude;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace LanguageExt.Parsec
{
    public static partial class Reply
    {
        public static Reply<I, O> OK<I, O>(O result, PString<I> remaining, ParserError? error = null) =>
            new(result, remaining, error);

        public static Reply<I, O> Error<I, O>(ParserErrorTag tag, Pos pos, string message, Lst<string> expected, Func<I, Pos> tokenPos) =>
            new(new ParserError(tag, pos, message, expected, null), tokenPos);

        public static Reply<I, O> Error<I, O>(ParserError error, Func<I, Pos> tokenPos) =>
            new(error, tokenPos);
    }

    public class Reply<I, O>
    {
        public readonly ReplyTag Tag;
        public readonly O? Result;
        public readonly PString<I> State;
        public readonly ParserError? Error;
        [MemberNotNullWhen(true, nameof(Error))]
        [MemberNotNullWhen(false, nameof(Result))]
        public bool IsFaulted => Tag == ReplyTag.Error;

        internal Reply([DisallowNull]ParserError error, Func<I, Pos> tokenPos)
        {
            Debug.Assert(error is not null);

            Tag = ReplyTag.Error;
            Error = error;
            State = PString<I>.Zero(tokenPos);
        }

        internal Reply(O result, PString<I> state, ParserError? error = null)
        {
            Debug.Assert(notnull(result));

            Tag = ReplyTag.OK;
            State = state;
            Result = result;
            Error = error;
        }

        internal Reply(ReplyTag tag, O? result, PString<I> state, ParserError? error)
        {
            Tag = tag;
            Result = result;
            State = state;
            Error = error;
        }

        public Reply<I, U> Project<S, U>(S s, Func<S, O, U> project) =>
            IsFaulted
                ? Reply.Error<I, U>(Error, State.TokenPos)
                : Reply.OK(project(s, Result), State, Error);

        public Reply<I, U> Select<U>(Func<O, U> map) =>
            IsFaulted
                ? Reply.Error<I, U>(Error, State.TokenPos)
                : Reply.OK(map(Result), State, Error);

        internal Reply<I, O> SetEndIndex(int endIndex) =>
            new(Tag, Result, State.SetEndIndex(endIndex), Error);
    }
}
