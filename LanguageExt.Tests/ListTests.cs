﻿using Xunit;
using System;
using System.Linq;
using static LanguageExt.List;

namespace LanguageExt.Tests;

public class ListTests
{
    [Fact]
    public void ConsTest1()
    {
        var test = Prelude.Cons(1, Prelude.Cons(2, Prelude.Cons(3, Prelude.Cons(4, Prelude.Cons(5, empty<int>())))));

        var array = test.ToArray();

        Assert.Equal(1, array[0]);
        Assert.Equal(2, array[1]);
        Assert.Equal(3, array[2]);
        Assert.Equal(4, array[3]);
        Assert.Equal(5, array[4]);
    }

    [Fact]
    public void ListConstruct()
    {
        var test = List(1, 2, 3, 4, 5);

        var array = test.ToArray();

        Assert.Equal(1, array[0]);
        Assert.Equal(2, array[1]);
        Assert.Equal(3, array[2]);
        Assert.Equal(4, array[3]);
        Assert.Equal(5, array[4]);
    }

    [Fact]
    public void MapTest()
    {
        // Generates 10,20,30,40,50
        var input   = List(1, 2, 3, 4, 5);
        var output1 = map(input, x => x * 10);

        // Generates 30,40,50
        var output2 = filter(output1, x => x > 20);

        // Generates 120
        var output3 = fold(output2, 0, (x, s) => s + x);

        Assert.Equal(120, output3);
    }

    [Fact]
    public void ReduceTest()
    {
        // Generates 10,20,30,40,50
        var input   = List(1, 2, 3, 4, 5);
        var output1 = map(input, x => x * 10);

        // Generates 30,40,50
        var output2 = filter(output1, x => x > 20);

        // Generates 120
        var output3 = reduce(output2, (x, s) => s + x);

        Assert.Equal(120, output3);
    }

    [Fact]
    public void MapTestFluent()
    {
        var res = List(1, 2, 3, 4, 5)
                 .Map(x => x * 10)
                 .Filter(x => x > 20)
                 .Fold(0, (x, s) => s + x);

        Assert.Equal(120, res);
    }

    [Fact]
    public void ReduceTestFluent()
    {
        var res = List(1, 2, 3, 4, 5)
                 .Map(x => x * 10)
                 .Filter(x => x > 20)
                 .Reduce((x, s) => s + x);

        Assert.Equal(120, res);
    }

    [Fact]
    public void RangeTest1()
    {
        var r = Range(0, 10).AsIterable();
        for (int i = 0; i < 10; i++)
        {
            Assert.True(r.First() == i);
            r = r.Skip(1);
        }
    }

    [Fact]
    public void RangeTest2()
    {
        var r = Range(0, 100, 10).AsIterable();
        for (int i = 0; i < 10; i+=10)
        {
            Assert.True(r.First() == i);
            r = r.Skip(1);
        }
    }

    [Fact]
    public void RangeTest4()
    {
        var r = Range('a', 'f');
        Assert.Equal("abcdef", string.Join("", r));
    }

    [Fact]
    public void RangeTest5()
    {
        var r = Range('f', 'a');
        Assert.Equal("fedcba", string.Join("", r));
    }

    [Fact]
    public void RepeatTest()
    {
        var r = repeat("Hello", 10);

        foreach (var item in r)
        {
            Assert.Equal("Hello", item);
        }
    }


    [Fact]
    public void GenerateTest()
    {
        var r = generate(10, i => "Hello " + i );

        for (int i = 0; i < 10; i++)
        {
            Assert.True(r.First() == "Hello " +i);
            r = r.Skip(1);
        }
    }

    [Fact]
    public void UnfoldTest()
    {
        var test = List(0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181);

        var fibs = take(unfold((0, 1), tup => map(tup, (a, b) => Some((a, (b, a + b))))), 20);

        Assert.True( test.SequenceEqual(fibs) );
    }

    [Fact]
    public void UnfoldTupleTest()
    {
        var test = List(0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181);

        var fibs = take( unfold( (0, 1), (a, b) => Some((a, b, a + b)) ), 20);

        Assert.True(test.SequenceEqual(fibs));
    }

    [Fact]
    public void UnfoldSingleTest()
    {
        var e = new Exception("Outer", new Exception("Inner"));

        var list = unfold(e, (state) =>
                                 state == null
                                     ? None
                                     : Optional(state.InnerException)
        );

        var res = list.ToList();

        Assert.True(res[0].Message == "Outer" && res[1].Message == "Inner");
    }

