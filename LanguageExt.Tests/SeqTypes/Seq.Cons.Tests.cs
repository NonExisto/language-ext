using Xunit;

namespace LanguageExt.Tests
{
    public class SeqConsTests
    {
        [Fact]
        public void TestEmpty()
        {
            var arr = Seq<int>.Empty;

            var seq = toSeq(arr);

            Assert.True(seq.IsEmpty);
            Assert.True(seq.Tail.IsEmpty);
            Assert.True(seq.Tail.Tail.IsEmpty);
            Assert.True(seq.Head.IsNone);
            Assert.Empty(seq);
            Assert.Equal(0, seq.Count());

            var res1 = seq.Match(
                ()      => true,
                (x, xs) => false);

            var res2 = seq.Match(
                ()      => true,
                x       => false,
                (x, xs) => false);

            Assert.True(res1);
            Assert.True(res2);

            var skipped = seq.Skip(1);
            Assert.True(skipped.IsEmpty);
            Assert.Empty(skipped);
            Assert.Equal(0, skipped.Count());
            Assert.True(skipped.Head.IsNone);
        }

        [Fact]
        public void TestOne()
        {
            var arr = 1.Cons();

            var seq = toSeq(arr);

            Assert.Equal(1, seq.Head);
            Assert.True(seq.Tail.IsEmpty);
            Assert.True(seq.Tail.Tail.IsEmpty);

            Assert.Single(seq);
            Assert.Equal(1, seq.Count());

            var res1 = seq.Match(
                ()      => false,
                (x, xs) => x == 1 && xs.IsEmpty);

            var res2 = seq.Match(
                ()      => false,
                x       => x == 1,
                (x, xs) => false);

            Assert.True(res1);
            Assert.True(res2);

            var skipped = seq.Skip(1);
            Assert.True(skipped.IsEmpty);
            Assert.Empty(skipped);
            Assert.Equal(0, skipped.Count());
            Assert.True(skipped.Head.IsNone);
        }
        
        static int Sum(Seq<int> seq) =>
            seq.Match(
                ()      => 0,
                x       => x,
                (x, xs) => x + Sum(xs));

        [Fact]
        public void TestMore()
        {
            var arr = 1.Cons(2.Cons(3.Cons(4.Cons(5.Cons()))));

            var seq = toSeq(arr);

            Assert.Equal(1, seq.Head);
            Assert.Equal(2, seq.Tail.Head);
            Assert.Equal(3, seq.Tail.Tail.Head);
            Assert.Equal(4, seq.Tail.Tail.Tail.Head);
            Assert.Equal(5, seq.Tail.Tail.Tail.Tail.Head);

            Assert.True(seq.Tail.Tail.Tail.Tail.Tail.IsEmpty);

            Assert.Equal(5, seq.Count);
            Assert.Equal(5, seq.Count());

            Assert.Equal(4, seq.Tail.Count);
            Assert.Equal(4, seq.Tail.Count());

            Assert.Equal(3, seq.Tail.Tail.Count);
            Assert.Equal(3, seq.Tail.Tail.Count());

            Assert.Equal(2, seq.Tail.Tail.Tail.Count);
            Assert.Equal(2, seq.Tail.Tail.Tail.Count());

            Assert.Single(seq.Tail.Tail.Tail.Tail);
            Assert.Equal(1, seq.Tail.Tail.Tail.Tail.Count());

            var res = Sum(seq);

            Assert.Equal(15, res);

            var skipped1 = seq.Skip(1);
            Assert.Equal(2, skipped1.Head);
            Assert.Equal(4, skipped1.Count);
            Assert.Equal(4, skipped1.Count());

            var skipped2 = seq.Skip(2);
            Assert.Equal(3, skipped2.Head);
            Assert.Equal(3, skipped2.Count);
            Assert.Equal(3, skipped2.Count());

            var skipped3 = seq.Skip(3);
            Assert.Equal(4, skipped3.Head);
            Assert.Equal(2, skipped3.Count);
            Assert.Equal(2, skipped3.Count());

            var skipped4 = seq.Skip(4);
            Assert.Equal(5, skipped4.Head);
            Assert.Single(skipped4);
            Assert.Equal(1, skipped4.Count());

            var skipped5 = seq.Skip(5);
            Assert.True(skipped5.IsEmpty);
            Assert.Empty(skipped5);
            Assert.Equal(0, skipped5.Count());
        }

        [Fact]
        public void MapTest()
        {
            var arr = 1.Cons(2.Cons(3.Cons(4.Cons(5.Cons()))));

            var seq1 = toSeq(arr);
            var seq2 = seq1.Map(x => x * 2);
            var seq3 = seq1.Select(x => x * 2);
            var seq4 = from x in seq1
                       select x * 2;

            var expected = Seq(2, 4, 6, 8, 10);

            Assert.True(expected == seq2);
            Assert.True(expected == seq3);
            Assert.True(expected == seq4);
        }

        [Fact]
        public void FilterTest()
        {
            var arr = 1.Cons(2.Cons(3.Cons(4.Cons(5.Cons()))));

            var seq1 = toSeq(arr);
            var seq2 = seq1.Filter(x => x % 2 == 0);
            var seq3 = seq1.Where(x => x % 2 == 0);
            var seq4 = from x in seq1
                       where x % 2 == 0
                       select x;

            var expected = Seq(2, 4);

            Assert.True(expected == seq2);
            Assert.True(expected == seq3);
            Assert.True(expected == seq4);
        }

        [Fact]
        public void BindTest()
        {
            var seq1 = 10.Cons(100.Cons());
            var seq2 = 1.Cons(2.Cons(3.Cons(4.Cons(5.Cons()))));

            var seq3 = seq1.Bind(x => seq2.Map(y => x * y));

            var expected = Seq(10, 20, 30, 40, 50, 100, 200, 300, 400, 500);

            Assert.True(seq3 == expected);
        }

        [Fact]
        public void FoldTest1()
        {
            var seq = 1.Cons(2.Cons(3.Cons(4.Cons(5.Cons()))));

            var res1 = seq.Fold(1, (s, x) => s * x);
            var res2 = seq.FoldBack(1, (s, x) => s * x);

            Assert.Equal(120, res1);
            Assert.Equal(120, res2);
        }

        [Fact]
        public void FoldTest2()
        {
            var seq = "a".Cons("b".Cons("c".Cons("d".Cons("e".Cons()))));

            var res1 = seq.Fold("", (s, x) => s + x);
            var res2 = seq.FoldBack("", (s, x) => s + x);

            Assert.Equal("abcde", res1);
            Assert.Equal("edcba", res2);
        }

        [Fact]
        public void Existential()
        {
            var Seq = 'a'.Cons('b'.Cons('c'.Cons('d'.Cons('e'.Cons()))));
            var seq2 = 'a'.Cons('b'.Cons('c'.Cons('_'.Cons('e'.Cons()))));

            var ex1 = Seq.Exists(x => x == 'd');
            var ex2 = seq2.Exists(x => x == 'd');

            var fa1 = Seq.ForAll(char.IsLetter);
            var fa2 = seq2.ForAll(char.IsLetter);

            Assert.True(ex1);
            Assert.False(ex2);

            Assert.True(fa1);
            Assert.False(fa2);
        }
    }
}
