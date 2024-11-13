using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Sasa.Collections;

namespace LanguageExt.Benchmarks
{
    [RPlotExporter, RankColumn]
    [GenericTypeArguments(typeof(int))]
    [GenericTypeArguments(typeof(string))]
    public class HashMapContainsKeyBenchmarks<T>
    {
        [Params(100, 1000, 10000, 100000)]
        public int N;

        T[] keys;

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
            keys = values.Keys.ToArray();

            sasaTrie = ValuesGenerator.SasaTrieSetup(values);
            immutableMap = ValuesGenerator.SysColImmutableDictionarySetup(values);
            immutableSortedMap = ValuesGenerator.SysColImmutableSortedDictionarySetup(values);
            dictionary = ValuesGenerator.SysColDictionarySetup(values);
            hashMap = ValuesGenerator.LangExtHashMapSetup(values);
            map = ValuesGenerator.LangExtMapSetup(values);
        }

        [Benchmark]
        public bool SysColImmutableDictionary()
        {
            var result = true;
            foreach (var key in keys)
            {
                result &= immutableMap.ContainsKey(key);
            }

            return result;
        }
        
        [Benchmark]
        public bool SasaTrie()
        {
            var result = true;
            foreach (var key in keys)
            {
                result &= sasaTrie.ContainsKey(key);
            }

            return result;
        }

        [Benchmark]
        public bool SysColImmutableSortedDictionary()
        {
            var result = true;
            foreach (var key in keys)
            {
                result &= immutableSortedMap.ContainsKey(key);
            }

            return result;
        }

        [Benchmark]
        public bool SysColDictionary()
        {
            var result = true;
            foreach (var key in keys)
            {
                result &= dictionary.ContainsKey(key);
            }

            return result;
        }

        [Benchmark]
        public bool LangExtHashMap()
        {
            var result = true;
            foreach (var key in keys)
            {
                result &= hashMap.ContainsKey(key);
            }

            return result;
        }

        [Benchmark]
        public bool LangExtMap()
        {
            var result = true;
            foreach (var key in keys)
            {
                result &= map.ContainsKey(key);
            }

            return result;
        }
    }
}