    [Fact]
    public void ReverseListTest1()
    {
        var list = List(1, 2, 3, 4, 5);
        var rev  = list.Rev();

        Assert.Equal(5, rev[0]);
        Assert.Equal(1, rev[4]);
    }

    [Fact]
    public void ReverseListTest2()
    {
        var list = List(1, 2, 3, 4, 5);
        var rev  = list.Rev();

        Assert.True(rev.IndexOf(1) == 4, "Should have been 4, actually is: " + rev.IndexOf(1));
        Assert.True(rev.IndexOf(5) == 0, "Should have been 0, actually is: " + rev.IndexOf(5));
    }

    [Fact]
    public void ReverseListTest3()
    {
        var list = List(1, 1, 2, 2, 2);
        var rev  = list.Rev();

        Assert.True(rev.LastIndexOf(1) == 4, "Should have been 4, actually is: " + rev.LastIndexOf(1));
        Assert.True(rev.LastIndexOf(2) == 2, "Should have been 2, actually is: " + rev.LastIndexOf(5));
    }

    [Fact]
    public void OpEqualTest()
    {
        var goodOnes = List(
            (List(1, 2, 3), List(1, 2, 3)),
            (Lst<int>.Empty, Lst<int>.Empty)
        );
        var badOnes = List(
            (List(1, 2, 3), List(1, 2, 4)),
            (List(1, 2, 3), Lst<int>.Empty)
        );

        goodOnes.Iter(t => t.Iter((fst, snd) =>
                                  {
                                      Assert.True(fst  == snd, $"'{fst}' == '{snd}'");
                                      Assert.False(fst != snd, $"'{fst}' != '{snd}'");
                                  }));

        badOnes.Iter(t => t.Iter((fst, snd) =>
                                 {
                                     Assert.True(fst  != snd, $"'{fst}' != '{snd}'");
                                     Assert.False(fst == snd, $"'{fst}' == '{snd}'");
                                 }));
    }

    [Fact]
    public void IterSimpleTest()
    {
        var embeddedSideEffectResult = 0;
        var expression = from dummy in Some(unit)
                         from i in List(2, 3, 5)
                         let _ = fun(() => embeddedSideEffectResult += i)()
                         select i;

        Assert.Equal(0, embeddedSideEffectResult);

        var sideEffectByAction = 0;

        expression.AsIterable().Iter(i => sideEffectByAction += i * i);
        Assert.Equal(2     + 3     + 5, embeddedSideEffectResult);
        Assert.Equal(2 * 2 + 3 * 3 + 5 * 5, sideEffectByAction);
    }

    [Fact]
    public void IterPositionalTest()
    {
        var embeddedSideEffectResult = 0;
        var expression = from dummy in Some(unit)
                         from i in List(2, 3, 5)
                         let _ = fun(() => embeddedSideEffectResult += i)()
                         select i;

        Assert.Equal(0, embeddedSideEffectResult);

        var sideEffectByAction = 0;

        expression.AsIterable().Iter((pos, i) => sideEffectByAction += i * pos);
        Assert.Equal(2     + 3     + 5, embeddedSideEffectResult);
        Assert.Equal(2 * 0 + 3 * 1 + 5 * 2, sideEffectByAction);
    }

    [Fact]
    public void ConsumeTest()
    {
        var embeddedSideEffectResult = 0;
        System.Collections.Generic.IEnumerable<int> expression = from dummy in Some(unit)
                                                                 from i in List(2, 3, 5)
                                                                 let _ = fun(() => embeddedSideEffectResult += i)()
                                                                 select i;

        Assert.Equal(0, embeddedSideEffectResult);
        expression.Consume();
        Assert.Equal(2 + 3 + 5, embeddedSideEffectResult);
    }
        
    [Fact]
    public void SkipLastTest1()
    {
        var list = List(1, 2, 3, 4, 5);

        var skipped = list.SkipLast().AsIterable().ToLst();

        Assert.True(skipped == List(1, 2, 3, 4));
    }

    [Fact]
    public void SkipLastTest2()
    {
        var list = List<int>();

        var skipped = list.SkipLast().AsIterable().ToLst();

        Assert.True(skipped == list);
    }

    [Fact]
    public void SkipLastTest3()
    {
        var list = List(1, 2, 3, 4, 5);

        var skipped = list.SkipLast(2).AsIterable().ToLst();

        Assert.True(skipped == List(1, 2, 3));
    }

