using Xunit;
using LanguageExt.ClassInstances;


namespace LanguageExt.Tests;

public class Divisible
{
    [Fact]
    public void OptionalNumericDivide()
    {
        var x = Some(20);
        var y = Some(10);
        var z = divide<TInt, int>(x, y);

        Assert.Equal(2, z);
    }
}
