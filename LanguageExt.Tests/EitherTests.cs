using System;
using FluentAssertions;
using LanguageExt.Common;
using Xunit;

namespace LanguageExt.Tests;

public class EitherTests
{
    [Fact] public void RightGeneratorTestsObject()
    {
        Either<string, int> either = Right(123);

        either.Match( Right: i => Assert.Equal(123, i),
                      Left:  _ => Assert.Fail("Shouldn't get here") );

        int c = either.Match( Right: i  => i + 1, 
                              Left: _ => 0 );

        Assert.Equal(124, c);
    }

    [Fact] public void SomeGeneratorTestsFunction()
    {
        var either = Right<string, int>(123);

        match(either, Right: i => Assert.Equal(123, i),
              Left:  _ => Assert.Fail("Shouldn't get here") );

        int c = match(either, Right: i => i + 1,
                      Left:  _ => 0 );

        Assert.Equal(124, c);
    }

    [Fact] public void LeftGeneratorTestsObject()
    {
        var either = ItsLeft;

        either.Match( Right: r => Assert.Fail("Shouldn't get here"),
                      Left:  l => Assert.Equal("Left", l));

        int c = either.Match( Right: r => r + 1, 
                              Left:  l => 0 );

        Assert.Equal(0, c);
    }

    [Fact] public void LeftGeneratorTestsFunction()
    {
        var either = ItsLeft;

        match(either, Right: r => Assert.Fail("Shouldn't get here"),
              Left:  l => Assert.Equal("Left", l));

        int c = match(either, Right: r => r + 1,
                      Left:  l => 0 );

        Assert.Equal(0, c);
    }

    [Fact]
    public void SomeLinqTest() =>
        (from x in Two
         from y in Four
         from z in Six
         select x + y + z)
       .Match(
            Right: r => Assert.Equal(12, r),
            Left:  _ => Assert.Fail("Shouldn't get here"));

    [Fact] public void LeftLinqTest() =>
        (from x in Two
         from y in Four
         from _ in ItsLeft
         from z in Six
         select x + y + z)
       .Match(
            l => Assert.Equal("Left", l),
            _ => Assert.Fail("Shouldn't get here"));

    [Fact] public void EitherFluentSomeNoneTest()
    {
        int res1 = GetValue(true)
                  .Right(r => r + 10)
                  .Left (l => l.Length);

        int res2 = GetValue(false)
                  .Right(r => r + 10)
                  .Left (l => l.Length);

        Assert.Equal(1010, res1);
        Assert.Equal(4, res2);
    }

    private static Either<string, int> GetValue(bool select)
    {
        if (select)
        {
            return 1000;
        }
        else
        {
            return "Left";
        }
    }

    [Fact]
    public void EitherLinqTest1() =>
        (from x in Right(2)
         from _ in Left("error")
         from z in Right(5)
         select x + z)
       .Match(Right: _ => Assert.Fail("Shouldn't get here"),
              Left: _ => Assert.True(true));

    [Fact]
    public void EitherLinqTest2() =>
        (from x in Right(2)
         from y in Right<string, int>(123)
         from z in Right(5)
         select x + y + z)
        .Match(Right: r => Assert.Equal(130, r),
               Left: _ => Assert.True(false));

    [Fact]
    public void EitherCoalesce()
    {
        var x = Right<string, int>(1) || Right<string, int>(2) || Left<string, int>("error");
    }

    [Fact]
    public void EitherInfer1() => AddEithers(Right(10), Left("error"));

    public static Either<string, int> AddEithers(Either<string, int> x, Either<string, int> y) =>
        from a in x
        from b in y
        select a + b;
    
    private static Either<string, int> ItsLeft => "Left";
    private static Either<string, int> Two => 2;
    private static Either<string, int> Four => 4;
    private static Either<string, int> Six => 6;

    [Fact]
    public void EitherShouldBeTrue()
    {
        var success = Right<Error, int>(42);
        var failure = Left<Error, int>(Errors.Cancelled);
        bool switched = false;
        if(success || Fail())
        {
            switched = true;
        }
        switched.Should().BeTrue();

        if(failure || success)
        {
            switched = false;
        }
        switched.Should().BeFalse();

        if(failure || failure)
        {
            switched = true;
        }
        switched.Should().BeFalse();
    }

    [Fact]
    public void EitherShouldBeFalse()
    {
        var success = Right<Error, int>(42);
        var failure = Left<Error, int>(Errors.Cancelled);
        bool switched = false;
        if(failure && Fail())
        {
            switched = true;
        }
        switched.Should().BeFalse();
        
        if(success && failure)
        {
            switched = true;
        }
        switched.Should().BeFalse();

        if(success && success)
        {
            switched = true;
        }
        switched.Should().BeTrue();
    }

    private static Either<Error, int> Fail() => throw new InvalidOperationException("Should not happen");
}
