using System;
using FluentAssertions;
using LanguageExt.Benchmarks;
using Xunit;

namespace LanguageExt.Tests;

public class FibonacciTest
{
	[Fact]
	public void AllAlgorithmResultsShouldMatch()
	{
		FibonacciBenchmark benchmark = new(){N = 22};
		long iterator = benchmark.EnumerableFib();
		long recursive = benchmark.RecursiveFib();
		long trampoline = benchmark.TrampolineFib();

		iterator.Should().Be(recursive).And.Be(trampoline);
	}
}
