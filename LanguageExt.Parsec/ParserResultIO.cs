using System;
using static LanguageExt.Prelude;

namespace LanguageExt.Parsec
{
    public static partial class ParserResultIO
    {
        public static ParserResult<I, O> Consumed<I, O>(Reply<I, O> reply) =>
            new(ResultTag.Consumed, reply);

        public static ParserResult<I, O> Empty<I, O>(Reply<I, O> reply) =>
            new(ResultTag.Empty, reply);

        public static ParserResult<I, O> EmptyOK<I, O>(O value, PString<I> input, ParserError? error = null) =>
            new(ResultTag.Empty, Reply.OK(value, input, error));

        public static ParserResult<I, O> EmptyError<I, O>(ParserError error, Func<I, Pos> tokenPos) =>
            new(ResultTag.Empty, Reply.Error<I, O>(error, tokenPos));

        public static ParserResult<I, O> ConsumedOK<I, O>(O value, PString<I> input) =>
            new(ResultTag.Consumed, Reply.OK(value, input));

        public static ParserResult<I, O> ConsumedOK<I, O>(O value, PString<I> input, ParserError? error) =>
            new(ResultTag.Consumed, Reply.OK(value, input, error));

        public static ParserResult<I, O> ConsumedError<I, O>(ParserError error, Func<I, Pos> tokenPos) =>
            new(ResultTag.Consumed, Reply.Error<I, O>(error, tokenPos));

    }

    public class ParserResult<I, O>
    {
        public readonly ResultTag Tag;
        public readonly Reply<I, O> Reply;

        internal ParserResult(ResultTag tag, Reply<I, O> reply)
        {
            Tag = tag;
            Reply = reply;
        }

        public ParserResult<I, O> SetEndIndex(int endIndex) =>
            new(Tag, Reply.SetEndIndex(endIndex));

        public ParserResult<I, U> Project<S, U>(S s, Func<S, O, U> project) =>
            new(Tag, Reply.Project(s, project));

        public override string ToString() =>
            Reply.Error is null
                ? "success"
                : Reply.Error.ToString();

        public bool IsFaulted =>
            Reply.IsFaulted;

        public R Match<R>(
            Func<ParserError, R> EmptyError,
            Func<ParserError, R> ConsumedError,
            Func<ParserResult<I, O>, R> Otherwise
            )
        {
            if (Tag == ResultTag.Empty && Reply.IsFaulted)
            {
                return EmptyError(Reply.Error);
            }
            if (Tag == ResultTag.Consumed && Reply.IsFaulted)
            {
                return ConsumedError(Reply.Error);
            }
            return Otherwise(this);
        }

        public R Match<R>(
            Func<ParserError, R> EmptyError,
            Func<ParserResult<I, O>, R> Otherwise
            )
        {
            if (Tag == ResultTag.Empty && Reply.IsFaulted)
            {
                return EmptyError(Reply.Error);
            }
            return Otherwise(this);
        }

        public R Match<R>(
            Func<Reply<I, O>, R> Empty,
            Func<ParserResult<I, O>, R> Otherwise
            )
        {
            if (Tag == ResultTag.Empty)
            {
                return Empty(Reply);
            }
            return Otherwise(this);
        }

        public R Match<R>(
            Func<Reply<I, O>, R> Empty,
            Func<Reply<I, O>, R> Consumed
            )
        {
            if (Tag == ResultTag.Empty)
            {
                return Empty(Reply);
            }
            return Consumed(Reply);
        }

        public R Match<R>(
            Func<O, PString<I>, ParserError?, R> ConsumedOK,
            Func<ParserError, R> ConsumedError,
            Func<O, PString<I>, ParserError?, R> EmptyOK,
            Func<ParserError, R> EmptyError
            )
        {
            if (Tag == ResultTag.Empty && !Reply.IsFaulted)
            {
                return EmptyOK(Reply.Result, Reply.State, Reply.Error);
            }
            if (Tag == ResultTag.Empty && Reply.IsFaulted)
            {
                return EmptyError(Reply.Error);
            }
            if (Tag == ResultTag.Consumed && !Reply.IsFaulted)
            {
                return ConsumedOK(Reply.Result, Reply.State, Reply.Error);
            }
            return ConsumedError(Reply.Error!);
        }

        public ParserResult<I, U> Select<U>(Func<O, U> map) =>
            new(Tag, Reply.Select(map));

        public Either<string, O> ToEither() =>
            Reply.IsFaulted
                ? Left<string, O>(ToString())
                : Right(Reply.Result);

        public Either<ERROR, O> ToEither<ERROR>(Func<string, ERROR> f) =>
            Reply.IsFaulted
                ? Left<ERROR, O>(f(ToString()))
                : Right(Reply.Result);

        public Option<O> ToOption() =>
            Reply.IsFaulted
                ? None
                : Some(Reply.Result);

    }
}
