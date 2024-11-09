using System.Runtime.CompilerServices;

namespace LanguageExt;

/// <summary>
/// Represents change to a Hash-Map
/// </summary>
/// <remarks>
/// This is primarily used by the `Change` events on the `AtomHashMap` types,
/// and the `Changes` property of `TrackingHashMap`. 
/// </remarks>
/// <typeparam name="K">Key type</typeparam>
/// <typeparam name="V">Value type</typeparam>
public sealed class HashMapPatch<K, V>
{
    readonly TrieMap<K, V> prev;
    readonly TrieMap<K, V> curr;
    readonly TrieMap<K, Change<V>>? changes;
    readonly K? key;
    readonly Change<V>? change;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal HashMapPatch(
        TrieMap<K, V> prev,
        TrieMap<K, V> curr,
        TrieMap<K, Change<V>> changes)
    {
        this.prev = prev;
        this.curr = curr;
        this.changes = changes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal HashMapPatch(
        TrieMap<K, V> prev,
        TrieMap<K, V> curr,
        K key,
        Change<V> change)
    {
        this.prev = prev;
        this.curr = curr;
        this.key = key;
        this.change = change;
    }

    public HashMap<K, V> From
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(prev);
    }

    public HashMap<K, V> To
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(curr);
    }

    public HashMap<K, Change<V>> Changes
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => changes is null
                   ? HashMap<K, Change<V>>.Empty.Add(key!, change!)
                   : new HashMap<K, Change<V>>(changes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => 
        Changes.ToString();
}
