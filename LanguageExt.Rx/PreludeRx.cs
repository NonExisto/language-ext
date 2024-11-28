using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace LanguageExt;

public static class PreludeRx
{
    /// <summary>
    /// Execute a function after a specified delay
    /// </summary>
    /// <param name="f">Function to execute</param>
    /// <param name="delayFor">Time span to delay for</param>
    /// <returns>IObservable T with the result</returns>
    public static IObservable<T> delay<T>(Func<T> f, TimeSpan delayFor) =>
        Observable.Return(Prelude.unit).Delay(delayFor, TaskPoolScheduler.Default).Select(_ => f());
        

    /// <summary>
    /// Execute a function at a specific time
    /// </summary>
    /// <remarks>
    /// This will fail to be accurate across a Daylight Saving Time boundary
    /// </remarks>
    /// <param name="f">Function to execute</param>
    /// <param name="delayUntil">DateTime to wake up at.</param>
    /// <returns>IObservable T with the result</returns>
    public static IObservable<T> delay<T>(Func<T> f, DateTime delayUntil) =>
        delay(f, delayUntil.ToUniversalTime() - DateTime.UtcNow);
}
