﻿using System;
using System.Linq;
using LanguageExt.UnsafeValueAccess;
using Xunit;

namespace LanguageExt.Tests;

public class LinqTests
{
    [Fact]
    public void EnumerableString()
    {
        var opt  = Some("pre ");
        var list = Some(new[] { "hello", "world" }.AsEnumerable());

        var res = (from a in opt
                   from x in list
                   from y in x
                   select a + y).AsIterable();

        Assert.Equal("pre hello", res.Head().ValueUnsafe());
        Assert.Equal("pre world", res.Tail().Head().ValueUnsafe());

        opt = None;

        res = (from a in opt
               from x in list
               from y in x
               select a + y).AsIterable();;

        Assert.True(!res.Any());
    }


    [Fact]
    public void MixedLinq()
    {
        var oa = Some(1);
        var lb = List(2, 3, 4, 5);

        var r1 =
            from a in oa.Map(x => List(x)) // a : int
            from b in Some(lb)                      // b : int
            select a + b;

        Assert.True(r1 == Some(List(1, 2, 3, 4, 5)));
    }

    [Fact]
    public void WithOptionSomeList()
    {
        var res = from v in GetOptionValue(true)
                  from r in Range(1, 10)
                  select v * r;

        var res2 = res.ToList();

        Assert.Equal(10, res2.Count());
        Assert.Equal(10, res2[0]);
        Assert.Equal(100, res2[9]);
    }

    [Fact]
    public void WithOptionNoneList()
    {
        var res = from v in GetOptionValue(false)
                  from r in Range(1, 10)
                  select v * r;

        Assert.True(!res.Any());
    }
    
    [Fact]
    public void WithEitherRightList()
    {
        var res = from v in toSeq(GetEitherValue(true))
                  from r in Range(1, 10)
                  select v * r;

        var res2 = res.ToList();

        Assert.Equal(10, res.Count());
        Assert.Equal(10, res2[0]);
        Assert.Equal(100, res2[9]);
    }

    [Fact]
    public void WithEitherLeftList()
    {
        var res = from v in toSeq(GetEitherValue(false))
                  from r in Range(1, 10)
                  select v * r;

        Assert.Empty(res);
    }
    
    [Fact]
    public void WhereArrayTest()
    {
        var res1 = from v in Array(1, 2, 3, 4, 5)
                   where v < 3
                   select v;

        Assert.Equal(2, res1.Count);
    }

    [Fact]
    public void WhereOptionTest()
    {
        var res1 = from v in GetOptionValue(true)
                   where v == 10
                   select v;

        Assert.Equal(10, res1.IfNone(0));

        var res2 = from v in GetOptionValue(false)
                   where v == 10
                   select v;

        Assert.Equal(0, res2.IfNone(0));
    }

    [Fact]
    public void OptionAndOrTest()
    {
        Option<int> optional1 = None;
        Option<int> optional2 = Some(10);
        Option<int> optional3 = Some(20);

        var res = from x in optional1 || optional2
                  from y in optional3
                  select x + y;

        Assert.True(res == Some(30));
    }

    private static Option<int> GetOptionValue(bool select) =>
        select
            ? Some(10)
            : None;

    private static Either<string, int> GetEitherValue(bool select)
    {
        if (select)
            return 10;
        else
            return "left";
    }


    [Fact]
    public void OptionLst1()
    {
        var list = List(1, 2, 3, 4);
        var opt  = Some(5);

        var res = from y in opt
                  from x in list
                  select x + y;
    }


    [Fact]
    public void OptionNoneTest1()
    {
        var res1 = from x in None
                   from y in Some(123)
                   from z in Some(456)
                   select y + z;

        var res2 = from y in Some(123)
                   from x in None
                   from z in Some(456)
                   select y + z;

        var res3 = from y in Some(123)
                   from x in None
                   from z in Some(456)
                   select y + z;
    }
}