    [Fact]
    public void SkipLastTest4()
    {
        var list = List<int>();

        var skipped = list.SkipLast(2).AsIterable().ToLst();

        Assert.True(skipped == list);
    }

    [Fact]
    public void SetItemTest()
    {
        Lst<int> lint = [];
        lint = lint.Insert(0, 0).Insert(1, 1).Insert(2, 2).Insert(3, 3);

        Assert.Equal(0, lint[0]);
        Assert.Equal(1, lint[1]);
        Assert.Equal(2, lint[2]);
        Assert.Equal(3, lint[3]);

        lint = lint.SetItem(2, 500);

        Assert.Equal(0, lint[0]);
        Assert.Equal(1, lint[1]);
        Assert.Equal(500, lint[2]);
        Assert.Equal(3, lint[3]);
    }

    [Fact]
    public void RemoveAllTest()
    {
        var test = List(1, 2, 3, 4, 5);
        Assert.True(test.RemoveAll(x => x % 2 == 0) == List(1, 3, 5));
    }

    [Fact]
    public void RemoveAtInsertTest()
    {
        Lst<int> lint = [];
        lint = lint.Insert(0, 0).Insert(1, 1).Insert(2, 2).Insert(3, 3);

        Assert.Equal(0, lint[0]);
        Assert.Equal(1, lint[1]);
        Assert.Equal(2, lint[2]);
        Assert.Equal(3, lint[3]);

        lint = lint.RemoveAt(2);

        Assert.Equal(0, lint[0]);
        Assert.Equal(1, lint[1]);
        Assert.Equal(3, lint[2]);

        lint = lint.Insert(2, 500);

        Assert.Equal(0, lint[0]);
        Assert.Equal(1, lint[1]);
        Assert.Equal(500, lint[2]);
        Assert.Equal(3, lint[3]);
    }

