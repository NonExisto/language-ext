using System;
using System.Linq;
using System.Reactive.Linq;
using BenchmarkDotNet.Attributes;

namespace LanguageExt.Benchmarks;

[RPlotExporter, RankColumn]
[MemoryDiagnoser(false)]
public class SteamTBenchmarks
{
	[Params(10, 50, 100)]
	public int N;

	[Benchmark]
	public int EnumerableLast()
	{
		return Enumerable.Range(-N/2, N/2).Last();
	}

	[Benchmark]
	public int ObservableLast()
	{
		return Enumerable.Range(-N/2, N/2).ToObservable().LastAsync().Wait();
	}

	[Benchmark]
	public int IterableLast()
	{
		return Prelude.Range(-N/2, N/2).ToIterable().Last().Match(x=> x, -1);
	}

	[Benchmark]
	public int StreamTLast()
	{
		int counter = 0;
		return (
			from v in Prelude.Range(-N/2, N/2).AsStream<IO>()
			where counter++ == N
			select v
		).Run().Run().Match(x => x.Head, -1);
	}
}
