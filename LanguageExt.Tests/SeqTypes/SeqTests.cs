using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static LanguageExt.Seq;

namespace LanguageExt.Tests;

public class SeqTests
{
    [Fact]
    public void ObjectExists()
    {
        var x = "test";

        Assert.Equal(1, Seq(x).Count());
        Assert.True(Seq(x).Head()  == x);
    }

    [Fact]
    public void ObjectNull()
    {
        string? x = null;

        Assert.Equal(0, toSeq(x).Count());
    }

    [Fact]
    public void EnumerableExists()
    {
        var x = new[] { "a", "b", "c" };

        var y = toSeq(x);

        var res = y.Tail().AsEnumerable().ToList();
        res.Count.Should().Be(2);


        Assert.Equal(3, y.Count);
        Assert.Equal("a", y.Head);
        Assert.Equal("b", y.Tail.Head);
        Assert.Equal("c", y.Tail.Tail.Head);
    }

    [Fact]
    public void EnumerableNull()
    {
        string[]? x = null;

        Assert.Equal(0, toSeq(x).Count);
    }

    [Fact]
    public void TakeTest()
    {
        static IEnumerable<int> Numbers()
        {
            for (int i = 0; i < 100; i++)
            {
                yield return i;
            }
        }

        var seq = Numbers().AsIterable().ToSeq();

        var a = seq.Take(5).Sum();
        Assert.Equal(10, a);

        var b = seq.Skip(5).Take(5).Sum();
        Assert.Equal(35, b);

    }

    [Fact]
    public void FoldTest()
    {
        var input   = Seq(1, 2, 3, 4, 5);
        var output1 = Foldable.fold((s, x) => s + x, "", input);
        Assert.Equal("12345", output1);
    }

    [Fact]
    public void FoldBackTest()
    {
        var input   = Seq(1, 2, 3, 4, 5);
        var output1 = Foldable.foldBack((s, x) => s + x, "", input);
        Assert.Equal("54321", output1);
    }

    [Fact]
    public void FoldWhileTest()
    {
        var input = Seq(10, 20, 30, 40, 50);

        var output1 = Foldable.foldWhile((s, x) => s + x, x => x.Value < 40, "", input);
        Assert.Equal("102030", output1);

        var output2 = Foldable.foldWhile((s, x) => s + x, s => s.State.Length < 6, "", input);
        Assert.Equal("102030", output2);

        var output3 = Foldable.foldWhile((s, x) => s + x, x => x.Value < 40, 0, input);
        Assert.Equal(60, output3);

        var output4 = Foldable.foldWhile((s, x) => s + x, s => s.State < 60, 0, input);
        Assert.Equal(60, output4);
    }

    [Fact]
    public void FoldBackWhileTest()
    {
        var input = Seq(10, 20, 30, 40, 50);

        var output1 = Foldable.foldBackWhile((s, x) => s + x, x => x.Value >= 40, "", input);
        Assert.Equal("5040", output1);

        var output2 = Foldable.foldBackWhile((s, x) => s + x, s => s.State.Length < 4, "", input);
        Assert.Equal("5040", output2);

        var output3 = Foldable.foldBackWhile((s, x) => s + x, x => x.Value >= 40, 0, input);
        Assert.Equal(90, output3);

        var output4 = Foldable.foldBackWhile((s, x) => s + x, s => s.State < 90, 0, input);
        Assert.Equal(90, output4);
    }

    [Fact]
    public void FoldUntilTest()
    {
        var input = Seq(10, 20, 30, 40, 50);

        var output1 = Foldable.foldUntil((s, x) => s + x, x => x.Value >= 40, "", input);
        Assert.Equal("102030", output1);

        var output2 = Foldable.foldUntil((s, x) => s + x, s => s.State.Length >= 6, "", input);
        Assert.Equal("102030", output2);

        var output3 = Foldable.foldUntil((s, x) => s + x, x => x.Value >= 40, 0, input);
        Assert.Equal(60, output3);

        var output4 = Foldable.foldUntil((s, x) => s + x, s => s.State >= 60, 0, input);
        Assert.Equal(60, output4);
    }

