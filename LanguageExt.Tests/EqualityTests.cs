﻿using Xunit;
using System;
using System.Linq;
using LanguageExt.ClassInstances;

namespace LanguageExt.Tests;

public class EqualityTests
{
    [Fact]
    public void EqualityTest1()
    {
        var optional = Some(123);

        if (optional == 123)
        {
            Assert.True(true);
        }
        else
        {
            Assert.False(true);
        }
    }

    [Fact]
    public void EqualityTest2()
    {
        Option<int> optional = None;

        if (optional == None)
        {
            Assert.True(true);
        }
        else
        {
            Assert.False(true);
        }
    }

    [Fact]
    public void EqualityTest3()
    {
        var optional = Some(123);

        if (optional == None)
        {
            Assert.False(true);
        }
        else
        {
            Assert.True(true);
        }
    }

    [Fact]
    public void EqualityTest4()
    {
        Option<int> optional = None;

        if (optional == Some(123))
        {
            Assert.False(true);
        }
        else
        {
            Assert.True(true);
        }
    }

    [Fact]
    public void NonEqualityTest1()
    {
        var optional = Some(123);

        if (optional != 123)
        {
            Assert.False(true);
        }
        else
        {
            Assert.True(true);
        }
    }

    [Fact]
    public void NonEqualityTest2()
    {
        Option<int> optional = None;

        if (optional != None)
        {
            Assert.False(true);
        }
        else
        {
            Assert.True(true);
        }
    }

    /// <summary>
    /// Test for issue #64
    /// It just needs to complete without throwing an exception to be tested
    /// https://github.com/louthy/language-ext/issues/64
    /// </summary>
    [Fact]
    public void EitherEqualityComparerTest()
    {
        var results = List<Either<Exception, int>>();

        var firsterror = results.FirstOrDefault(i => i.IsLeft);
        if (IsDefault(firsterror)) // <-- here i get exception
        {
        }
    }

    public static bool IsDefault<T>(T obj) =>
        EqDefault<T>.Equals(obj, default);


    [Fact]
    public static void OptionMonadEqualityTests1()
    {
        var optionx = Some(123);
        var optiony = Some(123);

        var optionr = IsEqual<EqInt, Option, int>(optionx, optiony);

        Assert.True(optionr);
        Assert.True(optionx == optiony);
    }

    [Fact]
    public static void OptionMonadEqualityTests2()
    {
        var optionx = Some("ABC");
        var optiony = Some("abc");

        var optionr = IsEqual<EqStringCurrentCultureIgnoreCase, Option, string>(optionx, optiony);

        Assert.True(optionr);

        Assert.True(optionx != optiony);
    }

    [Fact]
    public static void EitherMonadEqualityTests1()
    {
        var optionx = Right<Exception, int>(123);
        var optiony = Right<Exception, int>(123);

        var optionr = IsEqual<EqInt, Either<Exception>, int>(optionx, optiony);
        Assert.True(optionr);

        Assert.True(optionx == optiony);
    }

    [Fact]
    public static void EitherMonadEqualityTests2()
    {
        var optionx = Right<Exception, string>("ABC");
        var optiony = Right<Exception, string>("abc");

        var optionr = IsEqual<EqStringCurrentCultureIgnoreCase, Either<Exception>, string>(optionx, optiony);

        Assert.True(optionr);
        Assert.True(optionx != optiony);
    }

    public static bool IsEqual<EqA, M, A>(K<M, A> mx, K<M, A> my)
        where EqA : Eq<A>
        where M : Monad<M>, Foldable<M> =>
        (from x in mx
         from y in my
         select EqA.Equals(x, y))
        .ForAll(x => x);
}
