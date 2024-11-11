using System.Collections.Generic;
using Xunit;

namespace LanguageExt.Tests;

public class ListMatchingTests
{
    [Fact]
    public void RecursiveMatchSumTest()
    {
        var list0 = List<int>();
        var list1 = List(10);
        var list5 = List(10,20,30,40,50);

        Assert.Equal(0, Sum(list0));
        Assert.Equal(10, Sum(list1));
        Assert.Equal(150, Sum(list5));
    }

    public static int Sum(IEnumerable<int> list) =>
        match(list,
              ()      => 0,
              x       => x,
              (x, xs) => x + Sum(xs));

    [Fact]
    public void RecursiveMatchMultiplyTest()
    {
        var list0 = List<int>();
        var list1 = List(10);
        var list5 = List(10, 20, 30, 40, 50);

        Assert.Equal(0, Multiply(list0));
        Assert.Equal(10, Multiply(list1));
        Assert.Equal(12000000, Multiply(list5));
    }

    public static int Multiply(IEnumerable<int> list) =>
        list.Match(
            ()      => 0,
            x       => x,
            (x, xs) => x * Multiply(xs));

    [Fact]
    public void AnotherRecursiveMatchSumTest()
    {
        var list0 = List<int>();
        var list1 = List(10);
        var list5 = List(10, 20, 30, 40, 50);

        Assert.Equal(0, AnotherSum(list0));
        Assert.Equal(10, AnotherSum(list1));
        Assert.Equal(150, AnotherSum(list5));
    }

    public static int AnotherSum(IEnumerable<int> list) =>
        match(list,
              ()      => 0,
              (x, xs) => x + AnotherSum(xs));

    [Fact]
    public void AnotherRecursiveMatchMultiplyTest()
    {
        var list0 = List<int>();
        var list1 = List(10);
        var list5 = List(10, 20, 30, 40, 50);

        Assert.Equal(1, AnotherMultiply(list0));
        Assert.Equal(10, AnotherMultiply(list1));
        Assert.Equal(12000000, AnotherMultiply(list5));
    }

    public int AnotherMultiply(IEnumerable<int> list) =>
        list.Match(
            ()      => 1,
            (x, xs) => x * AnotherMultiply(xs));
}
