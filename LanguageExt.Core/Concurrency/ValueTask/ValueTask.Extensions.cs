using LanguageExt.ClassInstances;
using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace LanguageExt;

public static class ValueTaskExtensions
{
    public static bool CompletedSuccessfully<A>(this ValueTask<A> ma) =>
        ma is { IsCompleted: true, IsFaulted: false, IsCanceled: false };
        
    [Pure]
    public static ValueTask<A> AsFailedValueTask<A>(this Exception ex) =>
        ValueTask.FromException<A>(ex);

    /// <summary>
    /// Convert a value to a Task that completes immediately
    /// </summary>
    [Pure]
    public static ValueTask<A> AsValueTask<A>(this A self) =>
        new (self);

    /// <summary>
    /// Convert a Task to a ValueTask 
    /// </summary>
    [Pure]
    public static ValueTask<A> ToValue<A>(this Task<A> self) =>
        new (self);

    /// <summary>
    /// Flatten the nested Task type
    /// </summary>
    [Pure]
    public static async ValueTask<A> Flatten<A>(this ValueTask<ValueTask<A>> self)
    {
        var t = await self.ConfigureAwait(false);
        var u = await t.ConfigureAwait(false);
        return u;
    }

    /// <summary>
    /// Flatten the nested Task type
    /// </summary>
    [Pure]
    public static async ValueTask<A> Flatten<A>(this ValueTask<ValueTask<ValueTask<A>>> self)
    {
        var t = await self.ConfigureAwait(false);
        var u = await t.ConfigureAwait(false);
        var v = await u.ConfigureAwait(false);
        return v;
    }

    /// <summary>
    /// Standard LINQ Select implementation for Task
    /// </summary>
    [Pure]
    public static async ValueTask<U> Select<T, U>(this ValueTask<T> self, Func<T, U> map) =>
        map(await self.ConfigureAwait(false));

    /// <summary>
    /// Standard LINQ Where implementation for Task
    /// </summary>
    [Pure]
    public static async ValueTask<T> Where<T>(this ValueTask<T> self, Func<T, bool> pred)
    {
        var resT = await self.ConfigureAwait(false);
        var res = pred(resT);
        if (!res)
        {
            throw new TaskCanceledException();
        }

        return resT;
    }

    /// <summary>
    /// Standard LINQ SelectMany implementation for Task
    /// </summary>
    [Pure]
    public static async ValueTask<U> SelectMany<T, U>(this ValueTask<T> self,
        Func<T, ValueTask<U>> bind) =>
        await bind(await self.ConfigureAwait(false)).ConfigureAwait(false);

    /// <summary>
    /// Standard LINQ SelectMany implementation for Task
    /// </summary>
    [Pure]
    public static async ValueTask<V> SelectMany<T, U, V>(this ValueTask<T> self,
        Func<T, ValueTask<U>> bind,
        Func<T, U, V> project)
    {
        var resT = await self.ConfigureAwait(false);
        var resU = await bind(resT).ConfigureAwait(false);
        return project(resT, resU);
    }

