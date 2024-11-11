using Xunit;

namespace LanguageExt.Tests.FSharp;

public class FSharpTests
{
    [Theory]
    [InlineData("Error")]
    [InlineData("")]
    public void ErrorResult_to_Either(string error)
    {
        var result = Microsoft.FSharp.Core.FSharpResult<int, string>.NewError(error);

        var either = LanguageExt.FSharp.fs(result);

        match(either,
              Right: _ => Assert.Fail("Shouldn't get here"),
              Left: l => Assert.True(l == error));
    }

    [Fact]
    public void OKResult_to_Either()
    {
        var result = Microsoft.FSharp.Core.FSharpResult<int, string>.NewOk(123);

        var either = LanguageExt.FSharp.fs(result);

        match(either,
              Right: r => Assert.Equal(123, r),
              Left: _ => Assert.Fail("Shouldn't get here"));
    }

    [Fact]
    public void Either_to_ErrorResult()
    {
        var either = Either<string, int>.Left("Error");

        var result = LanguageExt.FSharp.fs(either);

        Assert.True(result.IsError);
        Assert.Equal("Error", result.ErrorValue);
    }

    [Fact]
    public void Either_to_OkResult()
    {
        var either = Either<string, int>.Right(123);

        var result = LanguageExt.FSharp.fs(either);

        Assert.True(result.IsOk);
        Assert.Equal(123, result.ResultValue);
    }
}
