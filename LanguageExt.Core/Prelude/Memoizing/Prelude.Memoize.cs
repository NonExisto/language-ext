using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LanguageExt;

public static partial class Prelude
{
    /// <summary>
    /// Returns a <see cref="Func{TResult}"/> that wraps func.  The first
    /// call to the resulting Func&lt;A&gt; will cache the result.
    /// Subsequent calls return the cached item.
    /// </summary>
    public static Func<A> memo<A>(Func<A> func)
    {
        var  sync     = new object();
        var  value    = default(A);
        bool valueSet = false;
        return () =>
               {
                   if(valueSet)
                   {
                       return value!;
                   }
                   lock(sync)
                   {
                       if (valueSet)
                       {
                           return value!;
                       }
                       else
                       {
                           value = func();
                           valueSet = true;
                           return value;
                       }
                   }
               };
    }

    /// <summary>
    /// Returns a <see cref="Func{T, TResult}"/> that wraps func.  Each time the resulting
    /// `Func&lt;A, B&gt;` is called with a new value, its result is memoized (cached).
    /// Subsequent calls use the memoized value.  
    /// 
    /// Remarks: 
    ///     Thread-safe and memory-leak safe.  
    /// </summary>
    public static Func<A, B> memo<A, B>(Func<A, B> func) where A : notnull
    {
        var cache   = new WeakDict<A, B> ();
        var syncMap = new ConcurrentDictionary<A, object>();

        return inp =>
               {
                   if(cache.TryGetValue(inp, out var x))
                   {
                       return x;
                   }
                   else
                   {
                       B   res;
                       var sync = syncMap.GetOrAdd(inp, new object());
                       lock (sync)
                       {
                           res = cache.GetOrAdd(inp, func);
                       }
                       syncMap.TryRemove(inp, out sync);
                       return res;
                   }
               };
    }

    /// <summary>
    /// Returns a <see cref="Func{T, TResult}"/> that wraps func.  Each time the resulting
    /// Func&lt;T,R&gt; is called with a new value, its result is memoized (cached).
    /// Subsequent calls use the memoized value.  
    /// </summary>
    /// <remarks>
    ///     No mechanism for freeing cached values and therefore can cause a
    ///     memory leak when holding onto the Func&lt;T,R&gt; reference.
    ///     Uses a ConcurrentDictionary for the cache and is thread-safe
    /// </remarks>
    public static Func<T, R> memoUnsafe<T, R>(Func<T, R> func) where T : notnull
    {
        var cache   = new ConcurrentDictionary<T, R>();
        
        return inp => cache.GetOrAdd(inp, func);
    }

    /// <summary>
    /// Enumerable memoization.  As an enumerable is enumerated each item is retained
    /// in an internal list, so that future evaluation of the enumerable isn't done. 
    /// Only items not seen before are evaluated.  
    /// 
    /// This minimizes one of the major problems with the IEnumerable / yield return 
    /// pattern by causing at-most-once evaluation of each item.  
    /// 
    /// Use the IEnumerable extension method Memo for convenience.
    /// </summary>
    /// <remarks>
    /// Although this allows efficient lazy evaluation, it does come at a memory cost.
    /// Each item is cached internally, so this method doesn't allow for evaluation of
    /// infinite sequences.
    /// </remarks>
    /// <param name="seq">Enumerable to memoize</param>
    /// <returns>IEnumerable with caching properties</returns>
    public static Seq<T> memo<T>(IEnumerable<T> seq) =>
        toSeq(seq);

    /// <summary>
    /// Used internally by the memo function.  It wraps a concurrent dictionary that has 
    /// its value objects wrapped in a <see cref="WeakReference{T}"/>
    /// The OnFinalize type is a private class within WeakDict and does nothing but hold
    /// the value and an Action to call when its finalized.  So when the WeakReference is
    /// collected by the GC, it forces the finalizer to be called on the OnFinalize object,
    /// which in turn executes the action which removes it from the ConcurrentDictionary.  
    /// That means that both the key and value are collected when the GC fires rather than 
    /// just the value.  Mitigates memory leak of keys.
    /// </summary>
    private class WeakDict<T, R> where T : notnull
    {
        private class OnFinalize<V>(Action onFinalize, V value)
        {
            public readonly V Value = value;

            ~OnFinalize() =>
                onFinalize.Invoke();
        }

        readonly ConcurrentDictionary<T, WeakReference<OnFinalize<R>>> dict = new();

        private WeakReference<OnFinalize<R>> NewRef(T key, R value) =>
            new (new OnFinalize<R>(() => dict.TryRemove(key, out _), value));

        public bool TryGetValue(T key, [NotNullWhen(true)]out R? value)
        {
            if(dict.TryGetValue(key, out var reference) && reference.TryGetTarget(out var target))
            {
                value = target.Value!;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public R GetOrAdd(T key, Func<T, R> valueFunc)
        {
            var res = dict.GetOrAdd(key, _ => NewRef(key, valueFunc(key)));

            if (res.TryGetTarget(out var target))
            {
                return target.Value;
            }
            else
            {
                R value = valueFunc(key);
                var upd = NewRef(key, value);
                res = dict.AddOrUpdate(key, upd, (_, _) => upd);
                return value;
            }
        }
    }
}


public static class MemoExtensions
{
    /// <summary>
    /// Returns a <see cref="Func{TResult}"/> that wraps func.  The first
    /// call to the resulting Func&lt;T&gt; will cache the result.
    /// Subsequent calls return the cached item.
    /// </summary>
    public static Func<T> Memo<T>(this Func<T> func) =>
        Prelude.memo(func);

    /// <summary>
    /// Returns a <see cref="Func{T, TResult}"/> that wraps func.  Each time the resulting
    /// Func&lt;T,R&gt; is called with a new value, its result is memoized (cached).
    /// Subsequent calls use the memoized value.  
    /// 
    /// Remarks: 
    ///     Thread-safe and memory-leak safe.  
    /// </summary>
    public static Func<T, R> Memo<T, R>(this Func<T, R> func) where T : notnull =>
        Prelude.memo(func);

    /// <summary>
    /// Returns a <see cref="Func{T, TResult}"/> that wraps func.  Each time the resulting
    /// Func&lt;T,R&gt; is called with a new value, its result is memoized (cached).
    /// Subsequent calls use the memoized value.  
    /// </summary>
    /// <remarks>
    ///     No mechanism for freeing cached values and therefore can cause a
    ///     memory leak when holding onto the Func&lt;T,R&gt; reference.
    ///     Uses a ConcurrentDictionary for the cache and is thread-safe
    /// </remarks>
    public static Func<T, R> MemoUnsafe<T, R>(this Func<T, R> func) where T : notnull =>
        Prelude.memoUnsafe(func);

    /// <summary>
    /// Enumerable memoization.  As an enumerable is enumerated each item is retained
    /// in an internal list, so that future evaluation of the enumerable isn't done. 
    /// Only items not seen before are evaluated.  
    /// 
    /// This minimizes one of the major problems with the IEnumerable / yield return 
    /// pattern by causing at-most-once evaluation of each item.  
    /// 
    /// Use the IEnumerable extension method Memo for convenience.
    /// </summary>
    /// <remarks>
    /// Although this allows efficient lazy evaluation, it does come at a memory cost.
    /// Each item is cached internally, so this method doesn't allow for evaluation of
    /// infinite sequences.
    /// </remarks>
    /// <param name="seq">Enumerable to memoize</param>
    /// <returns>IEnumerable with caching properties</returns>
    public static IEnumerable<T> Memo<T>(this IEnumerable<T> seq) =>
        Prelude.memo(seq);
}
