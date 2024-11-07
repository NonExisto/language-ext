using System;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace LanguageExt.Benchmarks;

[RPlotExporter, RankColumn]
[GenericTypeArguments(typeof(int))]
[GenericTypeArguments(typeof(string))]
[MemoryDiagnoser(false)]
public class EnumerableOptimalBenchmarks<T>
{
	[Params(100, 1000, 10000, 100000)]
	public int N;

	T[] values;

	[GlobalSetup]
	public void Setup()
	{
		values = ValuesGenerator.Default.GenerateUniqueValues<T>(N);
	}

	[Benchmark]
	public T SysLinq()
	{
		T result = default;
		var collection = values.Concat(values.Take(N/2)).Concat(values.Skip(N/2));
		foreach (var value in collection)
		{
			result = value;
		}

		return result;
	}

	[Benchmark]
	public T Optimal()
	{
		T result = default;
		var collection = values.ConcatFast(values.Take(N/2)).ConcatFast(values.Skip(N/2));
		foreach (var value in collection)
		{
			result = value;
		}

		return result;
	}
}
