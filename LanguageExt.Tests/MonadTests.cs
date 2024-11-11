﻿using Xunit;
using static LanguageExt.List;

namespace LanguageExt.Tests;

public class MonadTests
{
    [Fact]
    public void WriterTest()
    {
        var tell1 = fun((string x) => tell(List(x)));

        var value = fun((int x) => from _ in tell1($"Got number: {x}")
                                   select x);

        var multWithLog = from a in value(3)
                          from b in value(5)
                          from _ in tell1("Gonna multiply these two")
                          select a * b;

        var res = multWithLog.Run();

        Assert.Equal(3, length(res.Output));
        Assert.Equal(15, res.Value);
        Assert.Equal("Got number: 3", head(res.Output));
        Assert.Equal("Got number: 5", head(tail(res.Output)));
        Assert.Equal("Gonna multiply these two", head(tail(tail(res.Output))));
    }

    static Writer<Seq<string>, int> writer(int value, Seq<string> output) => 
        Writer.write(value, output);

    static Writer<Seq<string>, Seq<int>> multWithLog(Seq<int> input) =>
        from _ in writer(0, Seq("Start"))
        from r in input.Traverse(i => writer(i * 10, Seq($"Number: {i}")))
        select r;

    [Fact]
    public void WriterSequenceTest()
    {
        var res = multWithLog(Seq(1, 2, 3)).Run();

        Assert.True(res.Value  == Seq(10, 20, 30));
        Assert.True(res.Output == Seq("Start", "Number: 1", "Number: 2", "Number: 3"));
    }

    private class Bindings
    {
        public readonly Map<string, int> Map;
        public Bindings(params (string, int)[] items) => 
            Map = toMap(items);

        public static Bindings New(params (string, int)[] items) => 
            new (items);
    }

    [Fact]
    public void ReaderAskTest()
    {
        var lookupVar = fun((string name, Bindings bindings) => Map.find(bindings.Map, name).IfNone(0));

        var calcIsCountCorrect = from count in Reader.asks((Bindings env) => lookupVar("count", env))
                                 from bindings in ask<Bindings>()
                                 select count == Map.length(bindings.Map);

        var sampleBindings = Bindings.New(("count", 3), ("1", 1), ("b", 2));

        bool res = calcIsCountCorrect.Run(sampleBindings);
        Assert.True(res);
    }

    [Fact]
    public void ReaderLocalTest()
    {
        var calculateContentLen = from content in Reader.ask<string>()
                                  select content.Length;

        var calculateModifiedContentLen = Reader.local(content => "Prefix " + content, calculateContentLen);

        var s           = "12345";
        var modifiedLen = calculateModifiedContentLen.Run(s);
        var len         = calculateContentLen.Run(s);

        Assert.Equal(12, modifiedLen);
        Assert.Equal(5, len);
    }

    [Fact]
    public void StateTest()
    {
        var comp = from inp in State.get<string>()
                   let hw = inp + ", world"
                   from _ in State.put(hw)
                   select hw.Length;

        var r = comp.Run("hello");

        Assert.Equal(12, r.Value);
        Assert.Equal("hello, world", r.State);
    }
    // TODO: Restore when traits are complete

    /*
    [Fact]
    public void RWSTest()
    {
        var previous = RWS<MLst<string>, bool, Lst<string>,  Map<int, string>, int>(1);
        var comp =
            from val in previous
            from state in get<MLst<string>, bool, Lst<string>, Map<int, string>, int>()
            from env in ask<MLst<string>, bool, Lst<string>, Map<int, string>>()
            from _ in put<MLst<string>, bool, Lst<string>, Map<int, string>, int>(state.Add(2, "B"))
            from __ in tell<MLst<string>, bool, Lst<string>, Map<int, string>, int>(List($"{val}", $"{env}"))
            select val + 2;

        var (value, output, finalState, faulted) = comp(true, Map((1, "a")));
        Assert.True(value      == 3);
        Assert.True(output     == List("1", "True"));
        Assert.True(finalState == Map((1, "a"), (2, "B")));
    }

    [Fact]
    public void RWSFailTest()
    {
        var previous = RWS<MLst<string>, bool, Lst<string>,  Map<int, string>, int>(1);
        var comp =
            from val in previous
            from _ in modify<MLst<string>, bool, Lst<string>, Map<int, string>, int>(_ => failwith<Map<int,string>>(""))
            select val + 2;

        Assert.ThrowsAny<Exception>(act(comp.Run(false, Map((1, "a"))).IfFailThrow));
    }

    [Fact]
    public void RWSBottomTest()
    {
        var previous = RWS<MLst<string>, bool, Lst<string>,  Map<int, string>, int>(1);
        var comp =
            from val in previous
            where false
            select val + 3;

        Assert.ThrowsAny<Exception>(act(comp.Run(false, Map((1, "a"))).IfFailThrow));
    }
    */

    //[Fact]
    //public void ReaderWriterBindTest()
    //{
    //    var x = from a in ask<string>()
    //            from b in tell("Hello " + a)
    //            select b;

    //    Assert.True(x("everyone").Value().Output.Head() == "Hello everyone");
    //}

    //[Fact]
    //public void TryReaderBindTest()
    //{
    //    var tryadd = from a in Try(() => 123)
    //                 from b in ask<int>()
    //                 select a + b;

    //    var res = tryadd.Map(r => r.Lift(123))
    //                    .Lift();

    //    Assert.True(res == 246);
    //}

    //[Fact]
    //public void SomeLiftTest()
    //{
    //    var z = Some(Some(10));
    //    var r = z.LiftT();
    //    Assert.True(r == 10);
    //}

    //[Fact]
    //public void FilterTTest()
    //{
    //    var o = Some(List(1, 2, 3, 4, 5));
    //    var o2 = o.FilterT(n => n > 2);

    //    Assert.True(o2.Count() == 1);
    //    Assert.True(o2.CountT() == 3);
    //}


    //[Fact]
    //public void ReaderListSumTest()
    //{
    //    var r2 = from v in ask<int>()
    //             from l in List(1, 2, 3, 4, 5)
    //             select l * v;

    //    var r3 = r2.SumT()(10);

    //    Assert.True(r3 == 150);
    //}

    //private static string Error()
    //{
    //    throw new Exception("Nooooo");
    //}
}