    [Fact]
    public void FoldBackUntilTest()
    {
        var input = Seq(10, 20, 30, 40, 50);

        var output1 = Foldable.foldBackUntil((s, x) => s + x, x => x.Value < 40, "", input);
        Assert.Equal("5040", output1);

        var output2 = Foldable.foldBackUntil((s, x) => s + x, s => s.State.Length >= 4, "", input);
        Assert.Equal("5040", output2);

        var output3 = Foldable.foldBackUntil((s, x) => s + x, x => x.Value < 40, 0, input);
        Assert.Equal(90, output3);

        var output4 = Foldable.foldBackUntil((s, x) => s + x, s => s.State >= 90, 0, input);
        Assert.Equal(90, output4);
    }

    [Fact]
    public void EqualityTest_BothNull()
    {
        Seq<int> x = default;
        Seq<int> y = default;

        var eq = x == y;

        Assert.True(eq);
    }

    [Fact]
    public void EqualityTest_LeftNull()
    {
        Seq<int> x = default;
        Seq<int> y = Seq(1, 2, 3);

        var eq = x == y;

        Assert.False(eq);
    }

    [Fact]
    public void EqualityTest_RightNull()
    {
        Seq<int> x = Seq(1, 2, 3);
        Seq<int> y = default;

        var eq = x == y;

        Assert.False(eq);
    }

    [Fact]
    public void EqualityTest()
    {
        Seq<int> x = Seq(1, 2, 3);
        Seq<int> y = Seq(1, 2, 3);

        var eq = x == y;

        Assert.True(eq);
    }

    /// <summary>
    /// Test ensures that lazily evaluated Seq returns the same as strictly evaluated when multiple enumerations occur
    /// </summary>
    [Fact]
    public void GetEnumeratorTest()
    {           
        var res1 = empty<int>();
        var res2 = empty<int>();

        var a = new List<int> { 1, 2, 3, 4 }.AsIterable();

        var s1 = a.ToSeq();
        var s2 = a.ToSeq().Strict();

        foreach (var s in s1)
        {
            if (s == 2)
            {
                s1.FoldWhile("", (x, y) => "", x => x.Value != 3);
            }
            res1 = res1.Add(s);
        }

        foreach (var s in s2)
        {
            if (s == 2)
            {
                s2.FoldWhile("", (x, y) => "", x => x.Value != 3);
            }
            res2 = res2.Add(s);
        }

        var eq = res1 == res2;

        Assert.True(eq);
    }

    [Fact]
    public void AddTest()
    {
        var a = Seq("a");

        var b = a.Add("b");

        var c = a.Add("c");

        Assert.Equal("a", string.Join("|", a));
        Assert.Equal("a|b", string.Join("|", b));
        Assert.Equal("a|c", string.Join("|", c));

    }

    [Fact]
    public void InitStrictTest()
    {
        var sa = Seq(1, 2, 3, 4, 5);

        var sb = sa.Init; // [1,2,3,4]
        var sc = sb.Init; // [1,2,3]
        var sd = sc.Init; // [1,2]
        var se = sd.Init; // [1]
        var sf = se.Init; // []

        Assert.True(sb == Seq(1, 2, 3, 4));
        Assert.True(sc == Seq(1, 2, 3));
        Assert.True(sd == Seq(1, 2));
        Assert.True(se == Seq(1));
        Assert.True(sf == Empty);
    }

    [Fact]
    public void InitLazyTest()
    {
        var sa = toSeq(Range(1, 5));

        var sb = sa.Init; // [1,2,3,4]
        var sc = sb.Init; // [1,2,3]
        var sd = sc.Init; // [1,2]
        var se = sd.Init; // [1]
        var sf = se.Init; // []

        Assert.True(sb == Seq(1, 2, 3, 4));
        Assert.True(sc == Seq(1, 2, 3));
        Assert.True(sd == Seq(1, 2));
        Assert.True(se == Seq(1));
        Assert.True(sf == Empty);
    }

    [Fact]
    public void InitConcatTest()
    {
        var sa = toSeq(Range(1, 2)) + toSeq(Range(3, 3));

        var sb = sa.Init; // [1,2,3,4]
        var sc = sb.Init; // [1,2,3]
        var sd = sc.Init; // [1,2]
        var se = sd.Init; // [1]
        var sf = se.Init; // []

        Assert.True(sb == Seq(1, 2, 3, 4));
        Assert.True(sc == Seq(1, 2, 3));
        Assert.True(sd == Seq(1, 2));
        Assert.True(se == Seq(1));
        Assert.True(sf == Empty);
    }

