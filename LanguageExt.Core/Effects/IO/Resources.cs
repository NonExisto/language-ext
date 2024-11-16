using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using static LanguageExt.Prelude;

namespace LanguageExt;

/// <summary>
/// Holds the acquired resources for the `ResourceT` monad transformer
/// </summary>
public class Resources : IDisposable
{
    readonly AtomHashMap<object, TrackedResource> resources = AtomHashMap<object, TrackedResource>();
    readonly Resources? parent;

    public Resources(Resources? parent) =>
        this.parent = parent;

    public static IO<Resources> NewIO(Resources? parent) => 
        IO.lift(_ => new Resources(parent));
    
    public void Dispose()
    {
        var s = new CancellationTokenSource();
        var e = EnvIO.New(this, default, s, SynchronizationContext.Current);
        DisposeU(e);
    }
    
    public Unit DisposeU(EnvIO envIO)
    {
        foreach (var (_, Value) in resources)
        {
            Value.Release().Run(envIO);
        }
        return default;
    }

    public Unit DisposeU()
    {
        Dispose();
        return default;
    }

    public IO<Unit> DisposeIO() =>
        IO.lift(_ => DisposeU());

    public Unit Acquire<A>([DisallowNull]A value) where A : IDisposable
    {
        ArgumentNullException.ThrowIfNull(value);
        return resources.TryAdd(value, new TrackedResourceDisposable<A>(value), null);
    }

    public Unit AcquireAsync<A>([DisallowNull]A value) where A : IAsyncDisposable
    {
        ArgumentNullException.ThrowIfNull(value);
        return resources.TryAdd(value, new TrackedResourceAsyncDisposable<A>(value), null);
    }

    public Unit Acquire<A>([DisallowNull]A value, Func<A, IO<Unit>> release) 
    {
        ArgumentNullException.ThrowIfNull(value);
        return resources.TryAdd(value, new TrackedResourceWithFree<A>(value, release), null);
    }

    public IO<Unit> Release<A>([DisallowNull]A value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return resources.Find(value)
                        .Match(Some: f =>
                                     {
                                         resources.Remove(value);
                                         return f.Release();
                                     },
                               None: () => parent is null 
                                               ? unitIO
                                               : parent.Release(value));
    }

    public IO<Unit> ReleaseAll() =>
        IO.lift(envIO =>
                resources.Swap(
                        r =>
                        {
                            foreach (var (Key, Value) in r)
                            {
                                Value.Release().Run(envIO);
                            }
                            return [];
                        })
                );
    
    internal Unit Merge(Resources rhs) =>
        resources.Swap(r => r.AddRange(rhs.resources.AsIterable()));
}

abstract record TrackedResource
{
    public abstract IO<Unit> Release();
}

/// <summary>
/// Holds a resource with its disposal function
/// </summary>
sealed record TrackedResourceWithFree<A>(A Value, Func<A, IO<Unit>> Dispose) : TrackedResource
{
    public override IO<Unit> Release() => 
        Dispose(Value);
}

/// <summary>
/// Holds a resource with its disposal function
/// </summary>
sealed record TrackedResourceDisposable<A>(A Value) : TrackedResource
    where A : IDisposable
{
    public override IO<Unit> Release() =>
        Value switch
        {
            IAsyncDisposable disposable => IO<Unit>.LiftAsync(
                async () =>
                {
                    await disposable.DisposeAsync().ConfigureAwait(false);
                    return unit;
                }),
            
            _ => IO<Unit>.Lift(
                () =>
                {
                    Value.Dispose();
                    return unit;
                })
        };
}

/// <summary>
/// Holds a resource with its disposal function
/// </summary>
sealed record TrackedResourceAsyncDisposable<A>(A Value) : TrackedResource
    where A : IAsyncDisposable
{
    public override IO<Unit> Release() =>
        IO<Unit>.LiftAsync(
                async () =>
                {
                    await Value.DisposeAsync().ConfigureAwait(false);
                    return unit;
                });
}
