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
    /// Type class for newing an object with one constructor argument
    /// Also provides a Set for setting the value when being popped off a
    /// pool stack (see `Pool` below).
    /// </summary>
    internal interface New<A, B>
    {
        static abstract A Create(B value);
        static abstract void Set(A item, B value);
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

    /// <summary>
    /// Thread-safe pooling 
    /// Manages a concurrent stack of values that will grow as needed
    /// When spent new objects are allocated used the `New&lt;A&gt;` trait
    /// </summary>
    internal static class Pool<NewA, A, B> where NewA : New<A, B>
    {
        static readonly ConcurrentStack<A> stack = new();

        public static A Pop(B value)
        {
            if(stack.TryPop(out A? var))
            {
                NewA.Set(var, value);
                return var;
            }
            else
            {
                return NewA.Create(value);
            }
        }

        public static void Push(A value) =>
            stack.Push(value);
    }
}