    [Fact]
    public void HashTest()
    {
        var s1 = Seq("test");
        var s2 = Seq("test");

        Assert.True(s1.GetHashCode() == s2.GetHashCode());
    }
        
    [Fact]
    public void TakeWhileTest()
    {
        var str = "                          <p>The</p>".AsIterable();
        Assert.Equal("                          ",
                     string.Join("", str.ToSeq().TakeWhile(ch => ch == ' ')));
    }

    [Fact]
    public void TakeWhileIndex()
    {
        var str = "                          <p>The</p>".AsIterable();
        Assert.Equal("                          ",
                     string.Join("", str.ToSeq().TakeWhile((ch, index) => index != 26)));
    }

    [Fact]
    public void TakeWhile_HalfDefaultCapacityTest()
    {
        var str = "1234".AsIterable();
        Assert.Equal("1234", string.Join("", str.ToSeq().TakeWhile(ch => true)));
    }

    [Fact]
    public void TakeWhileIndex_HalfDefaultCapacityTest()
    {
        var str = "1234".AsIterable();
        Assert.Equal("1234", string.Join("", str.ToSeq().TakeWhile((ch, index) => true)));
    }
        
    [Fact]
    public void GeneratingZeroGivesEmptySequence()
    {
        var actual = generate(0, _ => unit);
        Assert.Equal(Seq<Unit>(), actual);
    }

