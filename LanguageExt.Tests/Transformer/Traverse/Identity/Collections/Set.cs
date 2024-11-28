using FluentAssertions;
using Xunit;
using Rint = LanguageExt.Tests.ReverseNumber<int>;

namespace LanguageExt.Tests.Transformer.Traverse.Identity.Collections;

public class Set
{
    [Fact]
    public void EmptySetIsEmpty()
    {
        var ma = Set<Identity<int>>.Empty;

        var mb = ma.Traverse(identity);

        Assert.Equal(Id(Set<int>.Empty), mb);
    }

    [Fact]
    public void SetOfIdentitiesIsIdentityOfSet()
    {
        var ma = Set(Id(1), Id(3), Id(5));

        var mb = ma.Traverse(identity);

        Assert.Equal(Id(Set(1, 3, 5)), mb);

        mb.As().Value.Should<int>().BeInAscendingOrder();
    }

    [Fact]
    public void SetOfIdentitiesIsIdentityOfSetReverse()
    {
        var ma = Set(Id<Rint>(1), Id<Rint>(3), Id<Rint>(5));

        var mb = ma.Traverse(identity);

        Assert.Equal(Id(Set<Rint>(1, 3, 5)), mb);

        mb.As().Value.AsEnumerable().Select(x => x.Value).Should<int>().BeInDescendingOrder();
    }
}
