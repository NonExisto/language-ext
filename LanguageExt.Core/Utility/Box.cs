using System;

namespace LanguageExt
{
    /// <summary>
    /// Represents a boxed value, if and only if `A` is a `struct`.  This can be used
    /// to stop unnecessary allocations when using generic types cast to `object`: the 
    /// Box isn't allocated for reference types and is for value-types.  Then access
    /// is transparent via `GetValue`.
    /// </summary>
    internal sealed class Box<A>
    {
        private readonly A Value;

        private Box(A value) =>
            Value = value;

        public static readonly Func<A, object> New;
        public static readonly Func<object, A> GetValue;

        static Box()
        {
            var isValueType = typeof(A).IsValueType;
            New = isValueType
                ? Box<A>.MakeNewStruct()
                : Box<A>.MakeNewClass();

            GetValue = isValueType
                ? Box<A>.GetValueStruct()
                : Box<A>.GetValueClass();
        }

        static Func<object, A> GetValueClass() => x => (A)x;

        static Func<object, A> GetValueStruct() => (object x) => ((Box<A>)x).Value;

        static Func<A, object> MakeNewClass() => static (A x) => x!;

        static Func<A, object> MakeNewStruct() => static (A x) => new Box<A>(x);
    }
}
