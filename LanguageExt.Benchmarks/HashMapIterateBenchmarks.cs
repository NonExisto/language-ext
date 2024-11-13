using System.Collections.Generic;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using Sasa.Collections;

namespace LanguageExt.Benchmarks
{
    [RPlotExporter, RankColumn]
    [GenericTypeArguments(typeof(int))]
    [GenericTypeArguments(typeof(string))]
    public class HashMapIterateBenchmarks<T>
    {
        [Params(100, 1000, 10000, 100000)]
        public int N;

        ImmutableDictionary<T, T> immutableMap;
        ImmutableSortedDictionary<T, T> immutableSortedMap;
        Dictionary<T, T> dictionary;
        HashMap<T, T> hashMap;
        Map<T, T> map;
        Trie<T, T> sasaTrie;

        [GlobalSetup]
        public void Setup()
        {
            var values = ValuesGenerator.Default.GenerateDictionary<T, T>(N);

            sasaTrie = ValuesGenerator.SasaTrieSetup(values);
            immutableMap = ValuesGenerator.SysColImmutableDictionarySetup(values);
            immutableSortedMap = ValuesGenerator.SysColImmutableSortedDictionarySetup(values);
            dictionary = ValuesGenerator.SysColDictionarySetup(values);
            hashMap = ValuesGenerator.LangExtHashMapSetup(values);
            map = ValuesGenerator.LangExtMapSetup(values);
        }

        [Benchmark]
        public T SysColImmutableDictionary()
        {
            T result = default;

            var collection = immutableMap;
            foreach (var item in collection)
            {
                result = item.Value;
            }

            return result;
        }

        [Benchmark]
        public T SysColImmutableSortedDictionary()
        {
            T result = default;

            var collection = immutableSortedMap;
            foreach (var item in collection)
            {
                result = item.Value;
            }

            return result;
        }
        
        [Benchmark]
        public T SasaTrie()
        {
            T result = default;

            var collection = sasaTrie;
            foreach (var item in collection)
            {
                result = item.Value;
            }

            return result;
        }

        [Benchmark]
        public T SysColDictionary()
        {
            T result = default;

            var collection = dictionary;
            foreach (var item in collection)
            {
                result = item.Value;
            }

            return result;
        }

        [Benchmark]
        public T LangExtHashMap()
        {
            T result = default;

            var collection = hashMap;
            foreach (var item in collection)
            {
                result = item.Value;
            }

            return result;
        }

        [Benchmark]
        public T LangExtMap()
        {
            T result = default;

            var collection = map;
            foreach (var item in collection)
            {
                result = item.Value;
            }

            return result;
        }
    }
}
