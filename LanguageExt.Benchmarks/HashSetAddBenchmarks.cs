﻿using System.Collections.Generic;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using static LanguageExt.Prelude;

namespace LanguageExt.Benchmarks
{
    [RPlotExporter, RankColumn]
    [GenericTypeArguments(typeof(int))]
    [GenericTypeArguments(typeof(string))]
    [MemoryDiagnoser(false)]
    public class HashSetAddBenchmarks<T>
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
        public ImmutableHashSet<T> SysColImmutableHashSet()
        {
            var set = ImmutableHashSet.Create<T>();
            foreach (var value in values)
            {
                set = set.Add(value);
            }

            return set;
        }

        [Benchmark]
        public ImmutableHashSet<T> SysColImmutableHashSetWithBuilder()
        {
            var set = ImmutableHashSet.CreateBuilder<T>();
            foreach (var value in values)
            {
                set.Add(value);
            }

            return set.ToImmutable();
        }

        [Benchmark]
        public ImmutableSortedSet<T> SysColImmutableSortedSet()
        {
            var set = ImmutableSortedSet.Create<T>();
            foreach (var value in values)
            {
                set = set.Add(value);
            }

            return set;
        }

        [Benchmark]
        public ImmutableSortedSet<T> SysColImmutableSortedSetWithBuilder()
        {
            var set = ImmutableSortedSet.CreateBuilder<T>();
            foreach (var value in values)
            {
                set.Add(value);
            }

            return set.ToImmutable();
        }

        [Benchmark]
        public System.Collections.Generic.HashSet<T> SysColHashSet()
        {
            var set = new System.Collections.Generic.HashSet<T>();
            foreach (var value in values)
            {
                set.Add(value);
            }

            return set;
        }

        [Benchmark]
        public HashSet<T> LangExtHashSet()
        {
            var set = HashSet<T>(EqualityComparer<T>.Default);
            foreach (var value in values)
            {
                set = set.Add(value);
            }

            return set;
        }

        [Benchmark]
        public Set<T> LangExtSet()
        {
            var set = Set<T>(Comparer<T>.Default);
            foreach (var value in values)
            {
                set = set.Add(value);
            }

            return set;
        }
    }
}
