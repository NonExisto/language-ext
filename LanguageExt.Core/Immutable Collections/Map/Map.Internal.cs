﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static LanguageExt.Prelude;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using LanguageExt.ClassInstances;
using System.Runtime.CompilerServices;

namespace LanguageExt;

/// <summary>
/// Immutable map
/// AVL tree implementation
/// AVL tree is a self-balancing binary search tree. 
/// [wikipedia.org/wiki/AVL_tree](http://en.wikipedia.org/wiki/AVL_tree)
/// </summary>
/// <typeparam name="K">Key type</typeparam>
/// <typeparam name="V">Value type</typeparam>
[Serializable]
internal sealed class MapInternal<K, V> :
    IReadOnlyCollection<(K Key, V Value)>
{
    public static MapInternal<K, V> Empty => new (MapItem<K, V>.Empty, false, getRegisteredOrderComparerOrDefault<K>());

    internal readonly MapItem<K, V> Root;
    internal readonly bool Rev;

    private readonly IComparer<K> _comparer;
    int hashCode;

    /// <summary>
    /// Ctor
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MapInternal(IComparer<K>? comparer)
    {
        Root = MapItem<K, V>.Empty;
        _comparer = comparer ?? getRegisteredOrderComparerOrDefault<K>();
    }

    /// <summary>
    /// Ctor
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MapInternal(MapItem<K, V> root, bool rev, IComparer<K>? comparer)
    {
        Root = root;
        Rev = rev;
        _comparer = comparer ?? getRegisteredOrderComparerOrDefault<K>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MapInternal(ReadOnlySpan<(K Key, V Value)> items, MapModule.AddOpt option, IComparer<K>? comparer)
    {
        _comparer = comparer ?? getRegisteredOrderComparerOrDefault<K>();
        var root = MapItem<K, V>.Empty;
        var addMethod = MapInternal<K,V>.Translate(option);
        foreach (var (Key, Value) in items)
        {
            root = addMethod(root, Key, Value, _comparer);
        }
        Root = root;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MapInternal(IEnumerable<(K Key, V Value)> items, MapModule.AddOpt option, IComparer<K>? comparer)
    {
        _comparer = comparer ?? getRegisteredOrderComparerOrDefault<K>();
        var root = MapItem<K, V>.Empty;
        var addMethod = MapInternal<K,V>.Translate(option);
        foreach (var (Key, Value) in items)
        {
            root = addMethod(root, Key, Value, _comparer);
        }
        Root = root;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MapInternal(IEnumerable<KeyValuePair<K, V>> items, MapModule.AddOpt option, IComparer<K>? comparer)
    {
        _comparer = comparer ?? getRegisteredOrderComparerOrDefault<K>();
        var root = MapItem<K, V>.Empty;
        var addMethod = MapInternal<K,V>.Translate(option);
        foreach (var item in items)
        {
            root = addMethod(root, item.Key, item.Value, _comparer);
        }
        Root = root;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MapInternal(IEnumerable<Tuple<K, V>> items, MapModule.AddOpt option, IComparer<K>? comparer)
    {
        _comparer = comparer ?? getRegisteredOrderComparerOrDefault<K>();
        var root = MapItem<K, V>.Empty;
        var addMethod = MapInternal<K,V>.Translate(option);
        foreach (var item in items)
        {
            root = addMethod(root, item.Item1, item.Item2, _comparer);
        }
        Root = root;
    }

    private static Func<MapItem<K, V>, K, V, IComparer<K>, MapItem<K,V>> Translate(MapModule.AddOpt option){
        Func<MapItem<K, V>, K, V, IComparer<K>, MapItem<K,V>> addMethod = option switch
        {
            MapModule.AddOpt.ThrowOnDuplicate => MapModule.Add,
            MapModule.AddOpt.TryUpdate => MapModule.AddOrUpdate,
            MapModule.AddOpt.TryAdd => MapModule.TryAdd,
            _ => throw new NotSupportedException() 
        };
        return addMethod;
    }

    /// <summary>
    /// 'this' accessor
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Optional value</returns>
    [Pure]
    public V this[K key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Find(key).IfNone(() => failwith<V>("Key doesn't exist in map"));
    }

    /// <summary>
    /// Is the map empty
    /// </summary>
    [Pure]
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Count == 0;
    }

    /// <summary>
    /// Number of items in the map
    /// </summary>
    [Pure]
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Root.Count;
    }

    [Pure]
    public Option<(K, V)> Min
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Root.IsEmpty
                   ? None
                   : MapModule.Min(Root);
    }

    [Pure]
    public Option<(K, V)> Max
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Root.IsEmpty
                   ? None
                   : MapModule.Max(Root);
    }

    /// <summary>
    /// Get the hash code of all items in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() =>
        hashCode == 0
            ? (hashCode = FNV32.Hash<HashableTuple<HashableDefault<K>, HashableDefault<V>, K, V>, (K, V)>(AsIterable()))
            : hashCode;

    /// <summary>
    /// Atomically adds a new item to the map
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if the key already exists</exception>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the key or value are null</exception>
    /// <returns>New Map with the item added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> Add(K key, V value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        return SetRoot(MapModule.Add(Root, key, value, _comparer));
    }

    /// <summary>
    /// Atomically adds a new item to the map.
    /// If the key already exists, then the new item is ignored
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the key or value are null</exception>
    /// <returns>New Map with the item added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> TryAdd(K key, V value)
    {
        ArgumentNullException.ThrowIfNull(key);
        return SetRoot(MapModule.TryAdd<K, V>(Root, key, value, _comparer));
    }

    /// <summary>
    /// Atomically adds a new item to the map.
    /// If the key already exists then the Fail handler is called with the unaltered map 
    /// and the value already set for the key, it expects a new map returned.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <param name="Fail">Delegate to handle failure, you're given the unaltered map 
    /// and the value already set for the key</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the key or value are null</exception>
    /// <returns>New Map with the item added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> TryAdd(K key, V value, Func<MapInternal<K, V>, V, MapInternal<K, V>> Fail)
    {
        ArgumentNullException.ThrowIfNull(key);
        return Find(key, v => Fail(this, v), () => Add(key, value));
    }

    /// <summary>
    /// Atomically adds a range of items to the map.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of tuples to add</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if any of the keys already exist</exception>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    public MapInternal<K, V> AddRange(IEnumerable<Tuple<K, V>> range)
    {
        if (Count == 0)
        {
            return new MapInternal<K, V>(range, MapModule.AddOpt.ThrowOnDuplicate, _comparer);
        }

        var self = Root;
        foreach (var item in range)
        {
            ArgumentNullException.ThrowIfNull(item.Item1);
            self = MapModule.Add(self, item.Item1, item.Item2, _comparer);
        }
        return SetRoot(self);
    }

    /// <summary>
    /// Atomically adds a range of items to the map.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of tuples to add</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if any of the keys already exist</exception>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    public MapInternal<K, V> AddRange(IEnumerable<(K, V)> range)
    {
        if (Count == 0)
        {
            return new MapInternal<K, V>(range, MapModule.AddOpt.ThrowOnDuplicate, _comparer);
        }

        var self = Root;
        foreach (var item in range)
        {
            ArgumentNullException.ThrowIfNull(item.Item1);
            self = MapModule.Add(self, item.Item1, item.Item2, _comparer);
        }
        return SetRoot(self);
    }

    /// <summary>
    /// Atomically adds a range of items to the map.  If any of the keys exist already
    /// then they're ignored.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of tuples to add</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    public MapInternal<K, V> TryAddRange(IEnumerable<Tuple<K, V>> range)
    {
        if (Count == 0)
        {
            return new MapInternal<K, V>(range, MapModule.AddOpt.TryAdd, _comparer);
        }

        var self = Root;
        foreach (var item in range)
        {
            ArgumentNullException.ThrowIfNull(item.Item1);
            self = MapModule.TryAdd(self, item.Item1, item.Item2, _comparer);
        }
        return SetRoot(self);
    }

    /// <summary>
    /// Atomically adds a range of items to the map.  If any of the keys exist already
    /// then they're ignored.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of tuples to add</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    public MapInternal<K, V> TryAddRange(IEnumerable<(K, V)> range)
    {
        if (Count == 0)
        {
            return new MapInternal<K, V>(range, MapModule.AddOpt.TryAdd, _comparer);
        }

        var self = Root;
        foreach (var item in range)
        {
            ArgumentNullException.ThrowIfNull(item.Item1);
            self = MapModule.TryAdd(self, item.Item1, item.Item2, _comparer);
        }
        return SetRoot(self);
    }

    /// <summary>
    /// Atomically adds a range of items to the map.  If any of the keys exist already
    /// then they're ignored.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of KeyValuePairs to add</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    public MapInternal<K, V> TryAddRange(IEnumerable<KeyValuePair<K, V>> range)
    {
        if (Count == 0)
        {
            return new MapInternal<K, V>(range, MapModule.AddOpt.TryAdd, _comparer);
        }

        var self = Root;
        foreach (var item in range)
        {
            ArgumentNullException.ThrowIfNull(item.Key);
            self = MapModule.TryAdd(self, item.Key, item.Value, _comparer);
        }
        return SetRoot(self);
    }

    /// <summary>
    /// Atomically adds a range of items to the map.  If any of the keys exist already
    /// then they're replaced.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of tuples to add</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    public MapInternal<K, V> AddOrUpdateRange(IEnumerable<Tuple<K, V>> range)
    {
        if (Count == 0)
        {
            return new MapInternal<K, V>(range, MapModule.AddOpt.TryUpdate, _comparer);
        }

        var self = Root;
        foreach (var item in range)
        {
            ArgumentNullException.ThrowIfNull(item.Item1);
            self = MapModule.AddOrUpdate(self, item.Item1, item.Item2, _comparer);
        }
        return SetRoot(self);
    }

    /// <summary>
    /// Atomically adds a range of items to the map.  If any of the keys exist already
    /// then they're replaced.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of tuples to add</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    public MapInternal<K, V> AddOrUpdateRange(IEnumerable<(K, V)> range)
    {
        if (Count == 0)
        {
            return new MapInternal<K, V>(range, MapModule.AddOpt.TryUpdate, _comparer);
        }

        var self = Root;
        foreach (var item in range)
        {
            ArgumentNullException.ThrowIfNull(item.Item1);
            self = MapModule.AddOrUpdate(self, item.Item1, item.Item2, _comparer);
        }
        return SetRoot(self);
    }

    /// <summary>
    /// Atomically adds a range of items to the map.  If any of the keys exist already
    /// then they're replaced.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of KeyValuePairs to add</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    public MapInternal<K, V> AddOrUpdateRange(IEnumerable<KeyValuePair<K, V>> range)
    {
        if (Count == 0)
        {
            return new MapInternal<K, V>(range, MapModule.AddOpt.TryUpdate, _comparer);
        }

        var self = Root;
        foreach (var item in range)
        {
            ArgumentNullException.ThrowIfNull(item.Key);
            self = MapModule.AddOrUpdate(self, item.Key, item.Value, _comparer);
        }
        return SetRoot(self);
    }

    /// <summary>
    /// Atomically removes an item from the map
    /// If the key doesn't exists, the request is ignored.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>New map with the item removed</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> Remove(K key) =>
        isnull(key)
            ? this
            : SetRoot(MapModule.Remove(Root, key, _comparer));

    /// <summary>
    /// Retrieve a value from the map by key
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <returns>Found value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<V> Find(K key) =>
        isnull(key)
            ? None
            : MapModule.TryFind(Root, key, _comparer);

    /// <summary>
    /// Retrieve a value from the map by key as an enumerable
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <returns>Found value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Seq<V> FindSeq(K key) =>
        Find(key).ToSeq();

    /// <summary>
    /// Retrieve a value from the map by key and pattern match the
    /// result.
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <param name="Some">delegate to map the existing value to a new one before setting</param>
    /// <param name="None">delegate to return a new map if the item can't be found</param>
    /// <returns>Found value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public R Find<R>(K key, Func<V, R> Some, Func<R> None) =>
        isnull(key)
            ? None()
            : match(MapModule.TryFind(Root, key, _comparer), Some, None);

    /// <summary>
    /// Retrieve the value from predecessor item to specified key
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <returns>Found key</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<(K, V)> FindPredecessor(K key) => MapModule.TryFindPredecessor(Root, key, _comparer);

    /// <summary>
    /// Retrieve the value from exact key, or if not found, the predecessor item 
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <returns>Found key</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<(K, V)> FindOrPredecessor(K key) => MapModule.TryFindOrPredecessor(Root, key, _comparer);

    /// <summary>
    /// Retrieve the value from next item to specified key
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <returns>Found key</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<(K, V)> FindSuccessor(K key) => MapModule.TryFindSuccessor(Root, key, _comparer);

    /// <summary>
    /// Retrieve the value from exact key, or if not found, the next item 
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <returns>Found key</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<(K, V)> FindOrSuccessor(K key) => MapModule.TryFindOrSuccessor(Root, key, _comparer);


    /// <summary>
    /// Try to find the key in the map, if it doesn't exist, add a new 
    /// item by invoking the delegate provided.
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <param name="None">Delegate to get the value</param>
    /// <returns>Updated map and added value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (MapInternal<K, V> Map, V Value) FindOrAdd(K key, Func<V> None) =>
        Find(key).Match(
            Some: x => (this, x),
            None: () =>
                  {
                      var v = None();
                      return (Add(key, v), v);
                  });

    /// <summary>
    /// Try to find the key in the map, if it doesn't exist, add a new 
    /// item provided.
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <param name="value">Delegate to get the value</param>
    /// <returns>Updated map and added value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (MapInternal<K, V>, V Value) FindOrAdd(K key, V value) =>
        Find(key).Match(
            Some: x => (this, x),
            None: () => (Add(key, value), value));

    /// <summary>
    /// Try to find the key in the map, if it doesn't exist, add a new 
    /// item provided.
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <param name="value">Delegate to get the value</param>
    /// <returns>Updated map and added value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (MapInternal<K, V>, Option<V> Value) FindOrMaybeAdd(K key, Func<Option<V>> value) =>
        Find(key).Match(
            Some: x => (this, Optional(x)),
            None: () => value().Map(v => (Add(key, v), Optional(v)))
                               .IfNone((this, None)));

    /// <summary>
    /// Try to find the key in the map, if it doesn't exist, add a new 
    /// item provided.
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <param name="value">Delegate to get the value</param>
    /// <returns>Updated map and added value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (MapInternal<K, V>, Option<V> Value) FindOrMaybeAdd(K key, Option<V> value) =>
        Find(key).Match(
            Some: x => (this, Optional(x)),
            None: () => value.Map(v => (Add(key, v), Optional(v)))
                             .IfNone((this, None)));

    /// <summary>
    /// Atomically updates an existing item
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the key or value are null</exception>
    /// <returns>New Map with the item added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> SetItem(K key, V value)
    {
        ArgumentNullException.ThrowIfNull(key);
        return SetRoot(MapModule.SetItem(Root, key, value, _comparer));
    }

    /// <summary>
    /// Retrieve a value from the map by key, map it to a new value,
    /// put it back.
    /// </summary>
    /// <param name="key">Key to set</param>
    /// <param name="Some">Replace item action</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if the item isn't found</exception>
    /// <exception cref="Exception">Throws Exception if Some returns null</exception>
    /// <returns>New map with the mapped value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> SetItem(K key, Func<V, V> Some) =>
        isnull(key)
            ? this
            : match(MapModule.TryFind(Root, key, _comparer),
                    Some: x => SetItem(key, Some(x)),
                    None: () => throw new ArgumentException("Key not found in Map"));

    /// <summary>
    /// Atomically updates an existing item, unless it doesn't exist, in which case 
    /// it is ignored
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the value is null</exception>
    /// <returns>New Map with the item added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> TrySetItem(K key, V value)
    {
        if (isnull(key)) return this;
        return SetRoot(MapModule.TrySetItem(Root, key, value, _comparer));
    }

    /// <summary>
    /// Atomically sets an item by first retrieving it, applying a map, and then putting it back.
    /// Silently fails if the value doesn't exist
    /// </summary>
    /// <param name="key">Key to set</param>
    /// <param name="Some">delegate to map the existing value to a new one before setting</param>
    /// <exception cref="Exception">Throws Exception if Some returns null</exception>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the key or value are null</exception>
    /// <returns>New map with the item set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> TrySetItem(K key, Func<V, V> Some) =>
        isnull(key)
            ? this
            : match(MapModule.TryFind(Root, key, _comparer),
                    Some: x => SetItem(key, Some(x)),
                    None: () => this);

    /// <summary>
    /// Atomically sets an item by first retrieving it, applying a map, and then putting it back.
    /// Calls the None delegate to return a new map if the item can't be found
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="key">Key</param>
    /// <param name="Some">delegate to map the existing value to a new one before setting</param>
    /// <param name="None">delegate to return a new map if the item can't be found</param>
    /// <exception cref="Exception">Throws Exception if Some returns null</exception>
    /// <exception cref="Exception">Throws Exception if None returns null</exception>
    /// <returns>New map with the item set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> TrySetItem(K key, Func<V, V> Some, Func<Map<K, V>, Map<K, V>> None) =>
        isnull(key)
            ? this
            : match(MapModule.TryFind(Root, key, _comparer),
                    Some: x => SetItem(key, Some(x)),
                    None: () => this);

    /// <summary>
    /// Atomically adds a new item to the map.
    /// If the key already exists, the new item replaces it.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the key or value are null</exception>
    /// <returns>New Map with the item added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> AddOrUpdate(K key, V value)
    {
        ArgumentNullException.ThrowIfNull(key);
        return SetRoot(MapModule.AddOrUpdate(Root, key, value, _comparer));
    }


    /// <summary>
    /// Retrieve a value from the map by key, map it to a new value,
    /// put it back.  If it doesn't exist, add a new one based on None result.
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <param name="Some">delegate to map the existing value to a new one before setting</param>
    /// <param name="None">delegate to return a new map if the item can't be found</param>
    /// <exception cref="Exception">Throws Exception if None returns null</exception>
    /// <exception cref="Exception">Throws Exception if Some returns null</exception>
    /// <returns>New map with the mapped value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> AddOrUpdate(K key, Func<V, V> Some, Func<V> None) =>
        isnull(key)
            ? this
            : match(MapModule.TryFind(Root, key, _comparer),
                    Some: x => SetItem(key, Some(x)),
                    None: () => Add(key, None()));

    /// <summary>
    /// Retrieve a value from the map by key, map it to a new value,
    /// put it back.  If it doesn't exist, add a new one based on None result.
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <param name="Some">delegate to map the existing value to a new one before setting</param>
    /// <param name="None">new value if the item can't be found</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException if None is null</exception>
    /// <exception cref="Exception">Throws Exception if Some returns null</exception>
    /// <returns>New map with the mapped value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> AddOrUpdate(K key, Func<V, V> Some, V None)
    {
        ArgumentNullException.ThrowIfNull(None);

        return isnull(key)
                   ? this
                   : match(MapModule.TryFind(Root, key, _comparer),
                           Some: x => SetItem(key, Some(x)),
                           None: () => Add(key, None));
    }

    /// <summary>
    /// Retrieve a range of values 
    /// </summary>
    /// <param name="keyFrom">Range start (inclusive)</param>
    /// <param name="keyTo">Range to (inclusive)</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keyFrom or keyTo are null</exception>
    /// <returns>Range of values</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Iterable<V> FindRange(K keyFrom, K keyTo)
    {
        ArgumentNullException.ThrowIfNull(keyFrom);
        ArgumentNullException.ThrowIfNull(keyTo);
        return _comparer.Compare(keyFrom, keyTo) > 0
                   ? MapModule.FindRange(Root, keyTo, keyFrom, _comparer).AsIterable()
                   : MapModule.FindRange(Root, keyFrom, keyTo, _comparer).AsIterable();
    }

    /// <summary>
    /// Retrieve a range of values 
    /// </summary>
    /// <param name="keyFrom">Range start (inclusive)</param>
    /// <param name="keyTo">Range to (inclusive)</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keyFrom or keyTo are null</exception>
    /// <returns>Range of values</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Iterable<(K, V)> FindRangePairs(K keyFrom, K keyTo)
    {
        ArgumentNullException.ThrowIfNull(keyFrom);
        ArgumentNullException.ThrowIfNull(keyTo);
        return _comparer.Compare(keyFrom, keyTo) > 0
                   ? MapModule.FindRangePairs(Root, keyTo, keyFrom, _comparer).AsIterable()
                   : MapModule.FindRangePairs(Root, keyFrom, keyTo, _comparer).AsIterable();
    }

    /// <summary>
    /// Skips 'amount' values and returns a new tree without the 
    /// skipped values.
    /// </summary>
    /// <param name="amount">Amount to skip</param>
    /// <returns>New tree</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Iterable<(K Key, V Value)> Skip(int amount)
    {
        return Iterable.createRange(Go());

        IEnumerable<(K, V)> Go()
        {
            using var enumerator = new MapEnumerator<K, V>(Root, Rev, amount);
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }

    /// <summary>
    /// Checks for existence of a key in the map
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <returns>True if an item with the key supplied is in the map</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(K key) =>
        !isnull(key) && (Find(key) ? true : false);

    /// <summary>
    /// Checks for existence of a key in the map
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <param name="value">Value to check</param>
    /// <returns>True if an item with the key supplied is in the map</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(K key, V value) =>
        match(Find(key),
              Some: v => ReferenceEquals(v, value),
              None: () => false
        );

    /// <summary>
    /// Clears all items from the map 
    /// </summary>
    /// <remarks>Functionally equivalent to calling Map.empty as the original structure is untouched</remarks>
    /// <returns>Empty map</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> Clear() =>
        new(_comparer);

    /// <summary>
    /// Atomically adds a range of items to the map
    /// </summary>
    /// <param name="pairs">Range of KeyValuePairs to add</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if any of the keys already exist</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> AddRange(IEnumerable<KeyValuePair<K, V>> pairs) =>
        AddRange(pairs.AsIterable().Map(kv => (kv.Key, kv.Value)));

    /// <summary>
    /// Atomically sets a series of items using the KeyValuePairs provided
    /// </summary>
    /// <param name="items">Items to set</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if any of the keys aren't in the map</exception>
    /// <returns>New map with the items set</returns>
    [Pure]
    public MapInternal<K, V> SetItems(IEnumerable<KeyValuePair<K, V>> items)
    {
        if (Count == 0)
        {
            return new MapInternal<K, V>(items, MapModule.AddOpt.ThrowOnDuplicate, _comparer);
        }

        var self = Root;
        foreach (var item in items)
        {
            if (isnull(item.Key)) continue;
            self = MapModule.SetItem(self, item.Key, item.Value, _comparer);
        }
        return SetRoot(self);
    }

    /// <summary>
    /// Atomically sets a series of items using the Tuples provided.
    /// </summary>
    /// <param name="items">Items to set</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if any of the keys aren't in the map</exception>
    /// <returns>New map with the items set</returns>
    [Pure]
    public MapInternal<K, V> SetItems(IEnumerable<Tuple<K, V>> items)
    {
        if (Count == 0)
        {
            return new MapInternal<K, V>(items, MapModule.AddOpt.ThrowOnDuplicate, _comparer);
        }

        var self = Root;
        foreach (var item in items)
        {
            if (isnull(item.Item1)) continue;
            self = MapModule.SetItem(self, item.Item1, item.Item2, _comparer);
        }
        return SetRoot(self);
    }

    /// <summary>
    /// Atomically sets a series of items using the Tuples provided.
    /// </summary>
    /// <param name="items">Items to set</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if any of the keys aren't in the map</exception>
    /// <returns>New map with the items set</returns>
    [Pure]
    public MapInternal<K, V> SetItems(IEnumerable<(K, V)> items)
    {
        if (Count == 0)
        {
            return new MapInternal<K, V>(items, MapModule.AddOpt.ThrowOnDuplicate, _comparer);
        }

        var self = Root;
        foreach (var item in items)
        {
            if (isnull(item.Item1)) continue;
            self = MapModule.SetItem(self, item.Item1, item.Item2, _comparer);
        }
        return SetRoot(self);
    }

    /// <summary>
    /// Atomically sets a series of items using the KeyValuePairs provided.  If any of the 
    /// items don't exist then they're silently ignored.
    /// </summary>
    /// <param name="items">Items to set</param>
    /// <returns>New map with the items set</returns>
    [Pure]
    public MapInternal<K, V> TrySetItems(IEnumerable<KeyValuePair<K, V>> items)
    {
        if (Count == 0)
        {
            return new MapInternal<K, V>(items, MapModule.AddOpt.TryAdd, _comparer);
        }

        var self = Root;
        foreach (var item in items)
        {
            if (isnull(item.Key)) continue;
            self = MapModule.TrySetItem(self, item.Key, item.Value, _comparer);
        }
        return SetRoot(self);
    }

    /// <summary>
    /// Atomically sets a series of items using the Tuples provided  If any of the 
    /// items don't exist then they're silently ignored.
    /// </summary>
    /// <param name="items">Items to set</param>
    /// <returns>New map with the items set</returns>
    [Pure]
    public MapInternal<K, V> TrySetItems(IEnumerable<Tuple<K, V>> items)
    {
        if (Count == 0)
        {
            return new MapInternal<K, V>(items, MapModule.AddOpt.TryAdd, _comparer);
        }

        var self = Root;
        foreach (var item in items)
        {
            if (isnull(item.Item1)) continue;
            self = MapModule.TrySetItem(self, item.Item1, item.Item2, _comparer);
        }
        return SetRoot(self);
    }

    /// <summary>
    /// Atomically sets a series of items using the Tuples provided  If any of the 
    /// items don't exist then they're silently ignored.
    /// </summary>
    /// <param name="items">Items to set</param>
    /// <returns>New map with the items set</returns>
    [Pure]
    public MapInternal<K, V> TrySetItems(IEnumerable<(K, V)> items)
    {
        if (Count == 0)
        {
            return new MapInternal<K, V>(items, MapModule.AddOpt.TryAdd, _comparer);
        }

        var self = Root;
        foreach (var item in items)
        {
            if (isnull(item.Item1)) continue;
            self = MapModule.TrySetItem(self, item.Item1, item.Item2, _comparer);
        }
        return SetRoot(self);
    }

    /// <summary>
    /// Atomically sets a series of items using the keys provided to find the items
    /// and the Some delegate maps to a new value.  If the items don't exist then
    /// they're silently ignored.
    /// </summary>
    /// <param name="keys">Keys of items to set</param>
    /// <param name="Some">Function map the existing item to a new one</param>
    /// <returns>New map with the items set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> TrySetItems(IEnumerable<K> keys, Func<V, V> Some)
    {
        var self = this;
        foreach (var key in keys)
        {
            if (isnull(key)) continue;
            self = TrySetItem(key, Some);
        }
        return self;
    }

    /// <summary>
    /// Atomically removes a set of keys from the map
    /// </summary>
    /// <param name="keys">Keys to remove</param>
    /// <returns>New map with the items removed</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> RemoveRange(IEnumerable<K> keys)
    {
        var self = Root;
        foreach (var key in keys)
        {
            self = MapModule.Remove(self, key, _comparer);
        }
        return SetRoot(self);
    }

    /// <summary>
    /// Returns true if a Key/Value pair exists in the map
    /// </summary>
    /// <param name="pair">Pair to find</param>
    /// <returns>True if exists, false otherwise</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(KeyValuePair<K, V> pair) =>
        match(MapModule.TryFind(Root, pair.Key, _comparer),
              Some: v => ReferenceEquals(v, pair.Value),
              None: () => false);

    /// <summary>
    /// Equivalent to map and filter but the filtering is done based on whether the returned
    /// Option is Some or None.  If the item is None then it's filtered out, if not the the
    /// mapped Some value is used.
    /// </summary>
    /// <param name="selector">Predicate</param>
    /// <returns>Filtered map</returns>
    [Pure]
    public MapInternal<K, U> Choose<U>(Func<K, V, Option<U>> selector)
    {
        IEnumerable<(K, U)> Yield()
        {
            foreach (var item in this)
            {
                var opt = selector(item.Key, item.Value);
                if (opt.IsNone) continue;
                yield return (item.Key, (U)opt);
            }
        }
        return new MapInternal<K, U>(Yield(), MapModule.AddOpt.TryAdd, _comparer);
    }

    /// <summary>
    /// Equivalent to map and filter but the filtering is done based on whether the returned
    /// Option is Some or None.  If the item is None then it's filtered out, if not the the
    /// mapped Some value is used.
    /// </summary>
    /// <param name="selector">Predicate</param>
    /// <returns>Filtered map</returns>
    [Pure]
    public MapInternal<K, U> Choose<U>(Func<V, Option<U>> selector)
    {
        IEnumerable<(K, U)> Yield()
        {
            foreach (var item in this)
            {
                var opt = selector(item.Value);
                if (opt.IsNone) continue;
                yield return (item.Key, (U)opt);
            }
        }
        return new MapInternal<K, U>(Yield(), MapModule.AddOpt.TryAdd, _comparer);
    }

    /// <summary>
    /// Enumerable of map keys
    /// </summary>
    [Pure]
    public Iterable<K> Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return Iterable.createRange(Go());
            IEnumerable<K> Go()
            {
                using var iter = new MapKeyEnumerator<K, V>(Root, Rev, 0);
                while (iter.MoveNext())
                {
                    yield return iter.Current;
                }
            }
        }
    }

    /// <summary>
    /// Enumerable of map values
    /// </summary>
    [Pure]
    public Iterable<V> Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return Iterable.createRange(Go());
            IEnumerable<V> Go()
            {
                using var iter = new MapValueEnumerator<K, V>(Root, Rev, 0);
                while (iter.MoveNext())
                {
                    yield return iter.Current;
                }
            }
        }
    }

    /// <summary>
    /// Map to a dictionary
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IDictionary<KR, VR> ToDictionary<KR, VR>(Func<(K Key, V Value), KR> keySelector, Func<(K Key, V Value), VR> valueSelector) where KR : notnull =>
        AsIterable().ToDictionary(keySelector, valueSelector);

    /// <summary>
    /// Enumerable of in-order tuples that make up the map
    /// </summary>
    /// <returns>Tuples</returns>
    [Pure]
    public Iterable<(K Key, V Value)> Pairs
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => AsIterable().Map(kv => (kv.Key, kv.Value));
    }

    /// <summary>
    /// GetEnumerator - IEnumerable interface
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapEnumerator<K, V> GetEnumerator() =>
        new(Root, Rev, 0);

    /// <summary>
    /// GetEnumerator - IEnumerable interface
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator<(K Key, V Value)> IEnumerable<(K Key, V Value)>.GetEnumerator() =>
        new MapEnumerator<K, V>(Root, Rev, 0);

    /// <summary>
    /// GetEnumerator - IEnumerable interface
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() =>
        new MapEnumerator<K, V>(Root, Rev, 0);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Iterable<(K Key, V Value)> AsIterable()
    {
        return Iterable.createRange(Go());
        IEnumerable<(K, V)> Go()
        {
            using var iter = new MapEnumerator<K, V>(Root, Rev, 0);
            while (iter.MoveNext())
            {
                yield return iter.Current;
            }
        }
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<(K Key, V Value)> AsEnumerable()
    {
        using var iter = new MapEnumerator<K, V>(Root, Rev, 0);
        while (iter.MoveNext())
        {
            yield return iter.Current;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MapInternal<K, V> SetRoot(MapItem<K, V> root) =>
        new(root, Rev, _comparer);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MapInternal<K, V> operator +(MapInternal<K, V> lhs, MapInternal<K, V> rhs) =>
        lhs.Append(rhs);

    /// <summary>
    /// Union two maps.  The merge function is called keys are
    /// present in both map.
    /// </summary>
    [Pure]
    public MapInternal<K, R> Union<V2, R>(MapInternal<K, V2> other, WhenMissing<K, V, R> MapLeft, WhenMissing<K, V2, R> MapRight, WhenMatched<K, V, V2, R> Merge)
    {
        ArgumentNullException.ThrowIfNull(MapLeft);
        ArgumentNullException.ThrowIfNull(MapRight);
        ArgumentNullException.ThrowIfNull(Merge);

        var root = MapItem<K, R>.Empty;

        foreach (var right in other)
        {
            var key  = right.Key;
            var left = Find(key);
            if (left.IsSome)
            {
                root = MapModule.TryAdd(
                    root,
                    key,
                    Merge(key, left.Value!, right.Value),
                    _comparer);
            }
            else
            {
                root = MapModule.TryAdd(
                    root,
                    key,
                    MapRight(key, right.Value),
                    _comparer);
            }
        }
        foreach (var (Key, Value) in this)
        {
            var key   = Key;
            var right = other.Find(key);
            if (right.IsNone)
            {
                root = MapModule.TryAdd(
                    root,
                    key,
                    MapLeft(key, Value),
                    _comparer);
            }
        }
        return new MapInternal<K, R>(root, Rev, _comparer);
    }

    /// <summary>
    /// Intersect two maps.  Only keys that are in both maps are
    /// returned.  The merge function is called for every resulting
    /// key.
    /// </summary>
    [Pure]
    public MapInternal<K, R> Intersect<V2, R>(MapInternal<K, V2> other, WhenMatched<K, V, V2, R> Merge)
    {
        ArgumentNullException.ThrowIfNull(Merge);

        var root = MapItem<K, R>.Empty;

        foreach (var right in other)
        {
            var left = Find(right.Key);
            if (left.IsSome)
            {
                root = MapModule.TryAdd(
                    root,
                    right.Key,
                    Merge(right.Key, left.Value!, right.Value),
                    _comparer);
            }
        }
        return new MapInternal<K, R>(root, Rev, _comparer);
    }

    /// <summary>
    /// Map differencing based on key.  this - other.
    /// </summary>
    [Pure]
    public MapInternal<K, V> Except(MapInternal<K, V> other)
    {
        var root = MapItem<K, V>.Empty;
        foreach(var item in this)
        {
            if (!other.ContainsKey(item.Key))
            {
                root = MapModule.Add(
                    root,
                    item.Key,
                    item.Value,
                    _comparer);
            }
        }
        return new MapInternal<K, V>(root, Rev, _comparer);
    }

    /// <summary>
    /// Keys that are in both maps are dropped and the remaining
    /// items are merged and returned.
    /// </summary>
    [Pure]
    public MapInternal<K, V> SymmetricExcept(MapInternal<K, V> other)
    {
        var root = MapItem<K, V>.Empty;

        foreach (var left in this)
        {
            if (!other.ContainsKey(left.Key))
            {
                root = MapModule.Add(
                    root,
                    left.Key,
                    left.Value,
                    _comparer);
            }
        }
        foreach (var right in other)
        {
            if (!ContainsKey(right.Key))
            {
                //map = map.Add(right.Key, right.Value);
                root = MapModule.Add(
                    root,
                    right.Key,
                    right.Value,
                    _comparer);
            }
        }
        return new MapInternal<K, V>(root, Rev, _comparer);
    }

    [Pure]
    public MapInternal<K, V> Append(MapInternal<K, V> rhs)
    {
        if (Count == 0)
        {
            return rhs;
        }
        if (rhs.Count == 0)
        {
            return this;
        }

        var self = this;
        foreach (var item in rhs)
        {
            if (!self.ContainsKey(item.Key))
            {
                self = self.Add(item.Key, item.Value);
            }
        }
        return self;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MapInternal<K, V> operator -(MapInternal<K, V> lhs, MapInternal<K, V> rhs) =>
        lhs.Subtract(rhs);

    [Pure]
    public MapInternal<K, V> Subtract(MapInternal<K, V> rhs)
    {
        if(Count == 0)
        {
            return Empty;
        }

        if (rhs.Count == 0)
        {
            return this;
        }

        if (rhs.Count < Count)
        {
            var self = this;
            foreach (var item in rhs)
            {
                self = self.Remove(item.Key);
            }
            return self;
        }
        else
        {
            var root = MapItem<K, V>.Empty;
            foreach (var item in this)
            {
                if (!rhs.Contains(item))
                {
                    root = MapModule.TryAdd(root, item.Key, item.Value, _comparer);
                }
            }
            return new MapInternal<K, V>(root, Rev, _comparer);
        }
    }

    [Pure]
    public bool Equals(MapInternal<K, V> rhs, IEqualityComparer<K> equalityComparer, IEqualityComparer<V> valueEqualityComparer)
    {
        if (ReferenceEquals(this, rhs)) return true;
        if (Count != rhs.Count) return false;
        if (hashCode != 0 && rhs.hashCode != 0 && hashCode != rhs.hashCode) return false;

        using var iterA = GetEnumerator();
        using var iterB = rhs.GetEnumerator();
        var       count = Count;

        for (int i = 0; i < count; i++)
        {
            iterA.MoveNext();
            iterB.MoveNext();
            if (!equalityComparer.Equals(iterA.Current.Key, iterB.Current.Key)) return false;
            if (!valueEqualityComparer.Equals(iterA.Current.Value, iterB.Current.Value)) return false;
        }
        return true;
    }

    [Pure]
    public int CompareTo(MapInternal<K, V> other, IComparer<V> comparer)
    {
        var cmp = Count.CompareTo(other.Count);
        if (cmp != 0) return cmp;
        using var iterA = GetEnumerator();
        using var iterB = other.GetEnumerator();
        while (iterA.MoveNext() && iterB.MoveNext())
        {
            cmp = _comparer.Compare(iterA.Current.Key, iterB.Current.Key);
            if (cmp != 0) return cmp;
            cmp = comparer.Compare(iterA.Current.Value, iterB.Current.Value);
            if (cmp != 0) return cmp;
        }
        return 0;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> Filter(Func<K, V, bool> f) =>
        new(AsIterable().Filter(mi => f(mi.Key, mi.Value)), 
            MapModule.AddOpt.ThrowOnDuplicate, _comparer);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MapInternal<K, V> Filter(Func<V, bool> f) =>
        new(AsIterable().Filter(mi => f(mi.Value)),
            MapModule.AddOpt.ThrowOnDuplicate, _comparer);
}



[Serializable]
sealed class MapItem<K, V> : ISerializable
{
    internal static readonly MapItem<K, V> Empty = new (0, 0, (default!, default!), default!, default!);

    internal bool IsEmpty => Count == 0;
    internal int Count;
    internal byte Height;
    internal MapItem<K, V> Left;
    internal MapItem<K, V> Right;

    /// <summary>
    /// Ctor
    /// </summary>
    internal MapItem(byte height, int count, (K Key, V Value) keyValue, MapItem<K, V> left, MapItem<K, V> right)
    {
        Count = count;
        Height = height;
        KeyValue = keyValue;
        Left = left;
        Right = right;
    }

    /// <summary>
    /// Deserialisation constructor
    /// </summary>
    MapItem(SerializationInfo info, StreamingContext context)
    {
        var key   = (K?)info.GetValue("Key", typeof(K))   ?? throw new SerializationException();
        var value = (V?)info.GetValue("Value", typeof(V)) ?? throw new SerializationException();
        KeyValue = (key, value);
        Count = 1;
        Height = 1;
        Left = Empty;
        Right = Empty;
    }

    /// <summary>
    /// Serialisation support
    /// </summary>
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Key", KeyValue.Key, typeof(K));
        info.AddValue("Value", KeyValue.Value, typeof(V));
    }

    internal int BalanceFactor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Count == 0
                   ? 0
                   : Right.Height - Left.Height;
    }

    public (K Key, V Value) KeyValue
    {
        get;
        internal set;
    }
}

static class MapModule
{
    public enum AddOpt
    {
        ThrowOnDuplicate,
        TryAdd,
        TryUpdate
    }
    
    public static S Fold<S, K, V>(MapItem<K, V> node, S state, Func<S, K, V, S> folder)
    {
        if (node.IsEmpty)
        {
            return state;
        }

        state = Fold(node.Left, state, folder);
        state = folder(state, node.KeyValue.Key, node.KeyValue.Value);
        state = Fold(node.Right, state, folder);
        return state;
    }

    public static S Fold<S, K, V>(MapItem<K, V> node, S state, Func<S, V, S> folder)
    {
        if (node.IsEmpty)
        {
            return state;
        }

        state = Fold(node.Left, state, folder);
        state = folder(state, node.KeyValue.Value);
        state = Fold(node.Right, state, folder);
        return state;
    }

    public static bool ForAll<K, V>(MapItem<K, V> node, Func<K, V, bool> pred) =>
        node.IsEmpty || pred(node.KeyValue.Key, node.KeyValue.Value) && ForAll(node.Left, pred) && ForAll(node.Right, pred);

    public static bool Exists<K, V>(MapItem<K, V> node, Func<K, V, bool> pred) =>
        !node.IsEmpty && (pred(node.KeyValue.Key, node.KeyValue.Value) || Exists(node.Left, pred) || Exists(node.Right, pred));

    public static MapItem<K, V> Add<K, V>(MapItem<K, V> node, K key, V value, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            return new MapItem<K, V>(1, 1, (key, value), MapItem<K, V>.Empty, MapItem<K, V>.Empty);
        }
        var cmp = comparer.Compare(key, node.KeyValue.Key);
        return cmp switch
        {
            < 0 => Balance(Make(node.KeyValue, Add(node.Left, key, value, comparer), node.Right)),
            > 0 => Balance(Make(node.KeyValue, node.Left, Add(node.Right, key, value, comparer))),
            _ => throw new ArgumentException("An element with the same key already exists in the Map")
        };
    }

    public static MapItem<K, V> SetItem<K, V>(MapItem<K, V> node, K key, V value, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            throw new ArgumentException("Key not found in Map");
        }
        var cmp = comparer.Compare(key, node.KeyValue.Key);
        return cmp switch
        {
            < 0 => Balance(Make(node.KeyValue, SetItem(node.Left, key, value, comparer), node.Right)),
            > 0 => Balance(Make(node.KeyValue, node.Left, SetItem(node.Right, key, value, comparer))),
            _ => new MapItem<K, V>(node.Height, node.Count, (key, value), node.Left, node.Right)
        };
    }

    public static MapItem<K, V> TrySetItem<K, V>(MapItem<K, V> node, K key, V value, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            return node;
        }
        var cmp = comparer.Compare(key, node.KeyValue.Key);
        return cmp switch
        {
            < 0 => Balance(Make(node.KeyValue, TrySetItem(node.Left, key, value, comparer), node.Right)),
            > 0 => Balance(Make(node.KeyValue, node.Left, TrySetItem(node.Right, key, value, comparer))),
            _ => new MapItem<K, V>(node.Height, node.Count, (key, value), node.Left, node.Right)
        };
    }

    public static MapItem<K, V> TryAdd<K, V>(MapItem<K, V> node, K key, V value, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            return new MapItem<K, V>(1, 1, (key, value), MapItem<K, V>.Empty, MapItem<K, V>.Empty);
        }
        var cmp = comparer.Compare(key, node.KeyValue.Key);
        return cmp switch
        {
            < 0 => Balance(Make(node.KeyValue, TryAdd(node.Left, key, value, comparer), node.Right)),
            > 0 => Balance(Make(node.KeyValue, node.Left, TryAdd(node.Right, key, value, comparer))),
            _ => node
        };
    }

    public static MapItem<K, V> AddOrUpdate<K, V>(MapItem<K, V> node, K key, V value, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            return new MapItem<K, V>(1, 1, (key, value), MapItem<K, V>.Empty, MapItem<K, V>.Empty);
        }
        var cmp = comparer.Compare(key, node.KeyValue.Key);
        return cmp switch
        {
            < 0 => Balance(Make(node.KeyValue, AddOrUpdate(node.Left, key, value, comparer), node.Right)),
            > 0 => Balance(Make(node.KeyValue, node.Left, AddOrUpdate(node.Right, key, value, comparer))),
            _ => new MapItem<K, V>(node.Height, node.Count, (node.KeyValue.Key, value), node.Left, node.Right)
        };
    }

    public static MapItem<K, V> Remove<K, V>(MapItem<K, V> node, K key, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            return node;
        }
        var cmp = comparer.Compare(key, node.KeyValue.Key);
        if (cmp < 0)
        {
            return Balance(Make(node.KeyValue, Remove<K, V>(node.Left, key, comparer), node.Right));
        }
        else if (cmp > 0)
        {
            return Balance(Make(node.KeyValue, node.Left, Remove<K, V>(node.Right, key, comparer)));
        }
        else
        {
            // If this is a leaf, just remove it 
            // by returning Empty.  If we have only one child,
            // replace the node with the child.
            if (node.Right.IsEmpty && node.Left.IsEmpty)
            {
                return MapItem<K, V>.Empty;
            }
            else if (node.Right.IsEmpty && !node.Left.IsEmpty)
            {
                return node.Left;
            }
            else if (!node.Right.IsEmpty && node.Left.IsEmpty)
            {
                return node.Right;
            }
            else
            {
                // We have two children. Remove the next-highest node and replace
                // this node with it.
                var successor = node.Right;
                while (!successor.Left.IsEmpty)
                {
                    successor = successor.Left;
                }

                var newRight = Remove<K, V>(node.Right, successor.KeyValue.Key, comparer);
                return Balance(Make(successor.KeyValue, node.Left, newRight));
            }
        }
    }

    public static V Find<K, V>(MapItem<K, V> node, K key, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            throw new ArgumentException("Key not found in Map");
        }
        var cmp = comparer.Compare(key, node.KeyValue.Key);
        return cmp switch
        {
            < 0 => Find(node.Left, key, comparer),
            > 0 => Find(node.Right, key, comparer),
            _ => node.KeyValue.Value
        };
    }

    /// <summary>
    /// TODO: I suspect this is suboptimal, it would be better with a custom Enumerator 
    /// that maintains a stack of nodes to retrace.
    /// </summary>
    public static IEnumerable<V> FindRange<K, V>(MapItem<K, V> node, K a, K b, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            yield break;
        }
        if (comparer.Compare(node.KeyValue.Key, a) < 0)
        {
            foreach (var item in FindRange<K, V>(node.Right, a, b, comparer))
            {
                yield return item;
            }
        }
        else if (comparer.Compare(node.KeyValue.Key, b) > 0)
        {
            foreach (var item in FindRange<K, V>(node.Left, a, b, comparer))
            {
                yield return item;
            }
        }
        else
        {
            foreach (var item in FindRange<K, V>(node.Left, a, b, comparer))
            {
                yield return item;
            }
            yield return node.KeyValue.Value;
            foreach (var item in FindRange<K, V>(node.Right, a, b, comparer))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// TODO: I suspect this is suboptimal, it would be better with a custom Enumerator 
    /// that maintains a stack of nodes to retrace.
    /// </summary>
    public static IEnumerable<(K, V)> FindRangePairs<K, V>(MapItem<K, V> node, K a, K b, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            yield break;
        }
        if (comparer.Compare(node.KeyValue.Key, a) < 0)
        {
            foreach (var item in FindRangePairs<K, V>(node.Right, a, b, comparer))
            {
                yield return item;
            }
        }
        else if (comparer.Compare(node.KeyValue.Key, b) > 0)
        {
            foreach (var item in FindRangePairs<K, V>(node.Left, a, b, comparer))
            {
                yield return item;
            }
        }
        else
        {
            foreach (var item in FindRangePairs<K, V>(node.Left, a, b, comparer))
            {
                yield return item;
            }
            yield return node.KeyValue;
            foreach (var item in FindRangePairs<K, V>(node.Right, a, b, comparer))
            {
                yield return item;
            }
        }
    }

    public static Option<V> TryFind<K, V>(MapItem<K, V> node, K key, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            return None;
        }
        var cmp = comparer.Compare(key, node.KeyValue.Key);
        return cmp switch
        {
            < 0 => TryFind(node.Left, key, comparer),
            > 0 => TryFind(node.Right, key, comparer),
            _ => Optional(node.KeyValue.Value)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MapItem<K, V> Make<K, V>((K,V) kv, MapItem<K, V> l, MapItem<K, V> r) =>
        new ((byte)(1 + Math.Max(l.Height, r.Height)), l.Count + r.Count + 1, kv, l, r);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MapItem<K, V> Make<K, V>(K k, V v, MapItem<K, V> l, MapItem<K, V> r) =>
        new ((byte)(1 + Math.Max(l.Height, r.Height)), l.Count + r.Count + 1, (k, v), l, r);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MapItem<K, V> Balance<K, V>(MapItem<K, V> node) =>
        node.BalanceFactor >= 2
            ? node.Right.BalanceFactor < 0
                  ? DblRotLeft(node)
                  : RotLeft(node)
            : node.BalanceFactor <= -2
                ? node.Left.BalanceFactor > 0
                      ? DblRotRight(node)
                      : RotRight(node)
                : node;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MapItem<K, V> RotRight<K, V>(MapItem<K, V> node) =>
        node.IsEmpty || node.Left.IsEmpty
            ? node
            : Make(node.Left.KeyValue, node.Left.Left, Make(node.KeyValue, node.Left.Right, node.Right));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MapItem<K, V> RotLeft<K, V>(MapItem<K, V> node) =>
        node.IsEmpty || node.Right.IsEmpty
            ? node
            : Make(node.Right.KeyValue, Make(node.KeyValue, node.Left, node.Right.Left), node.Right.Right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MapItem<K, V> DblRotRight<K, V>(MapItem<K, V> node) =>
        node.IsEmpty || node.Left.IsEmpty
            ? node
            : RotRight(Make(node.KeyValue, RotLeft(node.Left), node.Right));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MapItem<K, V> DblRotLeft<K, V>(MapItem<K, V> node) =>
        node.IsEmpty || node.Right.IsEmpty
            ? node
            : RotLeft(Make(node.KeyValue, node.Left, RotRight(node.Right)));

    internal static Option<(K, V)> Max<K, V>(MapItem<K, V> node) =>
        node.Right.IsEmpty
            ? node.KeyValue
            : Max(node.Right);

    internal static Option<(K, V)> Min<K, V>(MapItem<K, V> node) =>
        node.Left.IsEmpty
            ? node.KeyValue
            : Min(node.Left);

    internal static Option<(K, V)> TryFindPredecessor<K, V>(MapItem<K, V> root, K key, IComparer<K> comparer) 
    {
        Option<(K, V)> predecessor = None;
        var            current     = root;

        if (root.IsEmpty)
        {
            return None;
        }

        do
        {
            var cmp = comparer.Compare(key, current.KeyValue.Key);
            if (cmp < 0)
            {
                current = current.Left;
            }
            else if (cmp > 0)
            {
                predecessor = current.KeyValue;
                current = current.Right;
            }
            else
            {
                break;
            }
        }
        while (!current.IsEmpty);

        if (!current.IsEmpty && !current.Left.IsEmpty)
        {
            predecessor = Max(current.Left);
        }

        return predecessor;
    }

    internal static Option<(K, V)> TryFindOrPredecessor<K, V>(MapItem<K, V> root, K key, IComparer<K> comparer) 
    {
        Option<(K, V)> predecessor = None;
        var            current     = root;

        if (root.IsEmpty)
        {
            return None;
        }

        do
        {
            var cmp = comparer.Compare(key, current.KeyValue.Key);
            if (cmp < 0)
            {
                current = current.Left;
            }
            else if (cmp > 0)
            {
                predecessor = current.KeyValue;
                current = current.Right;
            }
            else
            {
                return current.KeyValue;
            }
        }
        while (!current.IsEmpty);

        if (!current.IsEmpty && !current.Left.IsEmpty)
        {
            predecessor = Max(current.Left);
        }

        return predecessor;
    }

    internal static Option<(K, V)> TryFindSuccessor<K, V>(MapItem<K, V> root, K key, IComparer<K> comparer)
    {
        Option<(K, V)> successor = None;
        var            current   = root;

        if (root.IsEmpty)
        {
            return None;
        }

        do
        {
            var cmp = comparer.Compare(key, current.KeyValue.Key);
            if (cmp < 0)
            {
                successor = current.KeyValue;
                current = current.Left;
            }
            else if (cmp > 0)
            {
                current = current.Right;
            }
            else
            {
                break;
            }
        }
        while (!current.IsEmpty);

        if (!current.IsEmpty && !current.Right.IsEmpty)
        {
            successor = Min(current.Right);
        }

        return successor;        }

    internal static Option<(K, V)> TryFindOrSuccessor<K, V>(MapItem<K, V> root, K key, IComparer<K> comparer) 
    {
        Option<(K, V)> successor = None;
        var            current   = root;

        if (root.IsEmpty)
        {
            return None;
        }

        do
        {
            var cmp = comparer.Compare(key, current.KeyValue.Key);
            if (cmp < 0)
            {
                successor = current.KeyValue;
                current = current.Left;
            }
            else if (cmp > 0)
            {
                current = current.Right;
            }
            else
            {
                return current.KeyValue;
            }
        }
        while (!current.IsEmpty);

        if (!current.IsEmpty && !current.Right.IsEmpty)
        {
            successor = Min(current.Right);
        }

        return successor;
    }
}
