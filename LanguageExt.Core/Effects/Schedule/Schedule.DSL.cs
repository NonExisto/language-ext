using System;
using System.Collections.Generic;
using static LanguageExt.UnitsOfMeasure;

namespace LanguageExt;

/// <summary>
/// Time series of durations
/// </summary>
internal sealed record SchItems(Iterable<Duration> Items) : Schedule
{
    public override Iterable<Duration> Run() =>
        Items;
}

/// <summary>
/// Functor map
/// </summary>
internal sealed record SchMap(Schedule Schedule, Func<Duration, Duration> F) : Schedule 
{
    public override Iterable<Duration> Run() =>
        Schedule.Run().Map(F);
}

/// <summary>
/// Functor map
/// </summary>
internal sealed record SchMapIndex(Schedule Schedule, Func<Duration, int, Duration> F) : Schedule 
{
    public override Iterable<Duration> Run() =>
        Schedule.Run().Select(F);
}

/// <summary>
/// Filter
/// </summary>
internal sealed record SchFilter(Schedule Schedule, Func<Duration, bool> Pred) : Schedule 
{
    public override Iterable<Duration> Run() =>
        Schedule.Run().Filter(Pred);
}

/// <summary>
/// Functor bind
/// </summary>
internal sealed record SchBind(Schedule Schedule, Func<Duration, Schedule> BindF) : Schedule
{
    public override Iterable<Duration> Run() =>
        Schedule.Run().Bind(x => BindF(x).Run());
}    

/// <summary>
/// Functor bind and project
/// </summary>
internal sealed record SchBind2(Schedule Schedule, Func<Duration, Schedule> BindF, Func<Duration, Duration, Duration> Project) : Schedule
{
    public override Iterable<Duration> Run() =>
        Schedule.Run().Bind(x => BindF(x).Run().Map(y => Project(x, y)));
}

/// <summary>
/// Tail of sequence
/// </summary>
internal sealed record SchTail(Schedule Schedule) : Schedule
{
    public override Iterable<Duration> Run() =>
        Schedule.Run().Tail();
}    

/// <summary>
/// Skip items in sequence
/// </summary>
internal sealed record SchSkip(Schedule Schedule, int Count) : Schedule
{
    public override Iterable<Duration> Run() =>
        Schedule.Run().Skip(Count);
}    

/// <summary>
/// Take items in sequence
/// </summary>
internal sealed record SchTake(Schedule Schedule, int Count) : Schedule
{
    public override Iterable<Duration> Run() =>
        Schedule.Run().Take(Count);
}

/// <summary>
/// Append in sequence
/// </summary>
internal sealed record SchCombine(Schedule Left, Schedule Right) : Schedule
{
    public override Iterable<Duration> Run() =>
        Left.Run().Combine(Right.Run());
}    

/// <summary>
/// Interleave items in sequence
/// </summary>
internal sealed record SchInterleave(Schedule Left, Schedule Right) : Schedule
{
    public override Iterable<Duration> Run() =>
        Left.Run()
            .Zip(Right.Run(), static (d1, d2) => new[] {d1, d2})
            .SelectMany(x => x);
}

/// <summary>
/// Union sequence
/// </summary>
internal sealed record SchUnion(Schedule Left, Schedule Right) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            using var aEnumerator = Left.Run().GetEnumerator();
            using var bEnumerator = Right.Run().GetEnumerator();

            var hasA = aEnumerator.MoveNext();
            var hasB = bEnumerator.MoveNext();

            while (hasA || hasB)
            {
                yield return hasA switch
                             {
                                 true when hasB => Math.Min(aEnumerator.Current, bEnumerator.Current),
                                 true           => aEnumerator.Current,
                                 _              => bEnumerator.Current
                             };

                hasA = hasA && aEnumerator.MoveNext();
                hasB = hasB && bEnumerator.MoveNext();

            }
        }
    }
}

/// <summary>
/// Intersect sequence
/// </summary>
internal sealed record SchIntersect(Schedule Left, Schedule Right) : Schedule
{
    public override Iterable<Duration> Run() =>
        Left.Run()
            .Zip(Right.Run())
            .Map(static t => (Duration)Math.Max(t.Item1, t.Item2));
}    

/// <summary>
/// Cons an item onto sequence
/// </summary>
internal sealed record SchCons(Duration Left, Schedule Right) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            yield return Left;
            foreach (var r in Right.Run())
            {
                yield return r;
            }
        }
    }
}

internal sealed record SchRepeatForever(Schedule Schedule) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            while (true)
                foreach (var x in Schedule.Run())
                    yield return x;
        }
    }
}

internal sealed record SchLinear(Duration Seed, double Factor) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            Duration delayToAdd  = Seed * Factor;
            var      accumulator = Seed;

            yield return accumulator;
            while (true)
            {
                accumulator += delayToAdd;
                yield return accumulator;
            }
        }
    }
}

internal sealed record SchFibonacci(Duration Seed) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            var last        = Duration.Zero;
            var accumulator = Seed;

            yield return accumulator;
            while (true)
            {
                var current = accumulator;
                accumulator += last;
                last        =  current;
                yield return accumulator;
            }
        }
    }
}

