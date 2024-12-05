using FluentAssertions;
using Xunit;

namespace LanguageExt.Tests;

public class OptionNullableTests
{
	[Fact]
	public void BimapShouldNotProduceNullable()
	{
		var optional = Some(123);

		var result = optional.BiMap(x => (int?)null, x => null);
		result.Should<Option<int>>().Be(None);

		result = optional.BiMap(x => (int?)null, () => null);
		result.Should<Option<int>>().Be(None);
	}

	[Fact]
	public void MapShouldNotProduceNullable()
	{
		var optional = Some(123);

		var result = optional.Map(x => (int?)null);
		result.Should<Option<int>>().Be(None);
	}

	[Fact]
	public void SelectShouldNotProduceNullable()
	{
		var optional = Some(123);

		var result = optional.Select(x => (int?)null);
		result.Should<Option<int>>().Be(None);
	}

	[Fact]
	public void SelectManyShouldNotProduceNullable()
	{
		var result = from s in Some(123)
								 from x in Some(321)
								 select (int?)null;
		result.Should<Option<int>>().Be(None);

		result = from s in Some(123)
						 from x in Some(321)
						 let y = x + s
						 select (int?)null;
		result.Should<Option<int>>().Be(None);

		result = Some(123).SelectMany(x => Some(123), (x, y) => (int?)null);
		result.Should<Option<int>>().Be(None);
	}

	[Fact]
	public void PureBindShouldNotProduceNullable()
	{
		var optional = Some(123);

		var result = optional.Bind(x => Pure((int?)null));
		result.Should<Option<int>>().Be(None);
	}

	[Fact]
	public void ImplicitCastShouldNotProduceNullable()
	{
		Option<int?> result = (int?)null; // And yet it does. Guess only static analysis rule could prevent that.

		result.Should<Option<int?>>().Be(None); // At least we secure conversion result
		result.Count().Should().Be(0);
		
		result = 2;
		result.Should<Option<int?>>().Be(2);
	}

	[Fact]
	public void ImplicitCastShouldProduceNoneForNullReference()
	{
		Option<string> result = (string?)null;

		result.Should<Option<string>>().Be(None);
	}
}
