using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;

namespace LanguageExt.Benchmarks
{
    [RPlotExporter, RankColumn]
    [GenericTypeArguments(typeof(int))]
    [GenericTypeArguments(typeof(string))]
    public class HashSetContainsBenchmarks<T>
    {
        [Params(100, 1000, 10000, 100000)]
        public int N;

        T[] values;

        ImmutableHashSet<T> immutableHashSet;
        ImmutableSortedSet<T> immutableSortedSet;
        System.Collections.Generic.HashSet<T> sysHashSet;
        HashSet<T> hashSet;
        Set<T> set;

        [GlobalSetup]
        public void Setup()
        {
            values = ValuesGenerator.Default.GenerateUniqueValues<T>(N);

            immutableHashSet = ValuesGenerator.SysColImmutableHashSetSetup(values);
            immutableSortedSet = ValuesGenerator.SysColImmutableSortedSetSetup(values);
            sysHashSet = ValuesGenerator.SysColHashSetSetup(values);
            hashSet = ValuesGenerator.LangExtHashSetSetup(values);
            set = ValuesGenerator.LangExtSetSetup(values);
        }

        [Benchmark]
        public bool SysColImmutableHashSet()
        {
            var result = true;
            foreach (var value in values)
            {
                result &= immutableHashSet.Contains(value);
            }

            return result;
        }

        [Benchmark]
        public bool SysColImmutableSortedSet()
        {
            var result = true;
            foreach (var value in values)
            {
                result &= immutableSortedSet.Contains(value);
            }

            return result;
        }

        [Benchmark]
        public bool SysColHashSet()
        {
            var result = true;
            foreach (var value in values)
            {
                result &= sysHashSet.Contains(value);
            }

            return result;
        }

        [Benchmark]
        public bool LangExtHashSet()
        {
            var result = true;
            foreach (var value in values)
            {
                result &= hashSet.Contains(value);
            }

            return result;
        }

        [Benchmark]
        public bool LangExtSet()
        {
            var result = true;
            foreach (var value in values)
            {
                result &= set.Contains(value);
            }

            return result;
        }
    }
}
