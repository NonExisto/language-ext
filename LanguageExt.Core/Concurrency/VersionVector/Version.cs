using System;
using LanguageExt.Traits;
using static LanguageExt.Prelude;
using LanguageExt.ClassInstances;

namespace LanguageExt;

/// <summary>
/// Wraps up a version vector, making it easier to work with and not generics hell
/// </summary>
/// <typeparam name="Actor">Actor type</typeparam>
/// <typeparam name="K">Key type</typeparam>
/// <typeparam name="V">Value type</typeparam>
public abstract record Version<Actor, K, V>(K Key)
{
    /// <summary>
    /// Perform a write to the vector.  This increases the vector-clock by 1 for the `actor` provided.
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="timeStamp"></param>
    /// <param name="value">Value to write</param>
    public abstract Version<Actor, K, V> Write(Actor actor, long timeStamp, V value);

    /// <summary>
    /// Perform a write to the vector.  This increases the vector-clock by 1 for the `actor` provided.
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="value">Value to write</param>
    public Version<Actor, K, V> Write(Actor actor, V value) =>
        Write(actor, DateTime.UtcNow.Ticks, value);

    /// <summary>
    /// Perform a delete to the vector.  This increases the vector-clock by 1 for the `actor` provided.
    /// </summary>
    /// <param name="actor">Actor to delete</param>
    /// <param name="timeStamp"></param>
    public abstract Version<Actor, K, V> Delete(Actor actor, long timeStamp);

    /// <summary>
    /// Perform a delete to the vector.  This increases the vector-clock by 1 for the `actor` provided.
    /// </summary>
    /// <param name="actor">Actor to delete</param>
    public Version<Actor, K, V> Delete(Actor actor) =>
        Delete(actor, DateTime.UtcNow.Ticks);

    /// <summary>
    /// Get the value if there is one
    /// </summary>
    public abstract Option<V> Value { get; }
}

/// <summary>
/// Internal: Helper functions for mapping between the non-generics Version and the generics heavy VersionVector
/// </summary>
internal static class Version
{
    public static Version<Actor, K, V> ToVersion<ConflictV, OrdActor, Actor, K, V>(this VersionVector<ConflictV, OrdActor, TLong, Actor, long, V> vector, K key)
        where OrdActor   : Ord<Actor>
        where ConflictV : Conflict<V> =>
        vector.Value.IsSome
            ? new VersionValueVector<ConflictV, OrdActor, Actor, K, V>(key, vector)
            : new VersionDeletedVector<ConflictV, OrdActor, Actor, K, V>(key, vector);

    public static VersionVector<ConflictV, OrdActor, TLong, Actor, long, V>? ToVector<ConflictV, OrdActor, Actor, K, V>(
        this Version<Actor, K, V> version)
        where OrdActor : Ord<Actor>
        where ConflictV : Conflict<V> =>
        version switch
        {
            VersionValueVector<ConflictV, OrdActor, Actor, K, V> vv   => vv.Vector,
            VersionDeletedVector<ConflictV, OrdActor, Actor, K, V> vd => vd.Vector,
            _                                                         => null
        };
}

/// <summary>
/// Abstract representation of a version vector with a value
/// </summary>
/// <typeparam name="Actor">Actor type</typeparam>
/// <typeparam name="K">Key type</typeparam>
/// <typeparam name="V">Value type</typeparam>
internal abstract record VersionSome<Actor, K, V>(K Key, V value) : Version<Actor, K, V>(Key)
{
    /// <summary>
    /// Get the value if there is one
    /// </summary>
    public override Option<V> Value =>
        value;
}

/// <summary>
/// Abstract representation of a version vector without a value (it either never existed or has been deleted)
/// </summary>
/// <typeparam name="Actor">Actor type</typeparam>
/// <typeparam name="K">Key type</typeparam>
/// <typeparam name="V">Value type</typeparam>
internal abstract record VersionNone<Actor, K, V>(K Key) : Version<Actor, K, V>(Key)
{

    /// <summary>
    /// Get the value if there is one
    /// </summary>
    public override Option<V> Value =>
        None;
}