    /// <summary>
    /// Get the Count of a Task T.  Returns either 1 or 0 if cancelled or faulted.
    /// </summary>
    [Pure]
    public static async ValueTask<int> Count<T>(this ValueTask<T> self)
    {
        try
        {
            _ = await self.ConfigureAwait(false);
            return 1;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    /// <summary>
    /// Monadic bind operation for Task
    /// </summary>
    [Pure]
    public static ValueTask<U> Bind<T, U>(this ValueTask<T> self, Func<T, ValueTask<U>> bind) =>
        self.SelectMany(bind);

    /// <summary>
    /// Returns false if the Task is cancelled or faulted, otherwise
    /// it returns the result of pred(Result)
    /// </summary>
    [Pure]
    public static async ValueTask<bool> Exists<T>(this ValueTask<T> self, Func<T, bool> pred) =>
        pred(await self.ConfigureAwait(false));

    /// <summary>
    /// Returns false if the Task is cancelled or faulted, otherwise
    /// it returns the result of pred(Result)
    /// </summary>
    [Pure]
    public static async ValueTask<bool> ExistsAsync<T>(this ValueTask<T> self, Func<T, ValueTask<bool>> pred) =>
        await pred(await self.ConfigureAwait(false)).ConfigureAwait(false);

    /// <summary>
    /// Returns false if the Task is cancelled or faulted, otherwise
    /// it returns the result of pred(Result)
    /// </summary>
    [Pure]
    public static async ValueTask<bool> ForAll<T>(this ValueTask<T> self, Func<T, bool> pred) =>
        pred(await self.ConfigureAwait(false));

    /// <summary>
    /// Returns false if the Task is cancelled or faulted, otherwise
    /// it returns the result of pred(Result)
    /// </summary>
    [Pure]
    public static async ValueTask<bool> ForAllAsync<T>(this ValueTask<T> self, Func<T, ValueTask<bool>> pred) =>
        await pred(await self.ConfigureAwait(false)).ConfigureAwait(false);

    /// <summary>
    /// Filters the task.  This throws a BottomException when pred(Result)
    /// returns false
    /// </summary>
    [Pure]
    public static ValueTask<T> Filter<T>(this ValueTask<T> self, Func<T, bool> pred) =>
        self.Where(pred);

    /// <summary>
    /// Folds the Task.  Returns folder(state,Result) if not faulted or
    /// cancelled.  Returns state otherwise.
    /// </summary>
    [Pure]
    public static async ValueTask<S> Fold<T, S>(this ValueTask<T> self, S state, Func<S, T, S> folder) =>
        folder(state, await self.ConfigureAwait(false));

    /// <summary>
    /// Folds the Task.  Returns folder(state,Result) if not faulted or
    /// cancelled.  Returns state otherwise.
    /// </summary>
    [Pure]
    public static async ValueTask<S> FoldAsync<T, S>(this ValueTask<T> self, S state, Func<S, T, ValueTask<S>> folder) =>
        await folder(state, await self.ConfigureAwait(false)).ConfigureAwait(false);

    /// <summary>
    /// Iterates the Task.  Invokes f(Result) if not faulted or cancelled
    /// </summary>
    public static async ValueTask<Unit> Iter<T>(this ValueTask<T> self, Action<T> f)
    {
        f(await self.ConfigureAwait(false));
        return unit;
    }

    /// <summary>
    /// Impure iteration of the bound value in the structure
    /// </summary>
    /// <returns>
    /// Returns the original unmodified structure
    /// </returns>
    public static ValueTask<A> Do<A>(this ValueTask<A> ma, Action<A> f) =>
        ma.Map(x => {
            f(x);
            return x;
        });

    /// <summary>
    /// Returns map(Result) if not faulted or cancelled.
    /// </summary>
    [Pure]
    public static async ValueTask<U> Map<T, U>(this ValueTask<T> self, Func<T, U> map) =>
        map(await self.ConfigureAwait(false));

    /// <summary>
    /// Returns map(Result) if not faulted or cancelled.
    /// </summary>
    [Pure]
    public static async ValueTask<U> MapAsync<T, U>(this ValueTask<T> self, Func<T, ValueTask<U>> map) =>
        await map(await self.ConfigureAwait(false)).ConfigureAwait(false);

    [Pure]
    public static async ValueTask<V> Join<T, U, K, V>(this ValueTask<T> source,
        ValueTask<U> inner,
        Func<T, K> outerKeyMap,
        Func<U, K> innerKeyMap,
        Func<T, U, V> project)
    {
        await Task.WhenAll(source.AsTask(), inner.AsTask()).ConfigureAwait(false);
        if (!EqDefault<K>.Equals(outerKeyMap(source.Result), innerKeyMap(inner.Result)))
        {
            throw new OperationCanceledException();
        }
        return project(source.Result, inner.Result);
    }

    [Pure]
    public static async ValueTask<V> GroupJoin<T, U, K, V>(this ValueTask<T> source,
        ValueTask<U> inner,
        Func<T, K> outerKeyMap,
        Func<U, K> innerKeyMap,
        Func<T, ValueTask<U>, V> project)
    {
        T t = await source.ConfigureAwait(false);
        return project(t, inner.Where(u => EqDefault<K>.Equals(outerKeyMap(t), innerKeyMap(u))));
    }

    [Pure]
    public static async ValueTask<A> Plus<A>(this ValueTask<A> ma, ValueTask<A> mb)
    {
        try
        {
            return await ma.ConfigureAwait(false);
        }
        catch
        {
            return await mb.ConfigureAwait(false);
        }
    }

    [Pure]
    public static async ValueTask<A> PlusFirst<A>(this ValueTask<A> ma, ValueTask<A> mb) =>
        await ma.AsTask().PlusFirst(mb.AsTask());
        
    /// <summary>
    /// Cast a ValueTask to a <see cref="ValueTask{A}"/> (may throw if underlying value doesn't exist)
    /// </summary>
    public static ValueTask<A> Cast<A>(this ValueTask source) => 
        new(source.AsTask().Cast<A>());

    public static async ValueTask<Unit> ToUnit(this ValueTask source)
    {
        await source.ConfigureAwait(false);
        return unit;
    }
}