    [Fact]
    public void RemoveRange()
    {
        var list = List(1, 2, 3, 4);

        Assert.Equal(list.RemoveRange(2, 2), List(1, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveRange(2, 3));
    }

    [Fact]
    public void SetItemManyTest()
    {
        var range = IterableExtensions.AsIterable(Range(0, 100)).ToLst();
        for (int i = 0; i < 100; i++)
        {
            range = range.SetItem(i, i * 2);
            Assert.True(range[i] == i  * 2);
            for(var b = 0; b < i; b++)
            {
                Assert.True(range[b] == b * 2);
            }
            for (var a = i + 1; a < 100; a++)
            {
                Assert.True(range[a] == a);
            }
        }
    }

    [Fact]
    public void RemoveAtInsertManyTest()
    {
        var range = IterableExtensions.AsIterable(Range(0, 100)).ToLst();
        for (int i = 0; i < 100; i++)
        {
            range = range.RemoveAt(i);
            Assert.Equal(99, range.Count);
            range = range.Insert(i, i * 2);
            Assert.True(range[i] == i * 2);
            for (var b = 0; b < i; b++)
            {
                Assert.True(range[b] == b * 2);
            }
            for (var a = i + 1; a < 100; a++)
            {
                Assert.True(range[a] == a);
            }
        }
    }

    [Fact]
    public void EqualsTest()
    {
        Assert.False(List(1, 2, 3).Equals(List<int>()));
        Assert.False(List<int>().Equals(List(1, 2, 3)));
        Assert.True(List<int>().Equals(List<int>()));
        Assert.True(List(1).Equals(List(1)));
        Assert.True(List(1, 2).Equals(List(1, 2)));
        Assert.False(List(1, 2).Equals(List(1, 2, 3)));
        Assert.False(List(1, 2, 3).Equals(List(1, 2)));
    }

    [Fact]
    public void ListShouldRemoveByReference()
    {
        var o0 = new object();
        var o1 = new object();
        var o2 = new object();
        var l  = List(o0, o1);
        l = l.Remove(o2);
        Assert.Equal(2, l.Count);
        l = l.Remove(o0);
        Assert.Equal(1, l.Count);
        l = l.Remove(o1);
        Assert.Equal(0, l.Count);
    }

    [Fact]
    public void ListShouldRemoveByReferenceForReverseLists()
    {
        var o0 = new object();
        var o1 = new object();
        var o2 = new object();
        var l  = List(o0, o1).Reverse();
        l = l.Remove(o2);
        Assert.Equal(2, l.Count);
        l = l.Remove(o0);
        Assert.Equal(1, l.Count);
        l = l.Remove(o1);
        Assert.Equal(0, l.Count);
    }

    [Fact]
    public void FoldTest()
    {
        var input   = List(1, 2, 3, 4, 5);
        var output1 = fold(input, "", (s, x) => s + x.ToString());
        Assert.Equal("12345", output1);
    }

    [Fact]
    public void FoldBackTest()
    {
        var input   = List(1, 2, 3, 4, 5);
        var output1 = foldBack(input, "", (s, x) => s + x.ToString());
        Assert.Equal("54321", output1);
    }

    [Fact]
    public void FoldWhileTest()
    {
        var input = List(10, 20, 30, 40, 50);

        var output1 = foldWhile(input, "", (s, x) => s + x.ToString(), x => x < 40);
        Assert.Equal("102030", output1);

        var output2 = foldWhileState(input, "", (s, x) => s + x.ToString(), (string s) => s.Length < 6);
        Assert.Equal("102030", output2);

        var output3 = foldWhile(input, 0, (s, x) => s + x, x => x < 40);
        Assert.Equal(60, output3);

        var output4 = foldWhileState(input, 0, (s, x) => s + x, s => s < 60);
        Assert.Equal(60, output4);
    }

    [Fact]
    public void FoldBackWhileTest()
    {
        var input = List(10, 20, 30, 40, 50);

        var output1 = foldBackWhile(input, "", (s, x) => s + x.ToString(), x => x >= 40);
        Assert.Equal("5040", output1);

        var output2 = foldBackWhileState(input, "", (s, x) => s + x.ToString(), (string s) => s.Length < 4);
        Assert.Equal("5040", output2);

        var output3 = foldBackWhile(input, 0, (s, x) => s + x, x => x >= 40);
        Assert.Equal(90, output3);

        var output4 = foldBackWhileState(input, 0, (s, x) => s + x, s => s < 90);
        Assert.Equal(90, output4);
    }

    [Fact]
    public void FoldUntilTest()
    {
        var input = List(10, 20, 30, 40, 50);

        var output1 = foldUntil(input, "", (s, x) => s + x.ToString(), x => x >= 40);
        Assert.Equal("102030", output1);

        var output2 = foldUntilState(input, "", (s, x) => s + x.ToString(), (string s) => s.Length >= 6);
        Assert.Equal("102030", output2);

        var output3 = foldUntil(input, 0, (s, x) => s + x, x => x >= 40);
        Assert.Equal(60, output3);

        var output4 = foldUntilState(input, 0, (s, x) => s + x, s => s >= 60);
        Assert.Equal(60, output4);
    }

    [Fact]
    public void FoldBackUntilTest()
    {
        var input = List(10, 20, 30, 40, 50);

        var output1 = foldBackUntil(input, "", (s, x) => s + x.ToString(), x => x < 40);
        Assert.Equal("5040", output1);

        var output2 = foldBackUntilState(input, "", (s, x) => s + x.ToString(), (string s) => s.Length >= 4);
        Assert.Equal("5040", output2);

        var output3 = foldBackUntil(input, 0, (s, x) => s + x, x => x < 40);
        Assert.Equal(90, output3);

        var output4 = foldBackUntilState(input, 0, (s, x) => s + x, s => s >= 90);
        Assert.Equal(90, output4);
    }

    [Fact]
    public void itemLensGetShouldGetExistingValue()
    {
        var expected = "3";
        var list     = List("0","1", "2", "3", "4", "5");
        var actual   = Lst<string>.item(3).Get(list);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void itemLensGetShouldThrowExceptionForNonExistingValue() => Assert.Throws<IndexOutOfRangeException>(() =>
                                                                                                                     {
                                                                                                                         var list = List("0", "1", "2", "3", "4", "5");
                                                                                                                         var actual = Lst<string>.item(10).Get(list);
                                                                                                                     });

    [Fact]
    public void itemOrNoneLensGetShouldGetExistingValue()
    {
        var expected = "3";
        var list     = List("0", "1", "2", "3", "4", "5");
        var actual   = Lst<string>.itemOrNone(3).Get(list);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void itemOrNoneLensGetShouldReturnNoneForNonExistingValue()
    {
        var expected = Option<string>.None;
        var list     = List("0", "1", "2", "3", "4", "5");
        var actual   = Lst<string>.itemOrNone(10).Get(list);

        Assert.Equal(expected, actual);
    }
}
