using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LanguageExt.ClassInstances;

namespace LanguageExt.Tests;

public static class Comparers
{
		public static IEqualityComparer<int> IntComparer => _Int.Default;
		private sealed class _Int : IEqualityComparer<int>
    {
        public static readonly _Int Default = new _Int();
        public bool Equals(int x, int y) => EqInt.Equals(x,y);
        public int GetHashCode([DisallowNull] int obj) => EqInt.GetHashCode(obj);
    }
}
