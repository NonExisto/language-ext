using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using BenchmarkDotNet.Attributes;
using static LanguageExt.Prelude;

namespace LanguageExt.Benchmarks
{
    [RPlotExporter, RankColumn]
    [GenericTypeArguments(typeof(int))]
    [GenericTypeArguments(typeof(string))]
    [MemoryDiagnoser(false)]
    public class ListAddBenchmarks<T>
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
        public List<T> SysColList()
        {
            var collection = new List<T>();
            foreach (var value in values)
            {
                collection.Add(value);
            }

            return collection;
        }

        [Benchmark]
        public IList<T> SysColListWrap()
        {
            IList<T> collection = new Collection<T>();
            foreach (var value in values)
            {
                collection.Add(value);
            }

            return collection;
        }

        [Benchmark]
        public ImmutableList<T> SysColImmutableList()
        {
            var collection = ImmutableList.Create<T>();
            foreach (var value in values)
            {
                collection = collection.Add(value);
            }

            return collection;
        }

        [Benchmark]
        public ImmutableList<T> SysColImmutableListWithBuilder()
        {
            var builder = ImmutableList.CreateBuilder<T>();
            foreach (var value in values)
            {
                builder.Add(value);
            }

            return builder.ToImmutable();
        }

        [Benchmark]
        public Lst<T> LangExtLst()
        {
            var collection = List<T>();
            foreach (var value in values)
            {
                collection = collection.Add(value);
            }

            return collection;
        }

        [Benchmark]
        public Seq<T> LangExtSeq()
        {
            var collection = Seq<T>();
            foreach (var value in values)
            {
                collection = collection.Add(value);
            }

            return collection;
        }
    }
}
