using System;
using System.Threading;

namespace LanguageExt;

sealed record CancellationTokenCleanUp(CancellationTokenSource Src, CancellationTokenRegistration Reg) : IDisposable
{
    volatile int disposed;
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 0)
        {
            try { Src.Dispose(); } catch { /* not important */ }
            try { Reg.Dispose(); } catch { /* not important */ }
        }
    }
}

