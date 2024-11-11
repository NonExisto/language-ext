using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LanguageExt.Tests;

public class SeqModuleTests
{
    [Fact]
    public void EmptyTest() => Assert.True(Seq.empty<int>().IsEmpty);

    [Fact]
    public void CreateTest()
    {
        Assert.True(Seq.create<int>().IsEmpty);

        var seq = Seq.create(1, 2, 3, 4, 5);

        Assert.Equal(1, seq.Head);
        Assert.Equal(2, seq.Tail.Head);
        Assert.Equal(3, seq.Tail.Tail.Head);
        Assert.Equal(4, seq.Tail.Tail.Tail.Head);
        Assert.Equal(5, seq.Tail.Tail.Tail.Tail.Head);

        var seqr = Seq.createRange(new[] { 1, 2, 3, 4, 5 });

        Assert.Equal(1, seqr.Head);
        Assert.Equal(2, seqr.Tail.Head);
        Assert.Equal(3, seqr.Tail.Tail.Head);
        Assert.Equal(4, seqr.Tail.Tail.Tail.Head);
        Assert.Equal(5, seqr.Tail.Tail.Tail.Tail.Head);
    }

    [Fact]
    public void InitTest()
    {
        int            counter = 0;
        int run(int x) { counter++; return x; }

        var seq = Seq.generate(5, x => run((x + 1) * 2));

        var fst = seq.Take(1).Head;

        Assert.Equal(1, counter);
        Assert.Equal(2, fst);

        var snd = seq.Skip(1).Take(1).Head;

        Assert.Equal(2, counter);
        Assert.Equal(4, snd);

        var thd = seq.Skip(2).Take(1).Head;

        Assert.Equal(3, counter);
        Assert.Equal(6, thd);

        var fth = seq.Skip(3).Take(1).Head;

        Assert.Equal(4, counter);
        Assert.Equal(8, fth);

        var fit = seq.Skip(4).Take(1).Head;

        Assert.Equal(5, counter);
        Assert.Equal(10, fit);

        var sum = seq.Sum();

        Assert.Equal(5, counter);
        Assert.Equal(30, sum);
    }

    [Fact]
    public void EquivalentOfTheInitTestWithIEnumerable()
    {
        int            counter = 0;
        int run(int x) { counter++; return x; }

        var seq = List.generate(5, x => run((x + 1) * 2));

        var fst = seq.Take(1).First();

        Assert.Equal(1, counter);
        Assert.Equal(2, fst);

        var snd = seq.Skip(1).Take(1).First();

        // Assert.True(counter == 2);  equals 3 by this point
        Assert.Equal(4, snd);

        var thd = seq.Skip(2).Take(1).First();

        // Assert.True(counter == 3);   equals 6 by this point
        Assert.Equal(6, thd);

        var fth = seq.Skip(3).Take(1).First();

        // Assert.True(counter == 4);   equals 10 by this point (double what the InitTest needs!)
        Assert.Equal(8, fth);

        var fit = seq.Skip(4).Take(1).First();

        //Assert.True(counter == 5);    equals 15 by this point (treble what the InitTest needs!)
        Assert.Equal(10, fit);

        var sum = seq.Sum();

        // Assert.True(counter == 5);   equals 20 by this point(four times what the InitTest needs!!!)
        Assert.Equal(30, sum);
    }

    [Fact]
    public void TailsTestIterative()
    {
        var seq = Seq(1, 2, 3, 4, 5);

        var seqs = Seq.tails(seq);

        Assert.True(seqs.Head                     == Seq(1, 2, 3, 4, 5));
        Assert.True(seqs.Tail.Head                == Seq(2, 3, 4, 5));
        Assert.True(seqs.Tail.Tail.Head           == Seq(3, 4, 5));
        Assert.True(seqs.Tail.Tail.Tail.Head      == Seq(4, 5));
        Assert.True(seqs.Tail.Tail.Tail.Tail.Head == Seq.create(5));
        Assert.True(seqs.Tail.Tail.Tail.Tail.Tail.IsEmpty);
    }

    [Fact]
    public void TailsTestRecursive()
    {
        var seq = Seq(1, 2, 3, 4, 5);

        var seqs = Seq.tailsr(seq);

        Assert.True(seqs.Head                     == Seq(1, 2, 3, 4, 5));
        Assert.True(seqs.Tail.Head                == Seq(2, 3, 4, 5));
        Assert.True(seqs.Tail.Tail.Head           == Seq(3, 4, 5));
        Assert.True(seqs.Tail.Tail.Tail.Head      == Seq(4, 5));
        Assert.True(seqs.Tail.Tail.Tail.Tail.Head == Seq.create(5));
        Assert.True(seqs.Tail.Tail.Tail.Tail.Tail.IsEmpty);
    }

    [Fact]
    public void CountTests()
    {
        int            counter = 0;
        int run(int x) { counter++; return x; }

        var seq1 = Seq.generate(5, x => run((x + 1) * 2));
        var seq2 = 1.Cons(seq1);

        var cnt1 = seq1.Count;
        var cnt2 = seq2.Count;

        Assert.Equal(5, counter);
        Assert.Equal(5, cnt1);
        Assert.Equal(6, cnt2);

        var seq3 = Seq.generate(5, x => run((x + 1) * 2));
        var seq4 = 1.Cons(seq3);

        var cnt3 = seq4.Count;
        var cnt4 = seq3.Count;

        Assert.Equal(10, counter);
        Assert.Equal(5, cnt4);
        Assert.Equal(6, cnt3);
    }

    /// <summary>
    /// Runs 1000 tasks that sums the same lazy Seq to
    /// make sure we get the same result as a synchronous
    /// sum.
    /// </summary>
    [Fact]
    public async Task ParallelTests()
    {
        var sum = Range(1, 10000).Sum();

        var seq = toSeq(Range(1, 10000));

        var tasks = new List<Task<int>>();
        foreach(var x in Range(1, 1000))
        {
            tasks.Add(Task.Run(() => seq.Sum()));
        }

        await Task.WhenAll(tasks.ToArray());

        var results = tasks.Select(t => t.Result).ToArray();

        seq.Iter((i, x) => Assert.True(x != 0, $"Invalid value in the sequence at index {i}"));

        foreach (var task in results)
        {
            Assert.True(task == sum, $"Result is {task}, should be: {sum}");
        }
    }
}
