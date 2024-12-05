using Xunit;
using LanguageExt.Parsec;
using static LanguageExt.Parsec.PrimIO;
using static LanguageExt.Parsec.ItemIO;
using static LanguageExt.Parsec.IndentIO;
using Char = System.Char;

namespace LanguageExt.Tests
{
    public class ParsecIOTests
    {
        class Tok
        {
            public Tok(int col, int ln, char val)
            {
                Col = col;
                Ln  = ln;
                Val = val;
            }

            public readonly int Col;
            public readonly int Ln;
            public readonly char Val;
        }

        [Fact]
        public void Indent1SuccessTest()
        {
            var toks    = Seq(new Tok(1, 1, 'a'), new Tok(2, 1, 'b'), new Tok(2, 2, 'c'), new Tok(1, 3, 'd'), new Tok(1, 4, '1'));
            var letter  = satisfy<Tok>(t => t is not null && Char.IsLetter(t.Val));
            var digit   = satisfy<Tok>(t => t is not null && Char.IsDigit(t.Val));
            var letters = many1(attempt(letter));
            var digits  = many1(attempt(digit));

            var block = from ls1 in indented1(letters)
                        from ls2 in indented1(letters)
                        from dg1 in indented1(digits)
                        from ___ in eof<Tok>()
                        select (ls1, ls2, dg1);

            var res = block.Parse(toks, t => new Pos(t.Ln - 1, t.Col - 1)).ToEither().IfLeft(() => default);

            Assert.Equal(3, res.Item1.Count);
            Assert.Single(res.Item2);
            Assert.Single(res.Item3);
        }
    }
}
