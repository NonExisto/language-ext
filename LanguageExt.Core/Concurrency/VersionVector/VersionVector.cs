using System;
using LanguageExt.Traits;

namespace LanguageExt;

/// <summary>
/// Version vector.  Container of an optional value, a time-stamp, and a vector clock.  Used to manage versioning
/// of a single value.  ConflictA allows for injectable conflict resolution.
/// </summary>
/// <typeparam name="ConflictA">Conflict resolution instance type</typeparam>
/// <typeparam name="OrdActor">Actor ordering instance type</typeparam>
/// <typeparam name="NumClock">Numeric clock instance type</typeparam>
/// <typeparam name="Actor">Actor type</typeparam>
/// <typeparam name="Clock">Clock type</typeparam>
/// <typeparam name="A">Value to version type</typeparam>
public record VersionVector<ConflictA, OrdActor, NumClock, Actor, Clock, A>(
    Option<A> Value, 
    long TimeStamp, 
    VectorClock<OrdActor, NumClock, Actor, Clock> Vector) 
    where OrdActor  : Ord<Actor>
    where NumClock  : Num<Clock>
    where ConflictA : Conflict<A>
{
    /// <summary>
    /// Register a write from any actor
    /// </summary>
    /// <param name="version">The vector of the actor</param>
    /// <returns></returns>
    public VersionVector<ConflictA, OrdActor, NumClock, Actor, Clock, A> Put(
        VersionVector<ConflictA, OrdActor, NumClock, Actor, Clock, A> version) =>
        VectorClock.relation(Vector, version.Vector) switch
        {
            // `version` happened in the past, we don't care about it
            Relation.CausedBy => this,

            // `version` is causally linked to us, it is the future of us, so accept it
            Relation.Causes => version,

            // `version` has changed on a different branch of time to us, and so it conflicts.  Resolve it
            // by using the monoid as a merge for the values, and take the pairwise maximum of the two vector clocks 
            Relation.Concurrent => ResolveConflict(version),

            // Should never get here
            _ => throw new NotSupportedException()
        };

    VersionVector<ConflictA, OrdActor, NumClock, Actor, Clock, A> ResolveConflict(VersionVector<ConflictA, OrdActor, NumClock, Actor, Clock, A> version)
    {
        var (ts, nv) = ConflictA.Resolve((TimeStamp, Value), (version.TimeStamp, version.Value));
        return new VersionVector<ConflictA, OrdActor, NumClock, Actor, Clock, A>(
            Value: nv, 
            TimeStamp: ts, 
            Vector: VectorClock.max(Vector, version.Vector));
    }

    /// <summary>
    /// Perform a write to the vector.  This increases the vector-clock by 1 for the `actor` provided.
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="timeStamp"></param>
    /// <param name="value">Value to write</param>
    public VersionVector<ConflictA, OrdActor, NumClock, Actor, Clock, A> Put(Actor actor, long timeStamp, Option<A> value) =>
        new (Value: value, 
             TimeStamp: timeStamp, 
             Vector: Vector.Inc(actor, NumClock.FromInteger(0)));
}

/// <summary>
/// Version vector.  Container of an optional value, a time-stamp, and a vector clock.  Used to manage versioning
/// of a single value.  ConflictA allows for injectable conflict resolution.
/// </summary>
/// <typeparam name="ConflictA">Conflict resolution instance type</typeparam>
/// <typeparam name="Actor">Actor type</typeparam>
/// <typeparam name="A">Value to version type</typeparam>
public record VersionVector<ConflictA, Actor, A>(Option<A> Value, long TimeStamp, VectorClock<Actor> Vector)
    where Actor : IComparable<Actor>
    where ConflictA : Conflict<A>
{
    /// <summary>
    /// Register a write from any actor
    /// </summary>
    /// <param name="version">The vector of the actor</param>
    /// <returns></returns>
    public VersionVector<ConflictA, Actor, A> Put(VersionVector<ConflictA, Actor, A> version) =>
        VectorClock.relation(Vector, version.Vector) switch
        {
            // `version` happened in the past, we don't care about it
            Relation.CausedBy => this,

            // `version` is causally linked to us, it is the future of us, so accept it
            Relation.Causes => version,

            // `version` has changed on a different branch of time to us, and so it conflicts.  Resolve it
            // by using the monoid as a merge for the values, and take the pairwise maximum of the two vector clocks 
            Relation.Concurrent => ResolveConflict(version),

            // Should never get here
            _ => throw new NotSupportedException()
        };

    VersionVector<ConflictA, Actor, A> ResolveConflict(VersionVector<ConflictA, Actor, A> version)
    {
        var (ts, nv) = ConflictA.Resolve((TimeStamp, Value), (version.TimeStamp, version.Value));
        return new VersionVector<ConflictA, Actor, A>(Value: nv,
                                                      TimeStamp: ts,
                                                      Vector: VectorClock.max(Vector, version.Vector));
    }

    /// <summary>
    /// Perform a write to the vector.  This increases the vector-clock by 1 for the `actor` provided.
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="timeStamp"></param>
    /// <param name="value">Value to write</param>
    public VersionVector<ConflictA, Actor, A> Put(Actor actor, long timeStamp, Option<A> value) =>
        new(Value: value, TimeStamp: timeStamp, Vector: Vector.Inc(actor, 0L));
}
