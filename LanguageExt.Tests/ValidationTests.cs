﻿using System;
using FluentAssertions;
using LanguageExt.Common;
using Xunit;

namespace LanguageExt.Tests;

public class ValidationTests
{
    public Func<string, string> ToUpper => x => x.ToUpper();

    [Fact]
    public void ValidationSeq_MapFails_Failure()
    {
        var failure = Fail<Seq<string>, int>(["something went wrong"])
                          .MapFail(xs => xs.Map(ToUpper));

        failure.Match(
            Succ: _ => Assert.Fail("should never get here"),
            Fail: errors =>
                  {
                      Assert.Equal(1, errors.Count);
                      Assert.Equal("SOMETHING WENT WRONG", errors.Head);
                  });
    }

    [Fact]
    public void ValidationSeq_MapFails_Success()
    {
        var success = Success<Seq<string>, int>(42)
                         .MapFail(xs => xs.Map(ToUpper));

        success.Match(
            Succ: succ => Assert.Equal(42, succ),
            Fail: errors => Assert.Fail("should never get here"));
    }

    [Fact]
    public void ValidationSeq_BiMap_Failure()
    {
        var failure = Fail<Seq<string>, int>(["something went wrong"])
                        .BiMap(Succ: succ => succ + 1,
                               Fail: xs => xs.Map(ToUpper));

        failure.Match(
            Succ: _ => Assert.Fail("should never get here"),
            Fail: errors =>
                  {
                      Assert.Equal(1, errors.Count);
                      Assert.Equal("SOMETHING WENT WRONG", errors.Head);
                  });
    }

    [Fact]
    public void ValidationSeq_BiMap_Success()
    {
        var success = Success<Seq<string>, int>(42)
           .BiMap(Succ: succ => succ + 1,
                  Fail: xs => xs.Map(ToUpper));

        success.Match(
            Succ: succ => Assert.Equal(43, succ),
            Fail: err => Assert.Fail("should never get here"));
    }

    [Fact]
    public void ValidCreditCardTest()
    {
        // Valid test
        var res = ValidateCreditCard("Paul", "1234567891012345", "10", "2020");

        res.Match(
            Succ: cc =>
                  {
                      Assert.Equal("Paul", cc.CardHolder);
                      Assert.Equal(10, cc.Month);
                      Assert.Equal(2020, cc.Year);
                      Assert.Equal("1234567891012345", cc.Number);
                  },
            Fail: err => Assert.Fail("should never get here"));
    }

    [Fact]
    public void InValidCreditCardNumberTest()
    {
        var res = ValidateCreditCard("Paul", "ABCDEF567891012345", "10", "2020");
        Assert.True(res.IsFail);

        res.Match(
            Succ: _ => Assert.Fail("should never get here"),
            Fail: errors =>
                  {
                      Assert.Equal(2, errors.Count);
                      Assert.Equal("only numbers are allowed", errors.Head.Message);
                      Assert.Equal("can not exceed 16 characters", errors.Tail.Head.Message);
                  });
    }

    [Fact]
    public void ExpiredAndInValidCreditCardNumberTest()
    {
        var res = ValidateCreditCard("Paul", "ABCDEF567891012345", "1", "2001");
        Assert.True(res.IsFail);

        res.Match(
            Succ: _ => Assert.Fail("should never get here"),
            Fail: errors =>
                  {
                      Assert.Equal(3, errors.Count);
                      Assert.Equal("only numbers are allowed", errors.Head.Message);
                      Assert.Equal("can not exceed 16 characters", errors.Tail.Head.Message);
                      Assert.True(errors.Tail.Tail.Head.Message == "card has expired");
                  });
    }

