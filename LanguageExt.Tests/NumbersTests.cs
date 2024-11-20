using System.Linq;
using FluentAssertions;
using Xunit;
using Rint = LanguageExt.Tests.ReverseNumber<int>;

namespace LanguageExt.Tests;

public class NumbersTests
{
	[Theory]
	[InlineData(100)]
	[InlineData(1000002)]
	[InlineData(-10002)]
	[InlineData(0)]
	public void RintShouldProduceRevertHashCode(int testValue)
	{
		testValue.GetHashCode().Should().Be(testValue);
		Rint v = new(testValue);
		v.GetHashCode().Should().Be(-testValue);
	}

	[Fact]
	public void RintShouldBeOrderedDescending()
	{
		var arr = Enumerable.Range(0, 100).Select(x => new Rint(x)).ToArray();
		System.Array.Sort(arr);
		arr.Select(x => x.Value).Should().BeInDescendingOrder();
	}
}
