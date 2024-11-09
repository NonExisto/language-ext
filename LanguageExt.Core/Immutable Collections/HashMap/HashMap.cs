﻿using LanguageExt.ClassInstances;
using static LanguageExt.Prelude;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using LanguageExt.Traits;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace LanguageExt;

/// <summary>
/// Unsorted immutable hash-map
/// </summary>
/// <typeparam name="K">Key type</typeparam>
/// <typeparam name="V">Value</typeparam>
[CollectionBuilder(typeof(HashMap), nameof(HashMap.createRange))]
public readonly struct HashMap<K, V> :
    IReadOnlyDictionary<K, V>,
    IEnumerable<(K Key, V Value)>,
    IEquatable<HashMap<K, V>>,
    IEqualityOperators<HashMap<K, V>, HashMap<K, V>, bool>,
    IAdditionOperators<HashMap<K, V>, HashMap<K, V>, HashMap<K, V>>,
    ISubtractionOperators<HashMap<K, V>, HashMap<K, V>, HashMap<K, V>>,
    IAdditiveIdentity<HashMap<K, V>, HashMap<K, V>>,
    Monoid<HashMap<K, V>>,
    K<HashMap<K>, V>
{
    [Pure] 
    public static HashMap<K, V> Empty { get; } = new(TrieMap<K, V>.Empty());

    readonly TrieMap<K, V>? _value;

    internal TrieMap<K, V> Value => 
        _value ?? TrieMap<K, V>.Empty();

    internal HashMap(IEqualityComparer<K>? equalityComparer)
    {
        _value = TrieMap<K, V>.Empty(equalityComparer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal HashMap(TrieMap<K, V> value) =>
        _value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap(IEnumerable<(K Key, V Value)> items) 
        : this(items, true)
    { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap(IEnumerable<(K Key, V Value)> items, bool tryAdd, IEqualityComparer<K>? equalityComparer = null) =>
        _value = new TrieMap<K, V>(items, tryAdd, equalityComparer);

    /// <summary>
    /// Item at index lens
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Lens<HashMap<K, V>, V> item(K key) => Lens<HashMap<K, V>, V>.New(
        Get: la => la[key!],
        Set: a => la => la.AddOrUpdate(key!, a)
    );

    /// <summary>
    /// Item or none at index lens
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Lens<HashMap<K, V>, Option<V>> itemOrNone(K key) => Lens<HashMap<K, V>, Option<V>>.New(
        Get: la => la.Find(key),
        Set: a => la => a.Match(Some: x => la.AddOrUpdate(key!, x), None: () => la.Remove(key))
    );

    /// <summary>
    /// Lens map
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Lens<HashMap<K, V>, HashMap<K, B>> map<B>(Lens<V, B> lens) => Lens<HashMap<K, V>, HashMap<K, B>>.New(
        Get: la => la.Map(lens.Get),
        Set: lb => la =>
                   {
                       foreach (var item in lb)
                       {
                           la = la.AddOrUpdate(item.Key!, lens.Set(item.Value, la[item.Key!]));
                       }
                       return la;
                   });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static HashMap<K, V> Wrap(TrieMap<K, V> value) =>
        new (value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static HashMap<K, U> Wrap<U>(TrieMap<K, U> value) =>
        new (value);

    /// <summary>
    /// 'this' accessor
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Optional value</returns>
    [Pure]
    public V this[K key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value[key!];
    }
    
    /// <summary>
    /// Is the map empty
    /// </summary>
    [Pure]
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value?.IsEmpty ?? true;
    }

    /// <summary>
    /// Number of items in the map
    /// </summary>
    [Pure]
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value?.Count ?? 0;
    }

    /// <summary>
    /// Atomically filter out items that return false when a predicate is applied
    /// </summary>
    /// <param name="pred">Predicate</param>
    /// <returns>New map with items filtered</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Filter(Func<V, bool> pred) =>
        Wrap(Value.Filter(pred));

    /// <summary>
    /// Atomically filter out items that return false when a predicate is applied
    /// </summary>
    /// <param name="pred">Predicate</param>
    /// <returns>New map with items filtered</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Filter(Func<K, V, bool> pred) =>
        Wrap(Value.Filter(pred));

    /// <summary>
    /// Atomically maps the map to a new map
    /// </summary>
    /// <returns>Mapped items in a new map</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, U> Map<U>(Func<V, U> mapper) =>
        Wrap(Value.Map(mapper));

    /// <summary>
    /// Atomically maps the map to a new map
    /// </summary>
    /// <returns>Mapped items in a new map</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, U> Map<U>(Func<K, V, U> mapper) =>
        Wrap(Value.Map(mapper));

    /// <summary>
    /// Atomically adds a new item to the map
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="key">Key</param>
    /// <param name="valueToAdd">Value</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if the key already exists</exception>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the key or value are null</exception>
    /// <returns>New Map with the item added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Add(K key, V valueToAdd) =>
        Wrap(Value.Add(key, valueToAdd));

    /// <summary>
    /// Atomically adds a new item to the map.
    /// If the key already exists, then the new item is ignored
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="key">Key</param>
    /// <param name="valueToAdd">Value</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the key or value are null</exception>
    /// <returns>New Map with the item added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> TryAdd(K key, V valueToAdd) =>
        Wrap(Value.TryAdd(key, valueToAdd));

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
    public HashMap<K, V> AddOrUpdate(K key, V value) =>
        Wrap(Value.AddOrUpdate(key, value));

    /// <summary>
    /// Retrieve a value from the map by key, map it to a new value,
    /// put it back.  If it doesn't exist, add a new one based on None result.
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <exception cref="Exception">Throws Exception if None returns null</exception>
    /// <exception cref="Exception">Throws Exception if Some returns null</exception>
    /// <returns>New map with the mapped value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> AddOrUpdate(K key, Func<V, V> Some, Func<V> None) =>
        Wrap(Value.AddOrUpdate(key, Some, None));

    /// <summary>
    /// Retrieve a value from the map by key, map it to a new value,
    /// put it back.  If it doesn't exist, add a new one based on None result.
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException if None is null</exception>
    /// <exception cref="Exception">Throws Exception if Some returns null</exception>
    /// <returns>New map with the mapped value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> AddOrUpdate(K key, Func<V, V> Some, V None) =>
        Wrap(Value.AddOrUpdate(key, Some, None));

    /// <summary>
    /// Atomically adds a range of items to the map.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of tuples to add</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if any of the keys already exist</exception>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> AddRange(IEnumerable<Tuple<K, V>> range) =>
        Wrap(Value.AddRange(range));

    /// <summary>
    /// Atomically adds a range of items to the map.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of tuples to add</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if any of the keys already exist</exception>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> AddRange(IEnumerable<(K Key, V Value)> range) =>
        Wrap(Value.AddRange(range));

    /// <summary>
    /// Atomically adds a range of items to the map.  If any of the keys exist already
    /// then they're ignored.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of tuples to add</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> TryAddRange(IEnumerable<Tuple<K, V>> range) =>
        Wrap(Value.TryAddRange(range));

    /// <summary>
    /// Atomically adds a range of items to the map.  If any of the keys exist already
    /// then they're ignored.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of tuples to add</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> TryAddRange(IEnumerable<(K Key, V Value)> range) =>
        Wrap(Value.TryAddRange(range));

    /// <summary>
    /// Atomically adds a range of items to the map.  If any of the keys exist already
    /// then they're ignored.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of KeyValuePairs to add</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> TryAddRange(IEnumerable<KeyValuePair<K, V>> range) =>
        Wrap(Value.TryAddRange(range));

    /// <summary>
    /// Atomically adds a range of items to the map.  If any of the keys exist already
    /// then they're replaced.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of tuples to add</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> AddOrUpdateRange(IEnumerable<Tuple<K, V>> range) =>
        Wrap(Value.AddOrUpdateRange(range));

    /// <summary>
    /// Atomically adds a range of items to the map.  If any of the keys exist already
    /// then they're replaced.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of tuples to add</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> AddOrUpdateRange(IEnumerable<(K Key, V Value)> range) =>
        Wrap(Value.AddOrUpdateRange(range));

    /// <summary>
    /// Atomically adds a range of items to the map.  If any of the keys exist already
    /// then they're replaced.
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="range">Range of KeyValuePairs to add</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keys or values are null</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> AddOrUpdateRange(IEnumerable<KeyValuePair<K, V>> range) =>
        Wrap(Value.AddOrUpdateRange(range));

    /// <summary>
    /// Atomically removes an item from the map
    /// If the key doesn't exists, the request is ignored.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>New map with the item removed</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Remove(K key) =>
        Wrap(Value.Remove(key));

    /// <summary>
    /// Retrieve a value from the map by key
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <returns>Found value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<V> Find(K key) =>
        Value.Find(key);

    /// <summary>
    /// Retrieve a value from the map by key and pattern match the
    /// result.
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <returns>Found value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public R Find<R>(K key, Func<V, R> Some, Func<R> None) =>
        Value.Find(key, Some, None);

    /// <summary>
    /// Try to find the key in the map, if it doesn't exist, add a new 
    /// item by invoking the delegate provided.
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <param name="None">Delegate to get the value</param>
    /// <returns>Updated map and added value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (HashMap<K, V> Map, V Value) FindOrAdd(K key, Func<V> None) =>
        Value.FindOrAdd(key, None).Map((x, y) => (Wrap(x), y));

    /// <summary>
    /// Try to find the key in the map, if it doesn't exist, add a new 
    /// item provided.
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <param name="valueToFindOrAdd">value</param>
    /// <returns>Updated map and added value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (HashMap<K, V>, V Value) FindOrAdd(K key, V valueToFindOrAdd) =>
        Value.FindOrAdd(key, valueToFindOrAdd).Map((x, y) => (Wrap(x), y));

    /// <summary>
    /// Try to find the key in the map, if it doesn't exist, add a new 
    /// item by invoking the delegate provided.
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <param name="None">Delegate to get the value</param>
    /// <returns>Updated map and added value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (HashMap<K, V> Map, Option<V> Value) FindOrMaybeAdd(K key, Func<Option<V>> None) =>
        Value.FindOrMaybeAdd(key, None).Map((x, y) => (Wrap(x), y));

    /// <summary>
    /// Try to find the key in the map, if it doesn't exist, add a new 
    /// item by invoking the delegate provided.
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <param name="None">Delegate to get the value</param>
    /// <returns>Updated map and added value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (HashMap<K, V> Map, Option<V> Value) FindOrMaybeAdd(K key, Option<V> None) =>
        Value.FindOrMaybeAdd(key, None).Map((x, y) => (Wrap(x), y));

    /// <summary>
    /// Atomically updates an existing item
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="key">Key</param>
    /// <param name="valueToSet">Value</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the key or value are null</exception>
    /// <returns>New Map with the item added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> SetItem(K key, V valueToSet) =>
        Wrap(Value.SetItem(key, valueToSet));

    /// <summary>
    /// Retrieve a value from the map by key, map it to a new value,
    /// put it back.
    /// </summary>
    /// <param name="key">Key to set</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if the item isn't found</exception>
    /// <exception cref="Exception">Throws Exception if Some returns null</exception>
    /// <returns>New map with the mapped value</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> SetItem(K key, Func<V, V> Some) =>
        Wrap(Value.SetItem(key, Some));

    /// <summary>
    /// Atomically updates an existing item, unless it doesn't exist, in which case 
    /// it is ignored
    /// </summary>
    /// <remarks>Null is not allowed for a Key or a Value</remarks>
    /// <param name="key">Key</param>
    /// <param name="valueToSet">Value</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the value is null</exception>
    /// <returns>New Map with the item added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> TrySetItem(K key, V valueToSet) =>
        Wrap(Value.TrySetItem(key, valueToSet));

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
    public HashMap<K, V> TrySetItem(K key, Func<V, V> Some) =>
        Wrap(Value.TrySetItem(key, Some));

    /// <summary>
    /// Checks for existence of a key in the map
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <returns>True if an item with the key supplied is in the map</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(K key) =>
        Value.ContainsKey(key);

    /// <summary>
    /// Checks for existence of a key and value in the map
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <param name="value">Value to check</param>
    /// <returns>True if an item with the key supplied is in the map</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(K key, V value) =>
        Value.Contains(key, value);

    /// <summary>
    /// Checks for existence of a value in the map
    /// </summary>
    /// <param name="value">Value to check</param>
    /// <returns>True if an item with the value supplied is in the map</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(V value, IEqualityComparer<V> equalityComparer)  =>
        Value.Contains(value, equalityComparer);

    /// <summary>
    /// Checks for existence of a key and value in the map
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <param name="value">Value to check</param>
    /// <returns>True if an item with the key supplied is in the map</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(K key, V value, IEqualityComparer<V> equalityComparer)  =>
        Value.Contains(key, value, equalityComparer);

    /// <summary>
    /// Clears all items from the set
    /// </summary>
    /// <remarks>Functionally equivalent to calling HSet.empty as the original structure is untouched</remarks>
    /// <returns>Empty HSet</returns>
    [Pure]
    public HashMap<K, V> Clear() =>
        new(Value.Clear());

    /// <summary>
    /// Atomically adds a range of items to the map
    /// </summary>
    /// <param name="pairs">Range of KeyValuePairs to add</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if any of the keys already exist</exception>
    /// <returns>New Map with the items added</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> AddRange(IEnumerable<KeyValuePair<K, V>> pairs) =>
        Wrap(Value.AddRange(pairs));

    /// <summary>
    /// Atomically sets a series of items using the KeyValuePairs provided
    /// </summary>
    /// <param name="items">Items to set</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if any of the keys aren't in the map</exception>
    /// <returns>New map with the items set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> SetItems(IEnumerable<KeyValuePair<K, V>> items) =>
        Wrap(Value.SetItems(items));

    /// <summary>
    /// Atomically sets a series of items using the Tuples provided.
    /// </summary>
    /// <param name="items">Items to set</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if any of the keys aren't in the map</exception>
    /// <returns>New map with the items set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> SetItems(IEnumerable<Tuple<K, V>> items) =>
        Wrap(Value.SetItems(items));

    /// <summary>
    /// Atomically sets a series of items using the Tuples provided.
    /// </summary>
    /// <param name="items">Items to set</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if any of the keys aren't in the map</exception>
    /// <returns>New map with the items set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> SetItems(IEnumerable<(K Key, V Value)> items) =>
        Wrap(Value.SetItems(items));

    /// <summary>
    /// Atomically sets a series of items using the KeyValuePairs provided.  If any of the 
    /// items don't exist then they're silently ignored.
    /// </summary>
    /// <param name="items">Items to set</param>
    /// <returns>New map with the items set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> TrySetItems(IEnumerable<KeyValuePair<K, V>> items) =>
        Wrap(Value.TrySetItems(items));

    /// <summary>
    /// Atomically sets a series of items using the Tuples provided  If any of the 
    /// items don't exist then they're silently ignored.
    /// </summary>
    /// <param name="items">Items to set</param>
    /// <returns>New map with the items set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> TrySetItems(IEnumerable<Tuple<K, V>> items) =>
        Wrap(Value.TrySetItems(items));

    /// <summary>
    /// Atomically sets a series of items using the Tuples provided  If any of the 
    /// items don't exist then they're silently ignored.
    /// </summary>
    /// <param name="items">Items to set</param>
    /// <returns>New map with the items set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> TrySetItems(IEnumerable<(K Key, V Value)> items) =>
        Wrap(Value.TrySetItems(items));

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
    public HashMap<K, V> TrySetItems(IEnumerable<K> keys, Func<V, V> Some) =>
        Wrap(Value.TrySetItems(keys, Some));

    /// <summary>
    /// Atomically removes a set of keys from the map
    /// </summary>
    /// <param name="keys">Keys to remove</param>
    /// <returns>New map with the items removed</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> RemoveRange(IEnumerable<K> keys) =>
        Wrap(Value.RemoveRange(keys));

    /// <summary>
    /// Returns true if a Key/Value pair exists in the map
    /// </summary>
    /// <param name="pair">Pair to find</param>
    /// <returns>True if exists, false otherwise</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains((K Key, V Value) pair) =>
        Value.Contains(pair.Key!, pair.Value);

    /// <summary>
    /// Enumerable of map keys
    /// </summary>
    [Pure]
    public Iterable<K> Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value.Keys;
    }

    /// <summary>
    /// Enumerable of map values
    /// </summary>
    [Pure]
    public Iterable<V> Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value.Values;
    }

    /// <summary>
    /// Map the map the a dictionary
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IDictionary<KR, VR> ToDictionary<KR, VR>(
        Func<(K Key, V Value), KR> keySelector, 
        Func<(K Key, V Value), VR> valueSelector)
        where KR : notnull =>
        AsIterable().ToDictionary(x => keySelector(x), x => valueSelector(x));

    /// <summary>
    /// GetEnumerator - IEnumerable interface
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<(K Key, V Value)> GetEnumerator() =>
        Value.GetEnumerator();

    /// <summary>
    /// GetEnumerator - IEnumerable interface
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() =>
        // ReSharper disable once NotDisposedResourceIsReturned
        Value.GetEnumerator();

    /// <summary>
    /// Allocation free conversion to a TrackingHashMap
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrackingHashMap<K, V> ToTrackingHashMap() =>
        new (Value);

    /// <summary>
    /// Format the collection as `[(key: value), (key: value), (key: value), ...]`
    /// The ellipsis is used for collections over 50 items
    /// To get a formatted string with all the items, use `ToFullString`
    /// or `ToFullArrayString`.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() =>
        CollectionFormat.ToShortArrayString(AsIterable().Map(kv => $"({kv.Key}: {kv.Value})"), Count);

    /// <summary>
    /// Format the collection as `(key: value), (key: value), (key: value), ...`
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToFullString(string separator = ", ") =>
        CollectionFormat.ToFullString(AsIterable().Map(kv => $"({kv.Key}: {kv.Value})"), separator);

    /// <summary>
    /// Format the collection as `[(key: value), (key: value), (key: value), ...]`
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToFullArrayString(string separator = ", ") =>
        CollectionFormat.ToFullArrayString(AsIterable().Map(kv => $"({kv.Key}: {kv.Value})"), separator);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Iterable<(K Key, V Value)> AsIterable() =>
        Value.AsIterable();

    /// <summary>
    /// Implicit conversion from an untyped empty list
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator HashMap<K, V>(SeqEmpty _) =>
        Empty;

    /// <summary>
    /// Equality of keys and values with `EqDefault<V>` used for values
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(HashMap<K, V> lhs, HashMap<K, V> rhs) =>
        lhs.Equals(rhs);

    /// <summary>
    /// In-equality of keys and values with `EqDefault<V>` used for values
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(HashMap<K, V> lhs, HashMap<K, V> rhs) =>
        !(lhs == rhs);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HashMap<K, V> operator +(HashMap<K, V> lhs, HashMap<K, V> rhs) =>
        lhs.Combine(rhs);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Combine(HashMap<K, V> rhs) =>
        Wrap(Value.Append(rhs.Value));

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HashMap<K, V> operator -(HashMap<K, V> lhs, HashMap<K, V> rhs) =>
        lhs.Subtract(rhs);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Subtract(HashMap<K, V> rhs) =>
        Wrap(Value.Subtract(rhs.Value));

    /// <summary>
    /// Returns True if 'other' is a proper subset of this set
    /// </summary>
    /// <returns>True if 'other' is a proper subset of this set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsProperSubsetOf(IEnumerable<(K Key, V Value)> other) =>
        Value.IsProperSubsetOf(other);

    /// <summary>
    /// Returns True if 'other' is a proper subset of this set
    /// </summary>
    /// <returns>True if 'other' is a proper subset of this set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsProperSubsetOf(IEnumerable<K> other) =>
        Value.IsProperSubsetOf(other);

    /// <summary>
    /// Returns True if 'other' is a proper superset of this set
    /// </summary>
    /// <returns>True if 'other' is a proper superset of this set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsProperSupersetOf(IEnumerable<(K Key, V Value)> other) =>
        Value.IsProperSupersetOf(other);

    /// <summary>
    /// Returns True if 'other' is a proper superset of this set
    /// </summary>
    /// <returns>True if 'other' is a proper superset of this set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsProperSupersetOf(IEnumerable<K> other) =>
        Value.IsProperSupersetOf(other);

    /// <summary>
    /// Returns True if 'other' is a superset of this set
    /// </summary>
    /// <returns>True if 'other' is a superset of this set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSubsetOf(IEnumerable<(K Key, V Value)> other) =>
        Value.IsSubsetOf(other);

    /// <summary>
    /// Returns True if 'other' is a superset of this set
    /// </summary>
    /// <returns>True if 'other' is a superset of this set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSubsetOf(IEnumerable<K> other) =>
        Value.IsSubsetOf(other);

    /// <summary>
    /// Returns True if 'other' is a superset of this set
    /// </summary>
    /// <returns>True if 'other' is a superset of this set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSubsetOf(HashMap<K, V> other) =>
        Value.IsSubsetOf(other.Value);

    /// <summary>
    /// Returns True if 'other' is a superset of this set
    /// </summary>
    /// <returns>True if 'other' is a superset of this set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSupersetOf(IEnumerable<(K Key, V Value)> other) =>
        Value.IsSupersetOf(other);

    /// <summary>
    /// Returns True if 'other' is a superset of this set
    /// </summary>
    /// <returns>True if 'other' is a superset of this set</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSupersetOf(IEnumerable<K> rhs) =>
        Value.IsSupersetOf(rhs);

    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Intersect(IEnumerable<K> rhs) =>
        Wrap(Value.Intersect(rhs));

    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Intersect(IEnumerable<(K Key, V Value)> rhs) =>
        Wrap(Value.Intersect(rhs));

    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Intersect(IEnumerable<(K Key, V Value)> rhs, WhenMatched<K, V, V, V> Merge) =>
        Wrap(Value.Intersect(rhs, Merge));

    /// <summary>
    /// Returns True if other overlaps this set
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Overlaps(IEnumerable<(K Key, V Value)> other) =>
        Value.Overlaps(other);

    /// <summary>
    /// Returns True if other overlaps this set
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Overlaps(IEnumerable<K> other) =>
        Value.Overlaps(other);

    /// <summary>
    /// Returns this - other.  Only the items in this that are not in 
    /// other will be returned.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Except(IEnumerable<K> rhs) =>
        Wrap(Value.Except(rhs));

    /// <summary>
    /// Returns this - other.  Only the items in this that are not in 
    /// other will be returned.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Except(IEnumerable<(K Key, V Value)> rhs) =>
        Wrap(Value.Except(rhs));

    /// <summary>
    /// Only items that are in one set or the other will be returned.
    /// If an item is in both, it is dropped.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> SymmetricExcept(HashMap<K, V> rhs) =>
        Wrap(Value.SymmetricExcept(rhs.Value));

    /// <summary>
    /// Only items that are in one set or the other will be returned.
    /// If an item is in both, it is dropped.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> SymmetricExcept(IEnumerable<(K Key, V Value)> rhs) =>
        Wrap(Value.SymmetricExcept(rhs));

    /// <summary>
    /// Finds the union of two sets and produces a new set with 
    /// the results
    /// </summary>
    /// <param name="other">Other set to union with</param>
    /// <returns>A set which contains all items from both sets</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Union(IEnumerable<(K, V)> rhs) =>
        TryAddRange(rhs);
        
    /// <summary>
    /// Union two maps.  
    /// </summary>
    /// <remarks>
    /// The `WhenMatched` merge function is called when keys are present in both map to allow resolving to a
    /// sensible value.
    /// </remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Union(IEnumerable<(K, V)> other, WhenMatched<K, V, V, V> Merge) =>
        Wrap(Value.Union(other, static (_, v) => v, static (_, v) => v, Merge));

    /// <summary>
    /// Union two maps.  
    /// </summary>
    /// <remarks>
    /// The `WhenMatched` merge function is called when keys are present in both map to allow resolving to a
    /// sensible value.
    /// </remarks>
    /// <remarks>
    /// The `WhenMissing` function is called when there is a key in the right-hand side, but not the left-hand-side.
    /// This allows the `V2` value-type to be mapped to the target `V` value-type. 
    /// </remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Union<W>(IEnumerable<(K, W)> other, WhenMissing<K, W, V> MapRight, WhenMatched<K, V, W, V> Merge) =>
        Wrap(Value.Union(other, static (_, v) => v, MapRight, Merge));

    /// <summary>
    /// Union two maps.  
    /// </summary>
    /// <remarks>
    /// The `WhenMatched` merge function is called when keys are present in both map to allow resolving to a
    /// sensible value.
    /// </remarks>
    /// <remarks>
    /// The `WhenMissing` function is called when there is a key in the left-hand side, but not the right-hand-side.
    /// This allows the `V` value-type to be mapped to the target `V2` value-type. 
    /// </remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, W> Union<W>(IEnumerable<(K, W)> other, WhenMissing<K, V, W> MapLeft, WhenMatched<K, V, W, W> Merge) =>
        Wrap(Value.Union(other, MapLeft, static (_, v) => v, Merge));

    /// <summary>
    /// Union two maps.  
    /// </summary>
    /// <remarks>
    /// The `WhenMatched` merge function is called when keys are present in both map to allow resolving to a
    /// sensible value.
    /// </remarks>
    /// <remarks>
    /// The `WhenMissing MapLeft` function is called when there is a key in the left-hand side, but not the
    /// right-hand-side.   This allows the `V` value-type to be mapped to the target `R` value-type. 
    /// </remarks>
    /// <remarks>
    /// The `WhenMissing MapRight` function is called when there is a key in the right-hand side, but not the
    /// left-hand-side.   This allows the `V2` value-type to be mapped to the target `R` value-type. 
    /// </remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, R> Union<W, R>(IEnumerable<(K, W)> other, 
                                     WhenMissing<K, V, R> MapLeft, 
                                     WhenMissing<K, W, R> MapRight, 
                                     WhenMatched<K, V, W, R> Merge) =>
        Wrap(Value.Union(other, MapLeft, MapRight, Merge));

    
    /// <summary>
    /// Equality of keys and values with `EqDefault<V>` used for values
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) =>
        obj is HashMap<K, V> hm && Equals(hm);

  
  /// <summary>
    /// Equality of keys and values with default `IEqualityComparer<V>` used for values
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(HashMap<K, V> other) =>
        Value.Equals(other.Value);

    /// <summary>
    /// Equality of keys and values with `IEqualityComparer<V>` used for values
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(HashMap<K, V> other, IEqualityComparer<V> equalityComparer) =>
        Value.Equals(other.Value, equalityComparer);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() =>
        Value.GetHashCode();

    /// <summary>
    /// Impure iteration of the bound values in the structure
    /// </summary>
    /// <returns>
    /// Returns the original unmodified structure
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Do(Action<V> f)
    {
        this.Iter(f);
        return this;
    }

    /// <summary>
    /// Atomically maps the map to a new map
    /// </summary>
    /// <returns>Mapped items in a new map</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, U> Select<U>(Func<V, U> mapper) =>
        Map(mapper);

    /// <summary>
    /// Atomically maps the map to a new map
    /// </summary>
    /// <returns>Mapped items in a new map</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, U> Select<U>(Func<K, V, U> mapper) =>
        Map(mapper);
    
    /// <summary>
    /// Atomically filter out items that return false when a predicate is applied
    /// </summary>
    /// <param name="pred">Predicate</param>
    /// <returns>New map with items filtered</returns>
    [Pure]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Where(Func<V, bool> pred) =>
        Filter(pred);

    /// <summary>
    /// Atomically filter out items that return false when a predicate is applied
    /// </summary>
    /// <param name="pred">Predicate</param>
    /// <returns>New map with items filtered</returns>
    [Pure]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashMap<K, V> Where(Func<K, V, bool> pred) =>
        Filter(pred);

    /// <summary>
    /// Return true if all items in the map return true when the predicate is applied
    /// </summary>
    /// <param name="pred">Predicate</param>
    /// <returns>True if all items in the map return true when the predicate is applied</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ForAll(Func<K, V, bool> pred)
    {
        foreach (var item in AsIterable())
        {
            if (!pred(item.Key, item.Value)) return false;
        }
        return true;
    }

    /// <summary>
    /// Return true if all items in the map return true when the predicate is applied
    /// </summary>
    /// <param name="pred">Predicate</param>
    /// <returns>True if all items in the map return true when the predicate is applied</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ForAll(Func<(K Key, V Value), bool> pred) =>
        AsIterable().Map(kv => (kv.Key, kv.Value)).ForAll(pred);

    /// <summary>
    /// Return true if *any* items in the map return true when the predicate is applied
    /// </summary>
    /// <param name="pred">Predicate</param>
    /// <returns>True if all items in the map return true when the predicate is applied</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Exists(Func<K, V, bool> pred)
    {
        foreach (var item in AsIterable())
        {
            if (pred(item.Key, item.Value)) return true;
        }
        return false;
    }

    /// <summary>
    /// Return true if *any* items in the map return true when the predicate is applied
    /// </summary>
    /// <param name="pred">Predicate</param>
    /// <returns>True if all items in the map return true when the predicate is applied</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Exists(Func<(K Key, V Value), bool> pred) =>
        AsIterable().Map(kv => (kv.Key, kv.Value)).Exists(pred);

    /// <summary>
    /// Atomically iterate through all key/value pairs in the map (in order) and execute an
    /// action on each
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <returns>Unit</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Unit Iter(Action<K, V> action)
    {
        foreach (var item in this)
        {
            action(item.Key, item.Value);
        }
        return unit;
    }

    /// <summary>
    /// Atomically iterate through all key/value pairs (as tuples) in the map (in order) 
    /// and execute an action on each
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <returns>Unit</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Unit Iter(Action<(K Key, V Value)> action)
    {
        foreach (var item in this)
        {
            action(item);
        }
        return unit;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() =>
        AsIterable().Map(p => new KeyValuePair<K, V>(p.Key, p.Value)).GetEnumerator();
    
    [Pure]
    IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => Keys;
    
    [Pure]
    IEnumerable<V> IReadOnlyDictionary<K, V>.Values => Values;

    [Pure]
    public bool TryGetValue(K key, out V value)
    {
        var v = Find(key);
        if (v.IsSome)
        {
            value = (V)v;
            return true;
        }
        else
        {
            value = default!;
            return false;
        }
    }

    [Pure]
    public bool HasSameEqualityComparer(IEqualityComparer<K> equalityComparer) => 
        Value.HasSameEqualityComparer(equalityComparer);
    
    /// <summary>
    /// Get a IReadOnlyDictionary for this map.  No mapping is required, so this is very fast.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IReadOnlyDictionary<K, V> ToReadOnlyDictionary() =>
        this;

    public static HashMap<K, V> AdditiveIdentity => 
        Empty;    
}