    [Fact]
    public void ValidationShouldBeTrue()
    {
        var success = Success<Seq<string>, int>(42);
        var failure = Fail<Seq<string>, int>(["something went wrong"]);
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
    public void OptionShouldBeFalse()
    {
        var success = Success<Seq<string>, int>(42);
        var failure = Fail<Seq<string>, int>(["something went wrong"]);
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

    private static Validation<Seq<string>, int> Fail() => throw new InvalidOperationException("Should not happen");

    /// <summary>
    /// Validates the string has only ASCII characters
    /// </summary>
    public static Validation<Error, string> AsciiOnly(string str) =>
        str.AsIterable().ForAll(c => c <= 0x7f)
            ? Success<Error, string>(str)
            : Fail<Error, string>(Error.New("only ascii characters are allowed"));

    /// <summary>
    /// Creates a delegate that when passed a string will validate that it's below
    /// a specific length
    /// </summary>
    public static Func<string, Validation<Error, string>> MaxStrLength(int max) =>
        str =>
            str.Length <= max
                ? Success<Error, string>(str)
                : Fail<Error, string>(Error.New($"can not exceed {max} characters"));

    /// <summary>
    /// Validates that the string passed contains only digits
    /// </summary>
    public static Validation<Error, string> DigitsOnly(string str) =>
        str.AsIterable().ForAll(char.IsDigit)
            ? Success<Error, string>(str)
            : Fail<Error, string>(Error.New($"only numbers are allowed"));

    /// <summary>
    /// Uses parseInt which returns an Option and converts it to a Validation
    /// value with a default Error if the parse fails
    /// </summary>
    public static Validation<Error, int> ToInt(string str) =>
        parseInt(str).ToValidation(Error.New("must be a number"));

    /// <summary>
    /// Validates that the value passed is a month
    /// </summary>
    public static Validation<Error, int> ValidMonth(int month) =>
        month >= 1 && month <= 12
            ? Success<Error, int>(month)
            : Fail<Error, int>(Error.New($"invalid month"));

    /// <summary>
    /// Validates that the value passed is a positive number
    /// </summary>
    public static Validation<Error, int> PositiveNumber(int value) =>
        value > 0
            ? Success<Error, int>(value)
            : Fail<Error, int>(Error.New($"must be positive"));

    /// <summary>
    /// Takes todays date and builds a delegate that can take a month and year
    /// to see if the credit card has expired.
    /// </summary>
    public static Func<int, int, Validation<Error, (int month, int year)>> ValidExpiration(int currentMonth, int currentYear) =>
        (month, year) =>
            year > currentYear || (year == currentYear && month >= currentMonth)
                ? Success<Error, (int, int)>((month, year))
                : Fail<Error, (int, int)>(Error.New($"card has expired"));

    /// <summary>
    /// Validate that the card holder is ASCII and has a maximum of 30 characters
    /// This uses the | operator as a disjunction computation.  If any items are
    /// Failed then the errors are collected and returned.  If they all pass then
    /// the Success value from the first item is propagated.  This only works when
    /// all the operands are of the same type and you only care about the first
    /// success value.  Which in this case is cardHolder for both.
    /// </summary>
    public static Validation<Error, string> ValidateCardHolder(string cardHolder) =>
        AsciiOnly(cardHolder) | MaxStrLength(30)(cardHolder);

    /// <summary>
    /// This is the main validation function for validating a credit card
    /// </summary>
    public static Validation<Error, CreditCard> ValidateCreditCard(string cardHolder, string number, string expMonth, string expYear)
    {
        var fakeDateTime = new DateTime(year: 2019, month: 1, day: 1);
        var cardHolderV  = ValidateCardHolder(cardHolder);
        var numberV      = DigitsOnly(number) + MaxStrLength(16)(number);
        var validToday   = ValidExpiration(fakeDateTime.Month, fakeDateTime.Year);

        // This falls back to monadic behaviour because validToday needs both
        // a month and year to continue.  
        var monthYear = from m in ToInt(expMonth).Bind(ValidMonth)
                        from y in ToInt(expYear).Bind(PositiveNumber)
                        from my in validToday(m, y)
                        select my;

        // The items to validate are placed in a tuple, then you call apply to
        // confirm that all items have passed the validation.  If not then all
        // the errors are collected.  If they have passed then the results are
        // passed to the lambda function allowing the creation of the
        // CreditCard object.
        return (cardHolderV, numberV, monthYear).Apply((c, num, my) => new CreditCard(c, num, my.month, my.year)).As();
    }

    public class CreditCard
    {
        public readonly string CardHolder;
        public readonly string Number;
        public readonly int Month;
        public readonly int Year;

        public CreditCard(string c, string num, int month, int year)
        {
            CardHolder = c;
            Number = num;
            Month = month;
            Year = year;
        }
    }
}
