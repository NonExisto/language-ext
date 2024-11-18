using System;
using FluentAssertions;
using LanguageExt.Common;
using Xunit;

namespace LanguageExt.Tests;

public class FinTests
{
		[Fact]
    public void FinShouldBeTrue()
    {
        var success = FinSucc(42);
        var failure = FinFail<int>(Errors.Cancelled);
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
    public void FinShouldBeFalse()
    {
        var success = FinSucc(42);
        var failure = FinFail<int>(Errors.Cancelled);
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

    private static Fin<int> Fail() => throw new InvalidOperationException("Should not happen");
}
