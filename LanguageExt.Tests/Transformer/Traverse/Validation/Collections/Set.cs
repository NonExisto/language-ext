using FluentAssertions;
using LanguageExt.Common;
using Xunit;
using Rint = LanguageExt.Tests.ReverseNumber<int>;

namespace LanguageExt.Tests.Transformer.Traverse.Validation.Collections;

public class Set
{
    [Fact]
    public void EmptySetIsSuccessEmptySet()
    {
        Set<Validation<Error, string>> ma = Empty;
        var                            mb = ma.Traverse(x => x);
        Assert.Equal(Success<Error, Set<string>>(Empty), mb);
    }

    [Fact]
    public void SetSuccessIsSuccessSet()
    {
        var ma = Set(Success<Error, int>(2), Success<Error, int>(8), Success<Error, int>(64));
        var mb = ma.Traverse(x => x);
        Assert.Equal(Success<Error, Set<int>>(Set(2, 8, 64)), mb);

        foreach (var set in mb.As())
        {
            set.Should<int>().BeInAscendingOrder();
        }
    }

    [Fact]
    public void SetSuccessIsSuccessSetReverse()
    {
        var ma = Set(Success<Error, Rint>(2), Success<Error, Rint>(8), Success<Error, Rint>(64));
        var mb = ma.Traverse(x => x);
        Assert.Equal(Success<Error, Set<Rint>>(Set<Rint>(2, 8, 64)), mb);

        foreach (var set in mb.As())
        {
            set.AsEnumerable().Select(x => x.Value).Should<int>().BeInDescendingOrder();
        }
    }

    [Fact]
    public void SetSuccAndFailIsFailedSet()
    {
        var ma = Set(Fail<Error, int>(Error.New("failed")), Success<Error, int>(12));
        var mb = ma.Traverse(x => x);
        Assert.Equal(Fail<Error, Set<int>>(Error.New("failed")), mb);
    }
}
