using FluentAssertions;
using LanguageExt.ClassInstances;
using Xunit;

namespace LanguageExt.Tests.TraitTests.ClassInstances;

public class TCharTests
{
	[Theory]
	[InlineData('A')]
	[InlineData('Ã')]
	[InlineData('Φ')]
	[InlineData('Ճ')]
	public void TCharTryUpperShouldIgnoreCapitalLetters(char testee)
	{
		TChar.TryUpper(testee).Should().Be(testee);
	}

	[Theory]
	[InlineData('ଇ')]
	[InlineData('⋘')]
	[InlineData('⍖')]
	public void TCharTryUpperShouldIgnoreLettersHavingNoCasing(char testee)
	{
		TChar.TryUpper(testee).Should().Be(testee);
	}

	[Theory]
	[InlineData('2')]
	[InlineData('5')]
	[InlineData('8')]
	public void TCharTryUpperShouldIgnoreDigits(char testee)
	{
		TChar.TryUpper(testee).Should().Be(testee);
	}

	[Theory]
	[InlineData('\ud83d')]
	[InlineData('\udd2e')]
	public void TCharTryUpperShouldIgnoreSurrogates(char testee)
	{
		TChar.TryUpper(testee).Should().Be(testee);
	}

	[Theory]
	[InlineData('a', 'A')]
	[InlineData('ϐ', 'Β')]
	[InlineData('й', 'Й')]
	public void TCharTryUpperShouldProcessMostCommonLetters(char testee, char expected)
	{
		TChar.TryUpper(testee).Should().Be(expected);
	}
}
