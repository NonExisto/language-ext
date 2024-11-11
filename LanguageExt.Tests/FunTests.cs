using FluentAssertions;
using Xunit;

namespace LanguageExt.Tests;

public class FunTests
{
    [Fact] public void LambdaInferTests()
    {
        var fn1 = fun( () => 123 );
        var fn2 = fun( (int a) => 123                                           + a );
        var fn3 = fun( (int a, int b) => 123                                    + a + b );
        var fn4 = fun( (int a, int b, int c) => 123                             + a + b + c);
        var fn5 = fun( (int a, int b, int c, int d) => 123                      + a + b + c + d);
        var fn6 = fun( (int a, int b, int c, int d, int e) => 123               + a + b + c + d + e);
        var fn7 = fun( (int a, int b, int c, int d, int e, int f) => 123        + a + b + c + d + e + f);
        var fn8 = fun( (int a, int b, int c, int d, int e, int f, int g) => 123 + a + b + c + d + e + f + g);

        var fnac1 = fun( () => { } );
        var fnac2 = fun( (int a) => Consume(123 + a) );
        var fnac3 = fun( (int a, int b) => Consume(123 + a + b));
        var fnac4 = fun( (int a, int b, int c) => Consume(123 + a + b + c));
        var fnac5 = fun( (int a, int b, int c, int d) => Consume(123 + a + b + c + d));
        var fnac6 = fun( (int a, int b, int c, int d, int e) => Consume(123 + a + b + c + d + e));
        var fnac7 = fun( (int a, int b, int c, int d, int e, int f) => Consume(123 + a + b + c + d + e + f));
        var fnac8 = fun( (int a, int b, int c, int d, int e, int f, int g) => Consume(123 + a + b + c + d + e + f + g));

        var ac1 = act(() => { });
        var ac2 = act((int a) => Consume(123 + a));
        var ac3 = act((int a, int b) => Consume(123 + a + b));
        var ac4 = act((int a, int b, int c) => Consume(123 + a + b + c));
        var ac5 = act((int a, int b, int c, int d) => Consume(123 + a + b + c + d));
        var ac6 = act((int a, int b, int c, int d, int e) => Consume(123 + a + b + c + d + e));
        var ac7 = act((int a, int b, int c, int d, int e, int f) => Consume(123 + a + b + c + d + e + f));
        var ac8 = act((int a, int b, int c, int d, int e, int f, int g) => Consume(123 + a + b + c + d + e + f + g));
    }

    private static void Consume(int number) => number.Should().BePositive();
}
