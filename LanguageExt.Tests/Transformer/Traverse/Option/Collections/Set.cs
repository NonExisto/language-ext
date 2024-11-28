using FluentAssertions;
using Xunit;
using Rint = LanguageExt.Tests.ReverseNumber<int>;

namespace LanguageExt.Tests.Transformer.Traverse.OptionT.Collections;

public class SetOption
{
    [Fact]
    public void EmptySetIsSomeEmptySet()
    {
        Set<Option<int>> ma = Empty;

        var mb = ma.Traverse(mx => mx).As();


        Assert.True(mb == Some(Set<int>.Empty));
    }
        
    [Fact]
    public void SetSomesIsSomeSets()
    {
        var ma = Set(Some(1), Some(2), Some(3));

        var mb = ma.Traverse(mx => mx).As();


        Assert.True(mb == Some(Set(1, 2, 3)));
        mb.IfSome(v => v.Should<int>().BeInAscendingOrder());
    }

    [Fact]
    public void SetSomesIsSomeSetsReverse()
    {
        var ma = Set(Some<Rint>(1), Some<Rint>(2), Some<Rint>(3));

        var mb = ma.Traverse(mx => mx).As();


        Assert.True(mb == Some(Set<Rint>(1, 2, 3)));
        mb.IfSome(v => v.AsEnumerable().Select(x => x.Value).Should<int>().BeInDescendingOrder());
    }
        
    [Fact]
    public void SetSomeAndNoneIsNone()
    {
        var ma = Set(Some(1), Some(2), None);

        var mb = ma.Traverse(mx => mx).As();


        Assert.True(mb == None);
    }
}
