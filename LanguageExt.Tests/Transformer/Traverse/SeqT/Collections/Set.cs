using FluentAssertions;
using Xunit;
using Rint = LanguageExt.Tests.ReverseNumber<int>;

namespace LanguageExt.Tests.Transformer.Traverse.SeqT.Collections;

public class SetSeq
{
    [Fact]
    public void EmptyEmptyIsEmptyEmpty()
    {
        Set<Seq<int>> ma = Empty;

        var mb = ma.Traverse(mx => mx).As();


        var mc = Seq.singleton(Set<int>.Empty);

        Assert.True(mb == mc);
    }

    [Fact]
    public void SetSeqCrossProduct()
    {
        var ma = Set(Seq(1, 2), Seq(10, 20, 30));

        var mb = ma.Traverse(mx => mx).As();


        var mc = Seq(
            Set(1, 10),
            Set(1, 20),
            Set(1, 30),
            Set(2, 10),
            Set(2, 20),
            Set(2, 30));

        Assert.True(mb == mc);

        foreach (var set in mb)
        {
            set.Should<int>().BeInAscendingOrder();
        }
    }

    [Fact]
    public void SetSeqCrossProductReverse()
    {
        var ma = Set(Seq<Rint>(1, 2), Seq<Rint>(10, 20, 30));

        var mb = ma.Traverse(mx => mx).As();


        var mc = Seq(
            Set<Rint>(1, 10),
            Set<Rint>(2, 10),
            Set<Rint>(1, 20),
            Set<Rint>(2, 20),
            Set<Rint>(1, 30),
            Set<Rint>(2, 30));

        Assert.True(mb == mc);

        foreach (var set in mb)
        {
            set.AsEnumerable().Select(x => x.Value).Should<int>().BeInDescendingOrder();
        }
    }

    [Fact]
    public void SetOfEmptiesAndNonEmptiesIsEmpty()
    {
        var ma = Set(Seq<int>(), Seq(1, 2, 3));

        var mb = ma.Traverse(mx => mx).As();


        var mc = Seq<Set<int>>.Empty;

        Assert.True(mb == mc);
    }

    [Fact]
    public void SetOfEmptiesIsEmpty()
    {
        var ma = Set(Seq<int>(), Seq<int>());

        var mb = ma.Traverse(mx => mx).As();


        var mc = Seq<Set<int>>.Empty;

        Assert.True(mb == mc);
    }
}
