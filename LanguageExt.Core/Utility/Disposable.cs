using System;
using System.Threading;

namespace LanguageExt;

internal static class Disposable<A>
{
    public static bool IsDisposable = typeof(IDisposable).IsAssignableFrom(typeof(A));
}

/// <summary>
/// Represents an Action-based disposable.
/// </summary>
internal sealed class AnonymousDisposable : IDisposable
{
    private volatile Action? _dispose;

    /// <summary>
    /// Constructs a new disposable with the given action used for disposal.
    /// </summary>
    /// <param name="dispose">Disposal action which will be run upon calling Dispose.</param>
    public AnonymousDisposable(Action dispose)
    {
        _dispose = dispose;
    }
    
    /// <summary>
    /// Calls the disposal action if and only if the current instance hasn't been disposed yet.
    /// </summary>
    public void Dispose() =>
        Interlocked.Exchange(ref _dispose, null)?.Invoke();
}