    [Fact]
    public void TakingOneAfterGeneratingZeroGivesEmptySequence()
    {
        var actual = generate(0, _ => unit).Take(1);
        Assert.Equal(Seq<Unit>(), actual);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    [InlineData(3, 4)]
    [InlineData(4, 5)]
    [InlineData(5, 6)]
    [InlineData(6, 7)]
    [InlineData(7, 8)]
    [InlineData(8, 9)]
    [InlineData(9, 10)]
    [InlineData(10, 20)]
    [InlineData(100, 1000)]
    [InlineData(1000, 10000)]
    public void TakingNAfterGeneratingMoreThanNGivesLengthNSequence(int actualLength, int tryToTake)
    {
        var expect = toSeq(generate(actualLength, identity)).Strict();
            
        var actual = generate(actualLength, identity).Take(tryToTake);
        Assert.Equal(expect, actual);
    }
        
    [Fact]
    public void SeqConcatTest()
    {
        var seq1 = (from x in new[] { 1 }.AsIterable()
                    select x).ToSeq();

        var seq2 = (from x in new[] { 2 }.AsIterable()
                    select x).ToSeq();


        var seq3 = (from x in new[] { 3 }.AsIterable()
                    select x).ToSeq();

        Assert.Equal(
            Seq(1, 2, 3),
            seq1
               .Concat(seq2)
               .Concat(seq3));
    }

    [Fact]
    public void Concat_strict_and_strict_then_index_test()
    {
        var s1 = Seq(1, 2, 3);
        var s2 = Seq(4, 5, 6);
        var s3 = s1.Concat(s2);
        Assert.Equal(1, s3[0]);
        Assert.Equal(2, s3[1]);
        Assert.Equal(3, s3[2]);
        Assert.Equal(4, s3[3]);
        Assert.Equal(5, s3[4]);
        Assert.Equal(6, s3[5]);
    }

    [Fact]
    public void Concat_lazy_and_strict_then_index_test_1()
    {
        var s1 = Range(1, 3).ToSeq();
        var s2 = Seq(4, 5, 6);
        var s3 = s1.Concat(s2);
        Assert.Equal(1, s3[0]);
        Assert.Equal(2, s3[1]);
        Assert.Equal(3, s3[2]);
        Assert.Equal(4, s3[3]);
        Assert.Equal(5, s3[4]);
        Assert.Equal(6, s3[5]);
    }

    [Fact]
    public void Concat_lazy_and_strict_then_index_test_2()
    {
        var s1 = Range(1, 3).ToSeq();
        var s2 = Seq(4, 5, 6);
        var s3 = s1.Concat(s2);
        Assert.Equal(6, s3[5]);
        Assert.Equal(5, s3[4]);
        Assert.Equal(4, s3[3]);
        Assert.Equal(3, s3[2]);
        Assert.Equal(2, s3[1]);
        Assert.Equal(1, s3[0]);
    }
        
    [Fact]
    public void Concat_lazy_and_lazy_then_index_test_1()
    {
        var s1 = Range(1, 3).ToSeq();
        var s2 = Range(4, 3).ToSeq();
        var s3 = s1.Concat(s2);
        Assert.Equal(1, s3[0]);
        Assert.Equal(2, s3[1]);
        Assert.Equal(3, s3[2]);
        Assert.Equal(4, s3[3]);
        Assert.Equal(5, s3[4]);
        Assert.Equal(6, s3[5]);
    }

    [Fact]
    public void Concat_lazy_and_lazy_then_index_test_2()
    {
        var s1 = Range(1, 3).ToSeq();
        var s2 = Range(4, 3).ToSeq();
        var s3 = s1.Concat(s2);
        Assert.Equal(6, s3[5]);
        Assert.Equal(5, s3[4]);
        Assert.Equal(4, s3[3]);
        Assert.Equal(3, s3[2]);
        Assert.Equal(2, s3[1]);
        Assert.Equal(1, s3[0]);
    }

    [Fact]
    public void CheckItems()
    {
        var xs = Seq<int>();
        Assert.Equal(0, xs.Count);
            
        xs = Seq(0);
        Assert.Equal(1, xs.Count);
        Assert.Equal(0, xs[0]);
            
        xs = Seq(0, 1);
        Assert.Equal(2, xs.Count);
        Assert.Equal(0, xs[0]);
        Assert.Equal(1, xs[1]);
            
        xs = Seq(0, 1, 2);
        Assert.Equal(3, xs.Count);
        Assert.Equal(0, xs[0]);
        Assert.Equal(1, xs[1]);
        Assert.Equal(2, xs[2]);
            
        xs = Seq(0, 1, 2, 3);
        Assert.Equal(4, xs.Count);
        Assert.Equal(0, xs[0]);
        Assert.Equal(1, xs[1]);
        Assert.Equal(2, xs[2]);
        Assert.Equal(3, xs[3]);
            
        xs = Seq(0, 1, 2, 3, 4);
        Assert.Equal(5, xs.Count);
        Assert.Equal(0, xs[0]);
        Assert.Equal(1, xs[1]);
        Assert.Equal(2, xs[2]);
        Assert.Equal(3, xs[3]);
        Assert.Equal(4, xs[4]);
            
        xs = Seq(0, 1, 2, 3, 4, 5);
        Assert.Equal(6, xs.Count);
        Assert.Equal(0, xs[0]);
        Assert.Equal(1, xs[1]);
        Assert.Equal(2, xs[2]);
        Assert.Equal(3, xs[3]);
        Assert.Equal(4, xs[4]);
        Assert.Equal(5, xs[5]);
            
        xs = Seq(0, 1, 2, 3, 4, 5, 6);
        Assert.Equal(7, xs.Count);
        Assert.Equal(0, xs[0]);
        Assert.Equal(1, xs[1]);
        Assert.Equal(2, xs[2]);
        Assert.Equal(3, xs[3]);
        Assert.Equal(4, xs[4]);
        Assert.Equal(5, xs[5]);
        Assert.Equal(6, xs[6]);
            
        xs = Seq(0, 1, 2, 3, 4, 5, 6, 7);
        Assert.Equal(8, xs.Count);
        Assert.Equal(0, xs[0]);
        Assert.Equal(1, xs[1]);
        Assert.Equal(2, xs[2]);
        Assert.Equal(3, xs[3]);
        Assert.Equal(4, xs[4]);
        Assert.Equal(5, xs[5]);
        Assert.Equal(6, xs[6]);
        Assert.Equal(7, xs[7]);
            
        xs = Seq(0, 1, 2, 3, 4, 5, 6, 7, 8);
        Assert.Equal(9, xs.Count);
        Assert.Equal(0, xs[0]);
        Assert.Equal(1, xs[1]);
        Assert.Equal(2, xs[2]);
        Assert.Equal(3, xs[3]);
        Assert.Equal(4, xs[4]);
        Assert.Equal(5, xs[5]);
        Assert.Equal(6, xs[6]);
        Assert.Equal(7, xs[7]);
        Assert.Equal(8, xs[8]);
            
        xs = Seq(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
        Assert.Equal(10, xs.Count);
        Assert.Equal(0, xs[0]);
        Assert.Equal(1, xs[1]);
        Assert.Equal(2, xs[2]);
        Assert.Equal(3, xs[3]);
        Assert.Equal(4, xs[4]);
        Assert.Equal(5, xs[5]);
        Assert.Equal(6, xs[6]);
        Assert.Equal(7, xs[7]);
        Assert.Equal(8, xs[8]);
        Assert.Equal(9, xs[9]);
            
        xs = Seq(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        Assert.Equal(11, xs.Count);
        Assert.Equal(0, xs[0]);
        Assert.Equal(1, xs[1]);
        Assert.Equal(2, xs[2]);
        Assert.Equal(3, xs[3]);
        Assert.Equal(4, xs[4]);
        Assert.Equal(5, xs[5]);
        Assert.Equal(6, xs[6]);
        Assert.Equal(7, xs[7]);
        Assert.Equal(8, xs[8]);
        Assert.Equal(9, xs[9]);
        Assert.Equal(10, xs[10]);
            
        xs = Seq(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
        Assert.Equal(12, xs.Count);
        Assert.Equal(0, xs[0]);
        Assert.Equal(1, xs[1]);
        Assert.Equal(2, xs[2]);
        Assert.Equal(3, xs[3]);
        Assert.Equal(4, xs[4]);
        Assert.Equal(5, xs[5]);
        Assert.Equal(6, xs[6]);
        Assert.Equal(7, xs[7]);
        Assert.Equal(8, xs[8]);
        Assert.Equal(9, xs[9]);
        Assert.Equal(10, xs[10]);
        Assert.Equal(11, xs[11]);
            
        xs = Seq(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12);
        Assert.Equal(13, xs.Count);
        Assert.Equal(0, xs[0]);
        Assert.Equal(1, xs[1]);
        Assert.Equal(2, xs[2]);
        Assert.Equal(3, xs[3]);
        Assert.Equal(4, xs[4]);
        Assert.Equal(5, xs[5]);
        Assert.Equal(6, xs[6]);
        Assert.Equal(7, xs[7]);
        Assert.Equal(8, xs[8]);
        Assert.Equal(9, xs[9]);
        Assert.Equal(10, xs[10]);
        Assert.Equal(11, xs[11]);
        Assert.Equal(12, xs[12]);
    }

    [Fact]
    public void SeqHashCodeRegression()
    {
        // GetHashCode is internally used to compare Seq values => has to be equal irrespective of creation method 

        var originalStrictTail = Seq(2, 3);
        var lazyTailSeq        = Set(1, 2, 3).Tail().ToSeq();
        var lazySeqTail        = Set(1, 2, 3).ToSeq().Tail;
        var lazyPattern        = Set(1, 2, 3).Case is (int _, Seq<int> tail) ? tail : throw new ("invalid");
            
        Assert.Equal(originalStrictTail.GetHashCode(), lazyTailSeq.GetHashCode());
        Assert.Equal(originalStrictTail, lazyTailSeq);
        Assert.Equal(originalStrictTail.GetHashCode(), lazySeqTail.GetHashCode());
        Assert.Equal(originalStrictTail, lazySeqTail);
        Assert.Equal(originalStrictTail.GetHashCode(), lazyPattern.GetHashCode());
        Assert.Equal(originalStrictTail, lazyPattern);
    }
        
    [Fact]
    public void SequenceParallelRandomDelayTest()
    {
        var input = Seq(1, 2, 3, 2, 5, 1, 1, 2, 3, 2, 1, 2, 4, 2, 1, 5, 6, 1, 3, 6, 2);
	
        var ma = input.Select(DoDelay).Traverse(identity).As();

        var res = from v in ma.Run().As()
                  select v switch
                         {
                             Either.Right<string, Seq<int>> (var seq) => seq.SequenceEqual(input),
                             _                                        => false
                         };

        Assert.True(res.Run());
    }

    static EitherT<string, IO, int> DoDelay(int seconds)
    {
        return liftIO(async () => await F(seconds));
        static async ValueTask<Either<string, int>> F(int seconds)
        {
            await Task.Delay(seconds * 100);
            return seconds;
        }
    }
}
