﻿using static LanguageExt.Parsec.ParserResult;

namespace LanguageExt.Parsec
{
    public static class Common
    {
        public static readonly Parser<Pos> getDefPos =
            (PString inp) => ConsumedOK(inp.DefPos, inp);

        public static Parser<T> setDefPos<T>(Pos defpos, Parser<T> p) =>
            (PString inp) => p(inp.SetDefPos(defpos));

        public static bool onside(Pos pos, Pos delta) =>
            pos.Column > delta.Column || pos.Line == delta.Line;

        public static ParserError mergeError(ParserError? err1, ParserError? err2) => (err1, err2) switch{
            (null, null) => ParserError.Unknown(Pos.Zero),
            (null, _ ) => err2,
            (_, null) => err1,
            _ => string.IsNullOrEmpty(err1.Msg) ? err2 : string.IsNullOrEmpty(err2.Msg) ? err1 : Compare(err1, err2)};
        private static ParserError Compare(ParserError err1, ParserError err2) => Pos.Compare(
                      err1.Pos, err2.Pos,
                      EQ: () =>
                        err1 > err2
                            ? new ParserError(err1.Tag, err1.Pos, err1.Msg, List.append(err1.Expected, err2.Expected).ToLst())
                            : new ParserError(err2.Tag, err2.Pos, err2.Msg, List.append(err1.Expected, err2.Expected).ToLst()),
                      GT: () => err1,
                      LT: () => err2
                      );
        public static Reply<T> mergeErrorReply<T>(ParserError err, Reply<T> reply) =>
            reply.Tag == ReplyTag.OK
                ? Reply.OK(reply.Result, reply.State, mergeError(err, reply.Error))
                : Reply.Error<T>(mergeError(err, reply.Error));
    }
}
