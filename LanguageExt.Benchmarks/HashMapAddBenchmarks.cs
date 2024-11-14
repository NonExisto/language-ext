using System.Collections.Generic;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using Sasa.Collections;
using static LanguageExt.Prelude;

namespace LanguageExt.Benchmarks
{
    [RPlotExporter, RankColumn]
    [GenericTypeArguments(typeof(int))]
    [GenericTypeArguments(typeof(string))]
    [MemoryDiagnoser(false)]
    public class HashMapAddBenchmarks<T>
    {
        [Params(100, 1000, 10000, 100000)]
        public int N;

        Dictionary<T, T> values;

        [GlobalSetup]
        public void Setup() => values = ValuesGenerator.Default.GenerateDictionary<T, T>(N);

        [Benchmark]
        public ImmutableDictionary<T, T> SysColImmutableDictionary()
        {
            var map = ImmutableDictionary.Create<T, T>();
            foreach (var kvp in values)
            {
                map = map.Add(kvp.Key, kvp.Value);
            }

            return map;
        }

         [Benchmark]
        public ImmutableDictionary<T, T> SysColImmutableDictionaryWithBuilder()
        {
            var map = ImmutableDictionary.CreateBuilder<T, T>();
            foreach (var kvp in values)
            {
                map.Add(kvp.Key, kvp.Value);
            }

            return map.ToImmutable();
        }

        [Benchmark]
        public ImmutableSortedDictionary<T, T> SysColImmutableSortedDictionary()
        {
            var map = ImmutableSortedDictionary.Create<T, T>();
            foreach (var kvp in values)
            {
                map = map.Add(kvp.Key, kvp.Value);
            }

            return map;
        }

        [Benchmark]
        public ImmutableSortedDictionary<T, T> SysColImmutableSortedDictionaryWithBuilder()
        {
            var map = ImmutableSortedDictionary.CreateBuilder<T, T>();
            foreach (var kvp in values)
            {
                map.Add(kvp.Key, kvp.Value);
            }

            return map.ToImmutable();
        }
        
        [Benchmark]
        public Trie<T, T> SasaTrie()
        {
            var map = Trie<T, T>.Empty;
            foreach (var kvp in values)
            {
                map = map.Add(kvp.Key, kvp.Value);
            }

            return map;
        }
        
        [Benchmark]
        public Dictionary<T, T> SysColDictionary()
        {
            var map = new Dictionary<T, T>();
            foreach (var kvp in values)
            {
                map.Add(kvp.Key, kvp.Value);
            }

            return map;
        }

        [Benchmark]
        public HashMap<T, T> LangExtHashMap()
        {
            var map = HashMap<T, T>(EqualityComparer<T>.Default);
            foreach (var kvp in values)
            {
                map = map.Add(kvp.Key, kvp.Value);
            }

            return map;
        }

        [Benchmark]
        public Map<T, T> LangExtMap()
        {
            var map = Map<T, T>(Comparer<T>.Default);
            foreach (var kvp in values)
            {
                map = map.Add(kvp.Key, kvp.Value);
            }

            return map;
        }
    }
}