/// <summary>
/// Representation of a version vector that never existed
/// </summary>
/// <typeparam name="ConflictV"></typeparam>
/// <typeparam name="OrdActor"></typeparam>
/// <typeparam name="Actor">Actor type</typeparam>
/// <typeparam name="K">Key type</typeparam>
/// <typeparam name="V">Value type</typeparam>
internal sealed record VersionNeverExistedVector<ConflictV, OrdActor, Actor, K, V>(K Key) : VersionNone<Actor, K, V>(Key)
    where OrdActor  : Ord<Actor>
    where ConflictV : Conflict<V>
{
    public static Version<Actor, K, V> New(K key) => new VersionNeverExistedVector<ConflictV, OrdActor, Actor, K, V>(key);

    /// <summary>
    /// Perform a write to the vector.  This increases the vector-clock by 1 for the `actor` provided.
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="timeStamp"></param>
    /// <param name="value">Value to write</param>
    public override Version<Actor, K, V> Write(Actor actor, long timeStamp, V value) =>
        new VersionValueVector<ConflictV, OrdActor, Actor, K, V>(
            Key,
            new VersionVector<ConflictV, OrdActor, TLong, Actor, long, V>(
                value,
                timeStamp,
                VectorClock.Single<OrdActor, TLong, Actor, long>(actor, 1L)));

    /// <summary>
    /// Perform a write to the vector.  This increases the vector-clock by 1 for the `actor` provided.
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="timeStamp"></param>
    public override Version<Actor, K, V> Delete(Actor actor, long timeStamp) =>
        this;
}

/// <summary>
/// Representation of a version vector that existed but has since had its value deleted
/// </summary>
/// <typeparam name="ConflictV"></typeparam>
/// <typeparam name="OrdActor"></typeparam>
/// <typeparam name="Actor">Actor type</typeparam>
/// <typeparam name="K">Key type</typeparam>
/// <typeparam name="V">Value type</typeparam>
internal sealed record VersionDeletedVector<ConflictV, OrdActor, Actor, K, V>(K Key, VersionVector<ConflictV, OrdActor, TLong, Actor, long, V> Vector) : VersionNone<Actor, K, V>(Key)
    where OrdActor : Ord<Actor>
    where ConflictV : Conflict<V>
{
    /// <summary>
    /// Perform a write to the vector.  This increases the vector-clock by 1 for the `actor` provided.
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="timeStamp"></param>
    /// <param name="value">Value to write</param>
    public override Version<Actor, K, V> Write(Actor actor, long timeStamp, V value) =>
        new VersionValueVector<ConflictV, OrdActor, Actor, K, V>(Key, Vector.Put(actor, timeStamp, value));

    /// <summary>
    /// Perform a write to the vector.  This increases the vector-clock by 1 for the `actor` provided.
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="timeStamp"></param>
    public override Version<Actor, K, V> Delete(Actor actor, long timeStamp) =>
        new VersionDeletedVector<ConflictV, OrdActor, Actor, K, V>(Key, Vector.Put(actor, timeStamp, None));
}

/// <summary>
/// Representation of a version vector with a value
/// </summary>
/// <typeparam name="ConflictV"></typeparam>
/// <typeparam name="OrdActor"></typeparam>
/// <typeparam name="Actor">Actor type</typeparam>
/// <typeparam name="K">Key type</typeparam>
/// <typeparam name="V">Value type</typeparam>
internal sealed record VersionValueVector<ConflictV, OrdActor, Actor, K, V>(K Key, VersionVector<ConflictV, OrdActor, TLong, Actor, long, V> Vector) : 
    VersionSome<Actor, K, V>(Key, Vector.Value.Value ?? throw new ArgumentNullException(nameof(Vector.Value.Value)))
    where OrdActor : Ord<Actor>
    where ConflictV : Conflict<V>
{
    /// <summary>
    /// Perform a write to the vector.  This increases the vector-clock by 1 for the `actor` provided.
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="timeStamp"></param>
    /// <param name="value">Value to write</param>
    public override Version<Actor, K, V> Write(Actor actor, long timeStamp, V value) =>
        new VersionValueVector<ConflictV, OrdActor, Actor, K, V>(Key, Vector.Put(actor, timeStamp, value));

    /// <summary>
    /// Perform a write to the vector.  This increases the vector-clock by 1 for the `actor` provided.
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="timeStamp"></param>
    public override Version<Actor, K, V> Delete(Actor actor, long timeStamp) =>
        new VersionDeletedVector<ConflictV, OrdActor, Actor, K, V>(Key, Vector.Put(actor, timeStamp, None));
}
