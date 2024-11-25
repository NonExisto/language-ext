using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Tr = LanguageExt.Trampoline<long>;

namespace LanguageExt.Benchmarks;

[RPlotExporter, RankColumn, MarkdownExporterAttribute.GitHub]
[MemoryDiagnoser(false)]
public class FibonacciBenchmark
{
	[Params(10, 50, 100)]
	public int N;

	[Benchmark]
	public long EnumerableFib()
	{
		return Schedule.fibonacci(1).Run().Take(N).Last().Match(x => (long)x.Milliseconds, 0);
	}

	[Benchmark]
	public long RecursiveFib()
	{
		Dictionary<int, long> cache = new(N - 2);
		return fib(N, cache);
		static long fib(int n, Dictionary<int, long> cache)
		{
			return n switch
			{
				1 => 1,
				2 => 1,
				_ => cache.TryGetValue(n, out var x) ? x : CacheIt(n, fib(n - 1, cache) + fib(n - 2, cache), cache),
			};
		}

	}

	[Benchmark]
	public long TrampolineFib()
	{
		Dictionary<int, long> cache = new(N - 2);
		return fib(N, cache).Run();
		static Tr fib(int n, Dictionary<int, long> cache)
		{
			return n switch
			{
				1 => Trampoline.Pure(1L),
				2 => Trampoline.Pure(1L),
				_ => resolve(n, cache),
			};
		}

		static Tr resolve(int n, Dictionary<int, long> cache)
		{
			if (cache.TryGetValue(n, out var x))
			{
				return Trampoline.Pure(x);
			}
			return from p1 in fib(n - 1, cache)
						 from p2 in fib(n - 2, cache)
						 select CacheIt(n, p1 + p2, cache);
		}
	}

	static long CacheIt(int n, long value, Dictionary<int, long> cache)
	{
		cache[n] = value;
		return value;
	}
}
