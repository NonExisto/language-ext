using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using LanguageExt.Traits;

namespace LanguageExt;

/// <summary>
/// Stream subscription
/// </summary>
/// <typeparam name="A">Stream value type</typeparam>
abstract class Sub<A> : IDisposable
{
    public abstract void Dispose();
    public abstract void Post(A value);
    public abstract void Complete();
}

/// <summary>
/// Stream subscription
/// </summary>
/// <typeparam name="M">Monad type lifted in the stream</typeparam>
/// <typeparam name="A">Stream value type</typeparam>
sealed class Sub<M, A> : Sub<A>
    where M : Monad<M>
{
    readonly ConcurrentQueue<A> queue = new();
    readonly AutoResetEvent wait = new(false);
    readonly Action unsubscribe;
    long active;

    /// <summary>
    /// Stream of items
    /// </summary>
    public readonly StreamT<M, A> Stream;
 
    internal Sub(Action unsubscribe)
    {
        this.unsubscribe = unsubscribe;
        Stream = StreamT<M, A>.Lift(CreateStream());
    }

    public override void Post(A value)
    {
        queue.Enqueue(value);
        wait.Set();
    }

    public override void Complete()
    {
        if (Interlocked.CompareExchange(ref active, Statuses.Completing, Statuses.Running) == Statuses.Running)
        {
            wait.Set();

            // Wait for the queue to empty
            SpinWait sw = default;
            while (!queue.IsEmpty)
            {
                sw.SpinOnce();
            }

            unsubscribe();
            Dispose();
        }
    }

    IEnumerable<A> CreateStream()
    {
        while (true)
        {
            switch (Interlocked.Read(ref active))
            {
                case Statuses.Running:
                    while (queue.TryDequeue(out var e))
                    {
                        yield return e;
                    }
                    break;
                    
                case Statuses.Completing:
                    while (queue.TryDequeue(out var e))
                    {
                        yield return e;
                    }
                    Interlocked.Exchange(ref active, Statuses.Disposed);
                    yield break;
                    
                default:
                    yield break;
            }
            wait.WaitOne();
        }
    }

    public override void Dispose() => wait.Dispose();
}
