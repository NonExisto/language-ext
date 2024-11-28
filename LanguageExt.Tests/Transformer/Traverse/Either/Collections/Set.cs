using FluentAssertions;
using LanguageExt.Common;
using Xunit;
using Rint = LanguageExt.Tests.ReverseNumber<int>;

namespace LanguageExt.Tests.Transformer.Traverse.EitherT.Collections;

public class SetEither
{
    [Fact]
    public void EmptySetIsRightEmptySet()
    {
        Set<Either<Error, int>> ma = Empty;

        var mb = ma.Traverse(x => x).As();

        Assert.True(mb == Right(Set<int>.Empty));
    }
        
    [Fact]
    public void SetRightsIsRightSets()
    {
        var ma = Set(Right<Error, int>(1), Right<Error, int>(2), Right<Error, int>(3));

        var mb = ma.Traverse(x => x).As();

        Assert.True(mb == Right(Set(1, 2, 3)));

        foreach (var set in mb)
        {
            set.Should<int>().BeInAscendingOrder();
        }
    }

    [Fact]
    public void SetRightsIsRightSetsReverse()
    {
        var ma = Set(Right<Error, Rint>(1), Right<Error, Rint>(2), Right<Error, Rint>(3));

        var mb = ma.Traverse(x => x).As();

        Assert.True(mb == Right(Set<Rint>(1, 2, 3)));

        foreach (var set in mb)
        {
            set.AsEnumerable().Select(x => x.Value).Should<int>().BeInDescendingOrder();
        }
    }
        
    [Fact]
    public void SetRightAndLeftIsLeftEmpty()
    {
        var ma = Set(Right<Error, int>(1), Right<Error, int>(2), Left<Error, int>(Error.New("alternative")));

        var mb = ma.Traverse(x => x).As();

        Assert.True(mb == Left(Error.New("alternative")));
    }
}
