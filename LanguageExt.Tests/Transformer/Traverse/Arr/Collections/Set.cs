using FluentAssertions;
using Xunit;
using Rint = LanguageExt.Tests.ReverseNumber<int>;

namespace LanguageExt.Tests.Transformer.Traverse.ArrT.Collections;

public class SetArr
{
    [Fact]
    public void EmptyEmptyIsEmptyEmpty()
    {
        Set<Arr<int>> ma = Empty;

        var mb = ma.KindT<Set, Arr, Arr<int>, int>()
                   .SequenceM()
                   .AsT<Arr, Set, Set<int>, int>()
                   .As();

        var mc = Arr.singleton(Set<int>.Empty);

        Assert.True(mb == mc);
    }

    [Fact]
    public void SetTypeInvariantInMap()
    {
        var ma = Set(1,2,3,4);
        var mb = ma.Map(x => 1);

        var mc = Set(1);

        Assert.True(mb == mc);
    }

    [Fact]
    public void SetArrCrossProduct()
    {
        var ma = Set(Array(1, 2), Array(10, 20, 30));

        var mb = ma.KindT<Set, Arr, Arr<int>, int>()
                   .SequenceM()
                   .AsT<Arr, Set, Set<int>, int>()
                   .As();

        var mc = Array(
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
    public void SetArrCrossProductReverse()
    {
        var ma = Set(Array<Rint>(1, 2), Array<Rint>(10, 20, 30));

        var mb = ma.KindT<Set, Arr, Arr<Rint>, Rint>()
                   .SequenceM()
                   .AsT<Arr, Set, Set<Rint>, Rint>()
                   .As();

        var mc = Array(
            Set<Rint>(10, 1),
            Set<Rint>(20, 1),
            Set<Rint>(30, 1),
            Set<Rint>(10, 2),
            Set<Rint>(20, 2),
            Set<Rint>(30, 2));

        Assert.True(mb == mc);

        foreach (var set in mb)
        {
            set.Select(item => item.Value).Should<int>().BeInDescendingOrder();
        }
    }

    [Fact]
    public void SetOfEmptiesAndNonEmptiesIsEmpty()
    {
        var ma = Set(Array<int>(), Array(1, 2, 3));

        var mb = ma.KindT<Set, Arr, Arr<int>, int>()
                   .SequenceM()
                   .AsT<Arr, Set, Set<int>, int>()
                   .As();

        var mc = Arr<Set<int>>.Empty;

        Assert.True(mb == mc);
    }

    [Fact]
    public void SetOfEmptiesIsEmpty()
    {
        var ma = Set(Array<int>(), Array<int>());

        var mb = ma.KindT<Set, Arr, Arr<int>, int>()
                   .SequenceM()
                   .AsT<Arr, Set, Set<int>, int>()
                   .As();

        var mc = Arr<Set<int>>.Empty;

        Assert.True(mb == mc);
    }
}