internal sealed record SchForever : Schedule
{
    public static readonly Schedule Default = new SchForever();

    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            while(true) yield return Duration.Zero;
        }
    }
}

internal sealed record SchNever : Schedule
{
    public static readonly Schedule Default = new SchNever();

    public override Iterable<Duration> Run() =>
        Iterable.empty<Duration>();
}

internal sealed record SchUpTo(Duration Max, Func<DateTime>? CurrentTimeFn = null) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            var now       = CurrentTimeFn ?? LiveNowFn;
            var startTime = now();
        
            while (now() - startTime < Max) 
                yield return Duration.Zero;
        }
    }
}

internal sealed record SchFixed(Duration Interval, Func<DateTime>? CurrentTimeFn = null) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            var now         = CurrentTimeFn ?? LiveNowFn;
            var startTime   = now();
            var lastRunTime = startTime;
            while (true)
            {
                var currentTime   = now();
                var runningBehind = currentTime > lastRunTime + (TimeSpan)Interval;
            
                var boundary = Interval == Duration.Zero
                                   ? Interval
                                   : secondsToIntervalStart(startTime, currentTime, Interval);
            
                var sleepTime = boundary == Duration.Zero 
                                    ? Interval 
                                    : boundary;
            
                lastRunTime = runningBehind ? currentTime : currentTime + (TimeSpan)sleepTime;
                yield return runningBehind ? Duration.Zero : sleepTime;
            }
        }
    }
}

internal sealed record SchWindowed(Duration Interval, Func<DateTime>? CurrentTimeFn = null) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            var now       = CurrentTimeFn ?? LiveNowFn;
            var startTime = now();
            while (true)
            {
                var currentTime = now();
                yield return secondsToIntervalStart(startTime, currentTime, Interval);
            }
        }
    }
}

internal sealed record SchSecondOfMinute(int Second, Func<DateTime>? CurrentTimeFn = null) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            var now = CurrentTimeFn ?? LiveNowFn;
            while (true)
                yield return durationToIntervalStart(roundBetween(Second, 0, 59), now().Second, 60) * seconds;
        }
    }
}

internal sealed record SchMinuteOfHour(int Minute, Func<DateTime>? CurrentTimeFn = null) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            var now = CurrentTimeFn ?? LiveNowFn;
            while (true)
                yield return durationToIntervalStart(roundBetween(Minute, 0, 59), now().Minute, 60) * minutes;
        }
    }
}

internal sealed record SchHourOfDay(int Hour, Func<DateTime>? CurrentTimeFn = null) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            var now = CurrentTimeFn ?? LiveNowFn;
            while (true)
                yield return durationToIntervalStart(roundBetween(Hour, 0, 23), now().Hour, 24) * hours;
        }
    }
}

internal sealed record SchDayOfWeek(DayOfWeek Day, Func<DateTime>? CurrentTimeFn = null) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            var now = CurrentTimeFn ?? LiveNowFn;
            while (true)
                yield return durationToIntervalStart((int)Day + 1, (int)now().DayOfWeek + 1, 7) * days;
        }    
    }
}

internal sealed record SchMaxDelay(Schedule Schedule, Duration Max) : Schedule
{
    public override Iterable<Duration> Run() =>
        Schedule.Run().Map(x => x > Max ? Max : x);
}

internal sealed record SchMaxCumulativeDelay(Schedule Schedule, Duration Max) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            var totalAppliedDelay = Duration.Zero;

            foreach (var duration in Schedule.Run())
            {
                if (totalAppliedDelay >= Max) yield break;
                totalAppliedDelay += duration;
                yield return duration;
            }
        }
    }
}

internal sealed record SchJitter1(Schedule Schedule, Duration MinRandom, Duration MaxRandom, Option<int> Seed) : Schedule
{
    public override Iterable<Duration> Run() =>
        Schedule.Run().Map(x => (Duration)(x + SingletonRandom.Uniform(MinRandom, MaxRandom, Seed)));
}

internal sealed record SchJitter2(Schedule Schedule, double Factor, Option<int> Seed) : Schedule
{
    public override Iterable<Duration> Run() =>
        Schedule.Run().Map(x => (Duration)(x + SingletonRandom.Uniform(0, x * Factor, Seed)));
}

internal sealed record SchDecorrelate(Schedule Schedule, double Factor, Option<int> Seed) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            foreach(var currentMilliseconds in Schedule.Run())
            {
                var rand1 = SingletonRandom.Uniform(0, currentMilliseconds * Factor, Seed);
                var rand2 = SingletonRandom.Uniform(0, currentMilliseconds * Factor, Seed);
                yield return currentMilliseconds + rand1;
                yield return currentMilliseconds - rand2;
            }
        }
    }
}

internal sealed record SchResetAfter(Schedule Schedule, Duration Max) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            while (true)
                foreach (var duration in (Schedule | maxCumulativeDelay(Max)).Run())
                    yield return duration;
        }
    }
}

internal sealed record SchRepeat(Schedule Schedule, int Times) : Schedule
{
    public override Iterable<Duration> Run()
    {
        return Go().AsIterable();
        IEnumerable<Duration> Go()
        {
            for (var i = 0; i < Times; i++)
                foreach (var duration in Schedule.Run())
                    yield return duration;
        }
    }
}
