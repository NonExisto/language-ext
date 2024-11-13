﻿using System.Collections.Generic;
using System.Collections.Immutable;
using Sasa.Collections;
using static LanguageExt.Prelude;

namespace LanguageExt.Benchmarks
{
    internal partial class ValuesGenerator
    {
        public static Trie<T, T> SasaTrieSetup<T>(Dictionary<T, T> values)
        {
            var trie = Trie<T, T>.Empty;
            foreach (var kvp in values)
            {
                trie = trie.Add(kvp.Key, kvp.Value);
            }
            return trie;
        }
        
        public static ImmutableDictionary<T, T> SysColImmutableDictionarySetup<T>(Dictionary<T, T> values)
        {
            var immutableMap = ImmutableDictionary.Create<T, T>();
            foreach (var kvp in values)
            {
                immutableMap = immutableMap.Add(kvp.Key, kvp.Value);
            }

            return immutableMap;
        }

        public static ImmutableSortedDictionary<T, T> SysColImmutableSortedDictionarySetup<T>(Dictionary<T, T> values)
        {
            var immutableMap = ImmutableSortedDictionary.Create<T, T>();
            foreach (var kvp in values)
            {
                immutableMap = immutableMap.Add(kvp.Key, kvp.Value);
            }

            return immutableMap;
        }

        public static Dictionary<T, T> SysColDictionarySetup<T>(Dictionary<T, T> values)
        {
            var dictionary = new Dictionary<T, T>();
            foreach (var kvp in values)
            {
                dictionary.Add(kvp.Key, kvp.Value);
            }

            return dictionary;
        }

        public static HashMap<T, T> LangExtHashMapSetup<T>(Dictionary<T, T> values)
        {
            var hashMap = HashMap<T, T>(EqualityComparer<T>.Default);
            foreach (var kvp in values)
            {
                hashMap = hashMap.Add(kvp.Key, kvp.Value);
            }

            return hashMap;
        }

        public static Map<T, T> LangExtMapSetup<T>(Dictionary<T, T> values)
        {
            var hashMap = Map<T, T>(Comparer<T>.Default);
            foreach (var kvp in values)
            {
                hashMap = hashMap.Add(kvp.Key, kvp.Value);
            }

            return hashMap;
        }
    }
}
