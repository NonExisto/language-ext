using System.Collections.Concurrent;

namespace LanguageExt
{
    /// <summary>
    /// Type class for newing an object
    /// </summary>
    internal interface New<A>
    {
        static abstract A Create();
    }

    /// <summary>
    /// Thread-safe pooling 
    /// Manages a concurrent stack of values that will grow as needed
    /// When spent new objects are allocated used the `New&lt;A&gt;` trait
    /// </summary>
    internal static class Pool<NewA, A> where NewA : New<A>
    {
        static readonly ConcurrentStack<A> stack = new();

        public static A Pop() =>
            stack.TryPop(out A? var)
                ? var
                : NewA.Create();

        public static void Push(A value) =>
            stack.Push(value);
    }
}
