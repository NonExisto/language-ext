using LanguageExt.Traits;
using static LanguageExt.Prelude;
using System;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Array = System.Array;

namespace LanguageExt;

internal enum UpdateType : byte
{
    Add,
    TryAdd,
    AddOrUpdate,
    SetItem,
    TrySetItem
}

internal enum TrieTag : byte
{
    Entries,
    Collision,
    Empty
}

internal readonly record struct UpdateContext(UpdateType Type, bool Mutate);

/// <summary>
/// Implementation of the CHAMP trie hash map data structure (Compressed Hash Array Map Trie)
/// [efficient-immutable-collections.pdf](https://michael.steindorfer.name/publications/phd-thesis-efficient-immutable-collections.pdf)
/// </summary>
/// <remarks>
/// Used by internally by `LanguageExt.HashMap`
/// </remarks>
internal sealed class TrieMap<K, V> :
    IReadOnlyCollection<(K Key, V Value)>,
    IEquatable<TrieMap<K, V>>
{
    public static TrieMap<K, V> Empty(IEqualityComparer<K>? equalityComparer = null) => 
        new (equalityComparer ?? getRegisteredEqualityComparerOrDefault<K>(), EmptyNode.Default, 0);

    readonly Node Root;
    readonly int _count;
    private readonly IEqualityComparer<K> _equalityComparer;
    int hash;

    /// <summary>
    /// Ctor
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    TrieMap(IEqualityComparer<K> equalityComparer, Node root, int count)
    {
        Root = root;
        _count = count;
        _equalityComparer = equalityComparer;
    }

    public TrieMap(IEnumerable<(K Key, V Value)> items, bool tryAdd = true, IEqualityComparer<K>? equalityComparer = null)
    {
        Root = EmptyNode.Default;
        _equalityComparer = equalityComparer ?? getRegisteredEqualityComparerOrDefault<K>();
        var update = new UpdateContext(tryAdd ? UpdateType.TryAdd : UpdateType.AddOrUpdate, true);
        foreach (var item in items)
        {
            var h       = (uint)_equalityComparer.GetHashCode(item.Key.require("Item key is null"));
            Sec section = default;
            var changed = Root.Update(update, item, h, section, _equalityComparer);
            if (changed)
            {
                _count += changed.Value.CountDelta;
                Root = changed.Value.Node;
            }
            
        }
    }

    public TrieMap(ReadOnlySpan<(K Key, V Value)> items, bool tryAdd = true, IEqualityComparer<K>? equalityComparer = null)
    {
        Root = EmptyNode.Default;
        _equalityComparer = equalityComparer ?? getRegisteredEqualityComparerOrDefault<K>();
        var update = new UpdateContext(tryAdd ? UpdateType.TryAdd : UpdateType.AddOrUpdate, true);
        foreach (var item in items)
        {
            var h       = (uint)_equalityComparer.GetHashCode(item.Key.require("Item key is null"));
            Sec section = default;
            var changed = Root.Update(update, item, h, section, _equalityComparer);
            if (changed)
            {
                _count += changed.Value.CountDelta;
                Root = changed.Value.Node;
            }
        }
    }

    /// <summary>
    /// True if no items in the map
    /// </summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count == 0;
    }

    /// <summary>
    /// Number of items in the map
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    /// <summary>
    /// Add an item to the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> Add(K key, V value) =>
        Update(key, value, new(UpdateType.Add, false));

    /// <summary>
    /// Add an item to the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, Change<V> Change) AddWithLog(K key, V value, IEqualityComparer<V> equalityComparer) =>
        UpdateWithLog(key, value, new(UpdateType.Add, false), equalityComparer);

    /// <summary>
    /// Try to add an item to the map.  If it already exists, do
    /// nothing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> TryAdd(K key, V value) =>
        Update(key, value, new(UpdateType.TryAdd, false));

    /// <summary>
    /// Try to add an item to the map.  If it already exists, do
    /// nothing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, Change<V> Change) TryAddWithLog(K key, V value, IEqualityComparer<V> equalityComparer) =>
        UpdateWithLog(key, value, new(UpdateType.TryAdd, false), equalityComparer);

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> AddOrUpdate(K key, V value) =>
        Update(key, value, new(UpdateType.AddOrUpdate, false));

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TrieMap<K, V> AddOrUpdateInPlace(K key, V value) =>
        Update(key, value, new(UpdateType.AddOrUpdate, true));

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, Change<V> Change) AddOrUpdateWithLog(K key, V value, IEqualityComparer<V> equalityComparer) =>
        UpdateWithLog(key, value, new(UpdateType.AddOrUpdate, false), equalityComparer);

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> AddOrUpdate(K key, Func<V, V> Some, Func<V> None) => 
        FindInternal(key).Match(v => AddOrUpdate(key, Some(v.Value)), () => AddOrUpdate(key, None()));

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, Change<V> Change) AddOrUpdateWithLog(K key, Func<V, V> Some, Func<V> None, IEqualityComparer<V> equalityComparer) => 
        FindInternal(key).Match(v => AddOrUpdateWithLog(key, Some(v.Value), equalityComparer),
            () => AddOrUpdateWithLog(key, None(), equalityComparer));

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> AddOrMaybeUpdate(K key, Func<V, V> Some, Func<Option<V>> None) => 
        FindInternal(key).Match(v => AddOrUpdate(key, Some(v.Value)), () => None().Match(x => AddOrUpdate(key, x), this));

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> AddOrUpdate(K key, Func<V, V> Some, V None) => 
        FindInternal(key).Match(v => AddOrUpdate(key, Some(v.Value)), AddOrUpdate(key, None));

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, Change<V> Change) AddOrUpdateWithLog(K key, Func<V, V> Some, V None, IEqualityComparer<V> equalityComparer) =>
        FindInternal(key).Match(v => AddOrUpdateWithLog(key, Some(v.Value), equalityComparer),
            () => AddOrUpdateWithLog(key, None, equalityComparer));

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> AddRange(IEnumerable<(K Key, V Value)> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.Add, false);
        foreach (var (Key, Value) in items)
        {
            self = self.Update(Key, Value, env);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) AddRangeWithLog(IEnumerable<(K Key, V Value)> items, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var env = new UpdateContext(UpdateType.Add, false);
        var changeEnv = new UpdateContext(UpdateType.AddOrUpdate, true);
        foreach (var (Key, Value) in items)
        {
            var pair = self.UpdateWithLog(Key, Value, env, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.Update(Key, pair.Change, changeEnv);
            }
        }
        return (self, changes);
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> AddRange(IEnumerable<Tuple<K, V>> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.Add, false);
        foreach (var item in items)
        {
            self = self.Update(item.Item1, item.Item2, env);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) AddRangeWithLog(IEnumerable<Tuple<K, V>> items, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var env = new UpdateContext(UpdateType.Add, false);
        var changeEnv = new UpdateContext(UpdateType.AddOrUpdate, true);
        foreach (var item in items)
        {
            var pair = self.UpdateWithLog(item.Item1, item.Item2, env, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.Update(item.Item1, pair.Change, changeEnv);
            }
        }
        return (self, changes);
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> AddRange(IEnumerable<KeyValuePair<K, V>> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.Add, false);
        foreach (var item in items)
        {
            self = self.Update(item.Key, item.Value, env);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) AddRangeWithLog(IEnumerable<KeyValuePair<K, V>> items, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var env = new UpdateContext(UpdateType.Add, false);
        var changeEnv = new UpdateContext(UpdateType.AddOrUpdate, true);
        foreach (var item in items)
        {
            var pair = self.UpdateWithLog(item.Key, item.Value, env, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.Update(item.Key, pair.Change, changeEnv);
            }
        }
        return (self, changes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> TryAddRange(IEnumerable<(K Key, V Value)> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.TryAdd, false);
        foreach (var (Key, Value) in items)
        {
            self = self.Update(Key, Value, env);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) TryAddRangeWithLog(IEnumerable<(K Key, V Value)> items, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var env = new UpdateContext(UpdateType.TryAdd, false);
        var changeEnv = new UpdateContext(UpdateType.AddOrUpdate, true);
        foreach (var (Key, Value) in items)
        {
            var pair = self.UpdateWithLog(Key, Value, env, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.Update(Key, pair.Change, changeEnv);
            }
        }
        return (self, changes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> TryAddRange(IEnumerable<Tuple<K, V>> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.TryAdd, false);
        foreach (var item in items)
        {
            self = self.Update(item.Item1, item.Item2, env);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) TryAddRangeWithLog(IEnumerable<Tuple<K, V>> items, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var env = new UpdateContext(UpdateType.TryAdd, false);
        var changeEnv = new UpdateContext(UpdateType.AddOrUpdate, true);
        foreach (var item in items)
        {
            var pair = self.UpdateWithLog(item.Item1, item.Item2, env, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.Update(item.Item1, pair.Change, changeEnv);
            }
        }
        return (self, changes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> TryAddRange(IEnumerable<KeyValuePair<K, V>> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.TryAdd, false);
        foreach (var item in items)
        {
            self = self.Update(item.Key, item.Value, env);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) TryAddRangeWithLog(IEnumerable<KeyValuePair<K, V>> items, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var env = new UpdateContext(UpdateType.TryAdd, false);
        var changeEnv = new UpdateContext(UpdateType.AddOrUpdate, true);
        foreach (var item in items)
        {
            var pair = self.UpdateWithLog(item.Key, item.Value, env, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.Update(item.Key, pair.Change, changeEnv);
            }
        }
        return (self, changes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> AddOrUpdateRange(IEnumerable<(K Key, V Value)> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.AddOrUpdate, false); ;
        foreach (var (Key, Value) in items)
        {
            self = self.Update(Key, Value, env);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) AddOrUpdateRangeWithLog(IEnumerable<(K Key, V Value)> items, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var env = new UpdateContext(UpdateType.AddOrUpdate, false);
        var changeEnv = new UpdateContext(UpdateType.AddOrUpdate, true);
        foreach (var (Key, Value) in items)
        {
            var pair = self.UpdateWithLog(Key, Value, env, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.Update(Key, pair.Change, changeEnv);
            }
        }
        return (self, changes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> AddOrUpdateRange(IEnumerable<Tuple<K, V>> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.AddOrUpdate, false);
        foreach (var item in items)
        {
            self = self.Update(item.Item1, item.Item2, env);
        }
        return self;
    }
       
    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) AddOrUpdateRangeWithLog(IEnumerable<Tuple<K, V>> items, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var env = new UpdateContext(UpdateType.AddOrUpdate, false);
        var changeEnv = new UpdateContext(UpdateType.AddOrUpdate, true);
        foreach (var item in items)
        {
            var pair = self.UpdateWithLog(item.Item1, item.Item2, env, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.Update(item.Item1, pair.Change, changeEnv);
            }
        }
        return (self, changes);
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> AddOrUpdateRange(IEnumerable<KeyValuePair<K, V>> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.AddOrUpdate, false);
        foreach (var item in items)
        {
            self = self.Update(item.Key, item.Value, env);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) AddOrUpdateRangeWithLog(IEnumerable<KeyValuePair<K, V>> items, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var env = new UpdateContext(UpdateType.AddOrUpdate, false);
        var changeEnv = new UpdateContext(UpdateType.AddOrUpdate, true);
        foreach (var item in items)
        {
            var pair = self.UpdateWithLog(item.Key, item.Value, env, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.Update(item.Key, pair.Change, changeEnv);
            }
        }
        return (self, changes);
    }        

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> SetItems(IEnumerable<(K Key, V Value)> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.SetItem, false);
        foreach (var (Key, Value) in items)
        {
            self = self.Update(Key, Value, env);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) SetItemsWithLog(IEnumerable<(K Key, V Value)> items, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var env = new UpdateContext(UpdateType.SetItem, false);
        var changeEnv = new UpdateContext(UpdateType.AddOrUpdate, true);
        foreach (var (Key, Value) in items)
        {
            var pair = self.UpdateWithLog(Key, Value, env, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.Update(Key, pair.Change, changeEnv);
            }
        }
        return (self, changes);
    }        

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> SetItems(IEnumerable<KeyValuePair<K, V>> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.SetItem, false);
        foreach (var item in items)
        {
            self = self.Update(item.Key, item.Value, env);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) SetItemsWithLog(IEnumerable<KeyValuePair<K, V>> items, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var env = new UpdateContext(UpdateType.SetItem, false);
        var changeEnv = new UpdateContext(UpdateType.AddOrUpdate, true);
        foreach (var item in items)
        {
            var pair = self.UpdateWithLog(item.Key, item.Value, env, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.Update(item.Key, pair.Change, changeEnv);
            }
        }
        return (self, changes);
    }        

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> SetItems(IEnumerable<Tuple<K, V>> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.SetItem, false);
        foreach (var item in items)
        {
            self = self.Update(item.Item1, item.Item2, env);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) SetItemsWithLog(IEnumerable<Tuple<K, V>> items, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var env = new UpdateContext(UpdateType.SetItem, false);
        var changeEnv = new UpdateContext(UpdateType.AddOrUpdate, true);
        foreach (var item in items)
        {
            var pair = self.UpdateWithLog(item.Item1, item.Item2, env, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.Update(item.Item1, pair.Change, changeEnv);
            }
        }
        return (self, changes);
    }        

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> TrySetItems(IEnumerable<(K Key, V Value)> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.TrySetItem, false);
        foreach (var (Key, Value) in items)
        {
            self = self.Update(Key, Value, env);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) TrySetItemsWithLog(IEnumerable<(K Key, V Value)> items, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var env = new UpdateContext(UpdateType.TrySetItem, false);
        var changeEnv = new UpdateContext(UpdateType.AddOrUpdate, true);
        foreach (var (Key, Value) in items)
        {
            var pair = self.UpdateWithLog(Key, Value, env, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.Update(Key, pair.Change, changeEnv);
            }
        }
        return (self, changes);
    }        

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> TrySetItems(IEnumerable<KeyValuePair<K, V>> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.TrySetItem, false);
        foreach (var item in items)
        {
            self = self.Update(item.Key, item.Value, env);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) TrySetItemsWithLog(IEnumerable<KeyValuePair<K, V>> items, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var env = new UpdateContext(UpdateType.TrySetItem, false);
        var changeEnv = new UpdateContext(UpdateType.AddOrUpdate, true);
        foreach (var item in items)
        {
            var pair = self.UpdateWithLog(item.Key, item.Value, env, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.Update(item.Key, pair.Change, changeEnv);
            }
        }
        return (self, changes);
    }        

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> TrySetItems(IEnumerable<Tuple<K, V>> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.TrySetItem, false);
        foreach (var item in items)
        {
            self = self.Update(item.Item1, item.Item2, env);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) TrySetItemsWithLog(IEnumerable<Tuple<K, V>> items, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var env = new UpdateContext(UpdateType.TrySetItem, false);
        var changeEnv = new UpdateContext(UpdateType.AddOrUpdate, true);
        foreach (var item in items)
        {
            var pair = self.UpdateWithLog(item.Item1, item.Item2, env, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.Update(item.Item1, pair.Change, changeEnv);
            }
        }
        return (self, changes);
    }  
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> TrySetItems(IEnumerable<K> items, Func<V, V> Some)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.TrySetItem(item, Some);
        }
        return self;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) TrySetItemsWithLog(IEnumerable<K> items, Func<V, V> Some, IEqualityComparer<V> equalityComparer)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        foreach (var item in items)
        {
            var pair = self.TrySetItemWithLog(item, Some, equalityComparer);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(item, pair.Change);
            }
        }
        return (self, changes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> RemoveRange(IEnumerable<K> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.Remove(item);
        }
        return self;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V>, TrieMap<K, Change<V>> Changes) RemoveRangeWithLog(IEnumerable<K> items)
    {
        var self    = this;
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        foreach (var item in items)
        {
            var pair = self.RemoveWithLog(item);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(item, pair.Change);
            }
        }
        return (self, changes);
    }

    /// <summary>
    /// Set an item that already exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> SetItem(K key, V value) =>
        Update(key, value, new(UpdateType.SetItem, false));

    /// <summary>
    /// Set an item that already exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, Change<V> Change) SetItemWithLog(K key, V value, IEqualityComparer<V> equalityComparer) =>
        UpdateWithLog(key, value, new(UpdateType.SetItem, false), equalityComparer);
        
    /// <summary>
    /// Set an item that already exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> SetItem(K key, Func<V, V> Some)
    {
        var value = Find(key).Map(Some).IfNone(() => throw new ArgumentException($"Key doesn't exist in map: {key}"));
        return SetItem(key, value);
    }
        
    /// <summary>
    /// Set an item that already exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, Change<V> Change) SetItemWithLog(K key, Func<V, V> Some, IEqualityComparer<V> equalityComparer)
    {
        var value = Find(key).Map(Some).IfNone(() => throw new ArgumentException($"Key doesn't exist in map: {key}"));
        return SetItemWithLog(key, value, equalityComparer);
    }

    /// <summary>
    /// Try to set an item that already exists in the map.  If none
    /// exists, do nothing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> TrySetItem(K key, V value) =>
        Update(key, value, new(UpdateType.TrySetItem, false));

    /// <summary>
    /// Try to set an item that already exists in the map.  If none
    /// exists, do nothing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, Change<V> Change) TrySetItemWithLog(K key, V value, IEqualityComparer<V> equalityComparer) =>
        UpdateWithLog(key, value, new(UpdateType.TrySetItem, false), equalityComparer);

    /// <summary>
    /// Set an item that already exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> TrySetItem(K key, Func<V, V> Some) =>
        Find(key)
           .Map(Some)
           .Match(Some: v => SetItem(key, v),
                  None: () => this);

    /// <summary>
    /// Set an item that already exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, Change<V> Change) TrySetItemWithLog(K key, Func<V, V> Some, IEqualityComparer<V> equalityComparer) =>
        Find(key)
           .Map(Some)
           .Match(Some: v => SetItemWithLog(key, v, equalityComparer),
                  None: () => (this, Change<V>.None));

    /// <summary>
    /// Remove an item from the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> Remove(K key)
    {
        var h       = (uint)_equalityComparer.GetHashCodeOrDefault(key);
        Sec section = default;
        var removed = Root.Remove(key, h, section, _equalityComparer);
        return removed
                   ? new TrieMap<K, V>(_equalityComparer, removed.Value.Node, _count + removed.Value.CountDelta)
                   : this;
    }

    /// <summary>
    /// Remove an item from the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, Change<V> Change) RemoveWithLog(K key)
    {
        var h       = (uint)_equalityComparer.GetHashCodeOrDefault(key);
        Sec section = default;
        var removed = Root.Remove(key, h, section, _equalityComparer);
        
        return removed
                   ? (new TrieMap<K, V>(_equalityComparer, removed.Value.Node, _count + removed.Value.CountDelta), 
                      removed.Value.CountDelta == 0
                          ? Change<V>.None
                          : Change<V>.Removed(removed.Value.Old))
                   :(this, Change<V>.None);
    }

    /// <summary>
    /// Indexer
    /// </summary>
    public V this[K key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => FindInternal(key).Map(v => v.Value).IfNone(() => throw new ArgumentException($"Key doesn't exist in map: {key}"));
    }

    /// <summary>
    /// Get a key value pair from a key
    /// </summary>
    public Option<(K Key, V Value)> GetOption(K key) => 
        FindInternal(key);

    /// <summary>
    /// Create an empty map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>>) ClearWithLog() =>
        (Empty(_equalityComparer), Map(Change<V>.Removed));

    /// <summary>
    /// Get the hash code of the items in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() =>
        hash == 0
            ? (hash = FNV32.Hash(_equalityComparer, AsIterable()))
            : hash;

    /// <summary>
    /// Returns the whether the `key` exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(K key) =>
        FindInternal(key).IsSome;

    /// <summary>
    /// Returns the whether the `value` exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(V value) =>
        Contains(value, getRegisteredEqualityComparerOrDefault<V>());

    /// <summary>
    /// Returns the whether the `value` exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(V value, IEqualityComparer<V> equalityComparer)  =>
        Values.Exists(v => equalityComparer.Equals(v, value));

    /// <summary>
    /// Returns the whether the `key` exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(K key, V value) =>
        Contains(key, value, getRegisteredEqualityComparerOrDefault<V>());

    /// <summary>
    /// Returns the whether the `key` exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(K key, V Value, IEqualityComparer<V> equalityComparer) =>
        Find(key).Map(v => equalityComparer.Equals(v, Value)).IfNone(false);

    /// <summary>
    /// Returns the value associated with `key`.  Or None, if no key exists
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<V> Find(K key) => 
        FindInternal(key).Map(v => v.Value);

    /// <summary>
    /// Returns the value associated with `key`.  Or None, if no key exists
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Option<(K Key, V Value)> FindInternal(K key)
    {
        var h       = (uint)_equalityComparer.GetHashCodeOrDefault(key);
        Sec section = default;
        return Root.Read(key, h, section, _equalityComparer);
    }

    /// <summary>
    /// Returns the value associated with `key` then match the result
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public R Find<R>(K key, Func<V, R> Some, Func<R> None) => 
        FindInternal(key).Match(v => Some(v.Value), None);

    /// <summary>
    /// Tries to find the value, if not adds it and returns the update map and/or value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, V Value) FindOrAdd(K key, Func<V> None) =>
        Find(key, Some: v => (this, v), None: () =>
                                              {
                                                  var v = None();
                                                  return (Add(key, v), v);
                                              });

    /// <summary>
    /// Tries to find the value, if not adds it and returns the update map and/or value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, V Value, Change<V> Change) FindOrAddWithLog(K key, Func<V> None, IEqualityComparer<V> equalityComparer)
    {
        var item = Find(key);
        if (item.IsSome)
        {
            return (this, item.Value, Change<V>.None);
        }
        else
        {
            var v    = None();
            var self = AddWithLog(key, v, equalityComparer);
            return (self.Map, v, self.Change);
        }
    }

    /// <summary>
    /// Tries to find the value, if not adds it and returns the update map and/or value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, V Value) FindOrAdd(K key, V value) =>
        Find(key, Some: v => (this, v), None: () => (Add(key, value), value));

    /// <summary>
    /// Tries to find the value, if not adds it and returns the update map and/or value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, V Value, Change<V> Change) FindOrAddWithLog(K key, V value, IEqualityComparer<V> equalityComparer)
    {
        var item = Find(key);
        if (item.IsSome)
        {
            return (this, item.Value, Change<V>.None);
        }
        else
        {
            var self = AddWithLog(key, value, equalityComparer);
            return (self.Map, value, self.Change);
        }
    }   
        
    /// <summary>
    /// Tries to find the value, if not adds it and returns the update map and/or value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, Option<V> Value) FindOrMaybeAdd(K key, Func<Option<V>> None) =>
        Find(key, Some: v => (this, v), None: () =>
                                              {
                                                  var v = None();
                                                  return v.IsSome
                                                             ? (Add(key, (V)v), v)
                                                             : (this, v);
                                              });

    /// <summary>
    /// Tries to find the value, if not adds it and returns the update map and/or value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, V? Value, Change<V> Change) FindOrMaybeAddWithLog(K key, Func<Option<V>> None, IEqualityComparer<V> equalityComparer)
    {
        var item = Find(key);
        if (item.IsSome)
        {
            return (this, item.Value, Change<V>.None);
        }
        else
        {
            var v = None();
            if (v.IsSome)
            {
                var self = AddWithLog(key, v.Value, equalityComparer);
                return (self.Map, v.Value, self.Change);
            }
            else
            {
                return (this, item.Value, Change<V>.None);
            }
        }
    }        

    /// <summary>
    /// Tries to find the value, if not adds it and returns the update map and/or value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, Option<V> Value) FindOrMaybeAdd(K key, Option<V> value) =>
        Find(key, Some: v => (this, v), None: () =>
                                                  value.IsSome
                                                      ? (Add(key, (V)value), value)
                                                      : (this, value));

    /// <summary>
    /// Tries to find the value, if not adds it and returns the update map and/or value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, Option<V> Value, Change<V> Change) FindOrMaybeAddWithLog(K key, Option<V> value, IEqualityComparer<V> equalityComparer)
    {
        var item = Find(key);
        if (item.IsSome)
        {
            return (this, item.Value, Change<V>.None);
        }
        else
        {
            if (value.IsSome)
            {
                var self = AddWithLog(key, value.Value, equalityComparer);
                return (self.Map, value.Value, self.Change);
            }
            else
            {
                return (this, item.Value, Change<V>.None);
            }
        }
    }  
        
    /// <summary>
    /// Returns the value associated with `key`.  Or None, if no key exists
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Seq<V> FindAll(K key) =>
        Find(key).ToSeq();

    /// <summary>
    /// Map from V to U
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, U> Map<U>(Func<V, U> f) =>
        new (AsIterable().Select(kv => (kv.Key, f(kv.Value))), false);

    /// <summary>
    /// Map from V to U
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, U> Map, TrieMap<K, Change<U>> Changes) MapWithLog<U>(Func<V, U> f)
    {
        var target  = TrieMap<K, U>.Empty(_equalityComparer);
        var changes = TrieMap<K, Change<U>>.Empty(_equalityComparer);
            
        foreach (var (Key, Value) in this)
        {
            var value = f(Value);
            target = target.AddOrUpdateInPlace(Key, value);
            changes = changes.AddOrUpdateInPlace(Key, new EntryMapped<V, U>(Value, value));
        }
        return (target, changes);
    }

    /// <summary>
    /// Map from V to U
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, U> Map<U>(Func<K, V, U> f) =>
        new (AsIterable().Select(kv => (kv.Key, f(kv.Key, kv.Value))), false);

    /// <summary>
    /// Filter
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> Filter(Func<V, bool> f) =>
        new (AsIterable().Filter(kv => f(kv.Value)), false);

    /// <summary>
    /// Map from V to U
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) FilterWithLog(Func<V, bool> f)
    {
        var target  = Empty(_equalityComparer);
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
            
        foreach (var (Key, Value) in this)
        {
            var pred = f(Value);
            if (pred)
            {
                target = target.AddOrUpdateInPlace(Key, Value);
            }
            else
            {
                changes = changes.AddOrUpdateInPlace(Key, Change<V>.Removed(Value));
            }
        }
        return (target, changes);
    }
        
    /// <summary>
    /// Filter
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> Filter(Func<K, V, bool> f) =>
        new (AsIterable().Filter(kv => f(kv.Key, kv.Value)), false);

    /// <summary>
    /// Map from V to U
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) FilterWithLog(Func<K, V, bool> f)
    {
        var target  = Empty(_equalityComparer);
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
            
        foreach (var (Key, Value) in this)
        {
            var pred = f(Key, Value);
            if (pred)
            {
                target = target.AddOrUpdateInPlace(Key, Value);
            }
            else
            {
                changes = changes.AddOrUpdateInPlace(Key, Change<V>.Removed(Value));
            }
        }
        return (target, changes);
    }

    /// <summary>
    /// Associative union
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> Append(TrieMap<K, V> rhs) =>
        TryAddRange(rhs.AsIterable());

    /// <summary>
    /// Associative union
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) AppendWithLog(TrieMap<K, V> rhs, IEqualityComparer<V> equalityComparer) =>
        TryAddRangeWithLog(rhs.AsIterable(), equalityComparer);

    /// <summary>
    /// Subtract
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> Subtract(TrieMap<K, V> rhs)
    {
        var lhs = this;
        foreach (var item in rhs.Keys)
        {
            lhs = lhs.Remove(item);
        }
        return lhs;
    }

    /// <summary>
    /// Subtract
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) SubtractWithLog(TrieMap<K, V> rhs)
    {
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var lhs     = this;
        foreach (var item in rhs.Keys)
        {
            var pair = lhs.RemoveWithLog(item);
            lhs = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(item, pair.Change);
            }
        }
        return (lhs, changes);
    }

    /// <summary>
    /// Equality
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(TrieMap<K, V> lhs, TrieMap<K, V> rhs) =>
        lhs.Equals(rhs);

    /// <summary>
    /// Non equality
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(TrieMap<K, V> lhs, TrieMap<K, V> rhs) =>
        !(lhs == rhs);

    /// <summary>
    /// Equality
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? rhs) =>
        rhs is TrieMap<K, V> map && Equals(map);

    /// <summary>
    /// Equality
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(TrieMap<K, V>? rhs) =>
        rhs is not null && Equals(rhs, getRegisteredEqualityComparerOrDefault<V>());

    /// <summary>
    /// Equality
    /// </summary>
    public bool Equals(TrieMap<K, V>? rhs, IEqualityComparer<V> equalityComparer)
    {
        if (rhs is null) return false;
        if (ReferenceEquals(this, rhs)) return true;
        if (Count != rhs.Count) return false;

        using var iterA = GetEnumerator();
        using var iterB = rhs.GetEnumerator();
        while (iterA.MoveNext() && iterB.MoveNext())
        {
            if (!_equalityComparer.Equals(iterA.Current.Key, iterB.Current.Key)) return false;
            if (!equalityComparer.Equals(iterA.Current.Value, iterB.Current.Value)) return false;
        }
        
        return true;
    }
        
    /// <summary>
    /// Update an item in the map - can mutate if needed
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    TrieMap<K, V> Update(K key, V value, UpdateContext env)
    {
        var h       = (uint)_equalityComparer.GetHashCodeOrDefault(key);
        Sec section = default;
        var changed = Root.Update(env, (key, value), h, section, _equalityComparer);
        return changed
                   ? new TrieMap<K, V>(_equalityComparer, changed.Value.Node, _count + changed.Value.CountDelta)
                   : this;
    }

    /// <summary>
    /// Update an item in the map - can mutate if needed
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    (TrieMap<K, V> Map, Change<V> Change) UpdateWithLog(K key, V value, UpdateContext env, IEqualityComparer<V> equalityComparer)
    {
        var h       = (uint)_equalityComparer.GetHashCodeOrDefault(key);
        Sec section = default;
        var changed = Root.Update(env, (key, value), h, section, _equalityComparer);
        return changed
                   ? (new TrieMap<K, V>(_equalityComparer, changed.Value.Node, _count + changed.Value.CountDelta), 
                      changed.Value.CountDelta == 0 
                          ? equalityComparer.Equals(changed.Value.Old, value)
                                ? Change<V>.None 
                                : Change<V>.Mapped(changed.Value.Old, value)
                          : Change<V>.Added(value))
                   : (this, Change<V>.None);
    }

    /// <summary>
    /// Returns True if 'other' is a proper subset of this set
    /// </summary>
    /// <returns>True if 'other' is a proper subset of this set</returns>
    public bool IsProperSubsetOf(IEnumerable<(K Key, V Value)> other) =>
        IsProperSubsetOf(other.Select(x => x.Key));

    /// <summary>
    /// Returns True if 'other' is a proper subset of this set
    /// </summary>
    /// <returns>True if 'other' is a proper subset of this set</returns>
    public bool IsProperSubsetOf(IEnumerable<K> other)
    {
        if (IsEmpty)
        {
            return other.Any();
        }

        var matches    = 0;
        var extraFound = false;
        foreach (var item in other)
        {
            if (ContainsKey(item))
            {
                matches++;
            }
            else
            {
                extraFound = true;
            }

            if (matches == Count && extraFound)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns True if 'other' is a proper superset of this set
    /// </summary>
    /// <returns>True if 'other' is a proper superset of this set</returns>
    public bool IsProperSupersetOf(IEnumerable<(K Key, V Value)> other) =>
        IsProperSupersetOf(other.Select(x => x.Key));

    /// <summary>
    /// Returns True if 'other' is a proper superset of this set
    /// </summary>
    /// <returns>True if 'other' is a proper superset of this set</returns>
    public bool IsProperSupersetOf(IEnumerable<K> other)
    {
        if (IsEmpty)
        {
            return false;
        }

        var matchCount = 0;
        foreach (var item in other)
        {
            matchCount++;
            if (!ContainsKey(item))
            {
                return false;
            }
        }

        return Count > matchCount;
    }

    /// <summary>
    /// Returns True if 'other' is a superset of this set
    /// </summary>
    /// <returns>True if 'other' is a superset of this set</returns>
    public bool IsSubsetOf(IEnumerable<(K Key, V Value)> other)
    {
        if (IsEmpty)
        {
            return true;
        }

        var matches = 0;
        foreach (var (Key, Value) in other)
        {
            if (ContainsKey(Key))
            {
                matches++;
            }
        }
        return matches == Count;
    }

    /// <summary>
    /// Returns True if 'other' is a superset of this set
    /// </summary>
    /// <returns>True if 'other' is a superset of this set</returns>
    public bool IsSubsetOf(IEnumerable<K> other)
    {
        if (IsEmpty)
        {
            return true;
        }

        var matches = 0;
        foreach (var item in other)
        {
            if (ContainsKey(item))
            {
                matches++;
            }
        }
        return matches == Count;
    }

    /// <summary>
    /// Returns True if 'other' is a superset of this set
    /// </summary>
    /// <returns>True if 'other' is a superset of this set</returns>
    public bool IsSubsetOf(TrieMap<K, V> other)
    {
        if (IsEmpty)
        {
            // All empty sets are subsets
            return true;
        }
        if(Count > other.Count)
        {
            // A subset must be smaller or equal in size
            return false;
        }

        foreach(var item in this)
        {
            if(!other.Contains(item))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns True if 'other' is a superset of this set
    /// </summary>
    /// <returns>True if 'other' is a superset of this set</returns>
    public bool IsSupersetOf(IEnumerable<(K Key, V Value)> other) =>
        IsSupersetOf(other.Select(x => x.Key));

    /// <summary>
    /// Returns True if 'other' is a superset of this set
    /// </summary>
    /// <returns>True if 'other' is a superset of this set</returns>
    public bool IsSupersetOf(IEnumerable<K> other)
    {
        foreach (var item in other)
        {
            if (!ContainsKey(item))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns True if other overlaps this set
    /// </summary>
    public bool Overlaps(IEnumerable<(K Key, V Value)> other) =>
        Overlaps(other.Select(x => x.Key));

    /// <summary>
    /// Returns True if other overlaps this set
    /// </summary>
    public bool Overlaps(IEnumerable<K> other)
    {
        if (IsEmpty)
        {
            return false;
        }

        foreach (var item in other)
        {
            if (ContainsKey(item))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    public TrieMap<K, V> Intersect(IEnumerable<K> other)
    {
        var res = new List<(K, V)>();
        foreach (var item in other)
        {
            GetOption(item).Do(res.Add);
        }
        return new TrieMap<K, V>(res);
    }

    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) IntersectWithLog(IEnumerable<K> other)
    {
        var set     = new TrieSet<K>(other, equalityComparer: _equalityComparer);
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var res     = Empty(_equalityComparer);
            
        foreach (var (Key, Value) in this)
        {
            if (set.Contains(Key))
            {
                res = res.AddOrUpdateInPlace(Key, Value);
            }
            else
            {
                changes = changes.AddOrUpdateInPlace(Key, Change<V>.Removed(Value));
            }
        }
        return (res, changes);
    }

    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    public TrieMap<K, V> Intersect(IEnumerable<(K Key, V Value)> other)
    {
        var res = new List<(K, V)>();
        foreach (var (Key, Value) in other)
        {
            GetOption(Key).Do(res.Add);
        }
        return new TrieMap<K, V>(res);
    }

    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) IntersectWithLog(
        IEnumerable<(K Key, V Value)> other) =>
        IntersectWithLog(other.Select(pair => pair.Key));

    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    public TrieMap<K, V> Intersect(
        IEnumerable<(K Key, V Value)> other,
        WhenMatched<K, V, V, V> Merge)
    {
        var t = Empty(_equalityComparer);
        foreach (var (Key, Value) in other)
        {
            var px = Find(Key);
            if (px.IsSome)
            {
                var r = Merge(Key, px.Value, Value);
                t = t.AddOrUpdateInPlace(Key, r);
            }
        }
        return t;
    }

    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) IntersectWithLog(
        TrieMap<K, V> other,
        WhenMatched<K, V, V, V> Merge, IEqualityComparer<V> equalityComparer)
    {
        var t = Empty(_equalityComparer);
        var c = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        foreach (var (Key, Value) in this)
        {
            var py = other.Find(Key);
            if (py.IsSome)
            {
                var r = Merge(Key, Value, py.Value);
                t = t.AddOrUpdateInPlace(Key, r);
                if (!equalityComparer.Equals(Value, r))
                {
                    c = c.AddOrUpdateInPlace(Key, Change<V>.Mapped(Value, r));
                }
            }
            else
            {
                c = c.AddOrUpdateInPlace(Key, Change<V>.Removed(Value));
            }
        }

        return (t, c);
    }

    /// <summary>
    /// Returns this - other.  Only the items in this that are not in 
    /// other will be returned.
    /// </summary>
    public TrieMap<K, V> Except(IEnumerable<K> other)
    {
        var self = this;
        foreach (var item in other)
        {
            self = self.Remove(item);
        }
        return self;
    }
        
    /// <summary>
    /// Returns this - other.  Only the items in this that are not in 
    /// other will be returned.
    /// </summary>
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) ExceptWithLog(IEnumerable<K> other)
    {
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var self    = this;
            
        foreach (var item in other)
        {
            var pair = self.RemoveWithLog(item);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(item, pair.Change);
            }
        }
        return (self, changes);
    }        

    /// <summary>
    /// Returns this - other.  Only the items in this that are not in 
    /// other will be returned.
    /// </summary>
    public TrieMap<K, V> Except(IEnumerable<(K Key, V Value)> other)
    {
        var self = this;
        foreach (var (Key, Value) in other)
        {
            self = self.Remove(Key);
        }
        return self;
    }

    /// <summary>
    /// Returns this - other.  Only the items in this that are not in 
    /// other will be returned.
    /// </summary>
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) ExceptWithLog(
        IEnumerable<(K Key, V Value)> other) =>
        ExceptWithLog(other.Select(p => p.Key));
 
    /// <summary>
    /// Only items that are in one set or the other will be returned.
    /// If an item is in both, it is dropped.
    /// </summary>
    public TrieMap<K, V> SymmetricExcept(TrieMap<K, V> rhs)
    {
        var self = this;
            
        foreach (var (Key, Value) in rhs)
        {
            var pair = self.RemoveWithLog(Key);
            if (pair.Change.HasNoChange)
            {
                self = self.Add(Key, Value);
            }
        }
        return self;
    }        
        
    /// <summary>
    /// Only items that are in one set or the other will be returned.
    /// If an item is in both, it is dropped.
    /// </summary>
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) SymmetricExceptWithLog(TrieMap<K, V> rhs)
    {
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var self    = this;
            
        foreach (var (Key, Value) in rhs)
        {
            var pair = self.RemoveWithLog(Key);
            if (pair.Change.HasNoChange)
            {
                self = self.Add(Key, Value);
                changes = changes.AddOrUpdateInPlace(Key, Change<V>.Added(Value));
            }
        }
        return (self, changes);
    }        

    /// <summary>
    /// Only items that are in one set or the other will be returned.
    /// If an item is in both, it is dropped.
    /// </summary>
    public TrieMap<K, V> SymmetricExcept(IEnumerable<(K Key, V Value)> rhs)
    {
        var self = this;
            
        foreach (var (Key, Value) in rhs)
        {
            var pair = self.RemoveWithLog(Key);
            if (pair.Change.HasNoChange)
            {
                self = self.Add(Key, Value);
            }
            else
            {
                self = pair.Map;
            }
        }
        return self;
    }
        
    /// <summary>
    /// Only items that are in one set or the other will be returned.
    /// If an item is in both, it is dropped.
    /// </summary>
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) SymmetricExceptWithLog(IEnumerable<(K Key, V Value)> rhs)
    {
        var changes = TrieMap<K, Change<V>>.Empty(_equalityComparer);
        var self    = this;
            
        foreach (var (Key, Value) in rhs)
        {
            var pair = self.RemoveWithLog(Key);
            if (pair.Change.HasNoChange)
            {
                self = self.Add(Key, Value);
                changes = changes.AddOrUpdateInPlace(Key, Change<V>.Added(Value));
            }
            else
            {
                self = pair.Map;
                changes = changes.AddOrUpdateInPlace(Key, pair.Change);
            }
        }
        return (self, changes);
    }

    /// <summary>
    /// Finds the union of two sets and produces a new set with 
    /// the results
    /// </summary>
    /// <param name="other">Other set to union with</param>
    /// <returns>A set which contains all items from both sets</returns>
    public TrieMap<K, V> Union(IEnumerable<(K, V)> other) =>
        TryAddRange(other);

    /// <summary>
    /// Finds the union of two sets and produces a new set with 
    /// the results
    /// </summary>
    /// <param name="other">Other set to union with</param>
    /// <param name="equalityComparer">Value equality comparer</param>
    /// <returns>A set which contains all items from both sets</returns>
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) UnionWithLog(IEnumerable<(K, V)> other, IEqualityComparer<V> equalityComparer) =>
        TryAddRangeWithLog(other, equalityComparer);
        
    /// <summary>
    /// Union two maps.  
    /// </summary>
    /// <remarks>
    /// The `WhenMatched` merge function is called when keys are present in both map to allow resolving to a
    /// sensible value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<K, V> Union(
        IEnumerable<(K Key, V Value)> other,
        WhenMatched<K, V, V, V> Merge) =>
        Union(other, MapLeft: static (_, v) => v, MapRight: static (_, v) => v, Merge);

    /// <summary>
    /// Union two maps.  
    /// </summary>
    /// <remarks>
    /// The `WhenMatched` merge function is called when keys are present in both map to allow resolving to a
    /// sensible value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) UnionWithLog(
        IEnumerable<(K Key, V Value)> other,
        WhenMatched<K, V, V, V> Merge, IEqualityComparer<V> equalityComparer) =>
        UnionWithLog(other, MapLeft: static (_, v) => v, MapRight: static (_, v) => v, Merge, equalityComparer, v => v);
        
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
    public TrieMap<K, V> Union<W>(
        IEnumerable<(K Key, W Value)> other,
        WhenMissing<K, W, V> MapRight, 
        WhenMatched<K, V, W, V> Merge) =>
        Union(other, MapLeft: static (_, v) => v, MapRight, Merge);

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
    public (TrieMap<K, V> Map, TrieMap<K, Change<V>> Changes) UnionWithLog<W>(
        IEnumerable<(K Key, W Value)> other, 
        WhenMissing<K, W, V> MapRight, 
        WhenMatched<K, V, W, V> Merge, IEqualityComparer<V> equalityComparer) =>
        UnionWithLog(other, MapLeft: static (_, v) => v, MapRight, Merge, equalityComparer, v => v);

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
    public TrieMap<K, R> Union<W, R>(
        IEnumerable<(K Key, W Value)> other, 
        WhenMissing<K, V, R> MapLeft, 
        WhenMissing<K, W, R> MapRight, 
        WhenMatched<K, V, W, R> Merge)
    {
        var t = TrieMap<K, R>.Empty(_equalityComparer);
        foreach(var (key, value) in other)
        {
            var px = Find(key);
            t = t.AddOrUpdateInPlace(key, px.IsSome 
                                              ? Merge(key, px.Value, value) 
                                              : MapRight(key, value));
        }

        foreach (var (key, value) in this)
        {
            if (t.ContainsKey(key)) continue;
            t = t.AddOrUpdateInPlace(key, MapLeft(key, value));
        }

        return t;
    }
        
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
    public (TrieMap<K, R> Map, TrieMap<K, Change<R>> Changes) UnionWithLog<W, R>(
        IEnumerable<(K Key, W Value)> other, 
        WhenMissing<K, V, R> MapLeft,
        WhenMissing<K, W, R> MapRight, 
        WhenMatched<K, V, W, R> Merge,
        IEqualityComparer<V> equalityComparer,
        Func<R, V?> match)
    {
        var t = TrieMap<K, R>.Empty(_equalityComparer);
        var c = TrieMap<K, Change<R>>.Empty(_equalityComparer);
        foreach(var (key, value) in other)
        {
            var px = Find(key);
            if (px.IsSome)
            {
                var r = Merge(key, px.Value, value);
                t = t.AddOrUpdateInPlace(key, r);
                if (!equalityComparer.Equals(px.Value, match(r)))
                {
                    c = c.AddOrUpdateInPlace(key, Change<R>.Mapped(px.Value, r));
                }
            }
            else
            {
                var r = MapRight(key, value);
                t = t.AddOrUpdateInPlace(key, r);
                c = c.AddOrUpdateInPlace(key, Change<R>.Added(r));
            }
        }

        foreach (var (key, value) in this)
        {
            if (t.ContainsKey(key)) continue;
                
            var r = MapLeft(key, value);
            t = t.AddOrUpdateInPlace(key, r);
            if (!equalityComparer.Equals(value, match(r)))
            {
                c = c.AddOrUpdateInPlace(key, Change<R>.Mapped(value, r));
            }
        }

        return (t, c);
    }

    public bool HasSameEqualityComparer(IEqualityComparer<K> equalityComparer) => 
        _equalityComparer == equalityComparer;
        
    /// <summary>
    /// Nodes in the CHAMP hash trie map can be in one of three states:
    /// 
    ///     Empty - nothing in the map
    ///     Entries - contains items and sub-nodes
    ///     Collision - keeps track of items that have different keys but the same hash
    /// 
    /// </summary>
    internal abstract class Node : IEnumerable<(K, V)>
    {
        public abstract TrieTag Type { get; }
        public abstract Option<(K Key, V Value)> Read(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer);
        public abstract Option<(int CountDelta, Node Node, V? Old)> Update(UpdateContext env, (K Key, V Value) change, uint hash, Sec section, IEqualityComparer<K> equalityComparer);
        public abstract Option<(int CountDelta, Node Node, V Old)> Remove(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer);
        public abstract IEnumerator<(K, V)> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    ///////////////////////////////////////////////////////////////////////////////////////////
    //
    // NOTE: Here be dragons!  The code below is has been optimized for performance.  Yes, it's 
    //       ugly, yes there's repetition, but it's all to squeeze the last few nanoseconds of 
    //       performance out of the system.  Don't hate me ;)
    //

    /// <summary>
    /// Contains items and sub-nodes
    /// </summary>
    internal sealed class Entries : Node
    {
        public readonly uint EntryMap;
        public readonly uint NodeMap;
        public readonly (K Key, V Value)[] Items;
        public readonly Node[] Nodes;

        public override TrieTag Type => TrieTag.Entries;

        public Entries(uint entryMap, uint nodeMap, (K, V)[] items, Node[] nodes)
        {
            EntryMap = entryMap;
            NodeMap = nodeMap;
            Items = items;
            Nodes = nodes;
        }

        public override Option<(int CountDelta, Node Node, V Old)> Remove(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer)
        {
            var hashIndex = Bit.Get(hash, section);
            var mask      = Bit.Mask(hashIndex);

            if (Bit.Get(EntryMap, mask))
            {
                // If key belongs to an entry
                var entryIndex = Bit.Index(EntryMap, mask);
                if (equalityComparer.Equals(Items[entryIndex].Key, key))
                {
                    var v = Items[entryIndex].Value;
                    return (-1, 
                            new Entries(
                                Bit.Set(EntryMap, mask, false), 
                                NodeMap,
                                RemoveAt(Items, entryIndex), 
                                Nodes),
                            v
                           );
                }
                return default;
            }
            else if (Bit.Get(NodeMap, mask))
            {
                //If key lies in a sub-node
                var entryIndex = Bit.Index(NodeMap, mask);
                var removed = Nodes[entryIndex].Remove(key, hash, section.Next(), equalityComparer);
                if (removed)
                { 
                    var (cd, subNode, v) = removed.Value;
                    switch (subNode.Type)
                    {
                        case TrieTag.Entries:

                            var subEntries = (Entries)subNode;

                            if (subEntries.Items.Length == 1 && subEntries.Nodes.Length == 0)
                            {
                                // If the node only has one subnode, make that subnode the new node
                                if (Items.Length == 0 && Nodes.Length == 1)
                                {
                                    // Build a new Entries for this level with the sublevel mask fixed
                                    return (cd, new Entries(
                                                Bit.Mask(Bit.Get((uint)equalityComparer.GetHashCode(subEntries.Items[0].Key!),
                                                            section)),
                                                0,
                                                Clone(subEntries.Items),
                                                Array.Empty<Node>()
                                            ),
                                            v);
                                }
                                else
                                {
                                    return (cd,
                                            new Entries(
                                                Bit.Set(EntryMap, mask, true),
                                                Bit.Set(NodeMap, mask, false),
                                                Insert(Items, Bit.Index(EntryMap, mask), subEntries.Items[0]),
                                                RemoveAt(Nodes, entryIndex)),
                                            v);
                                }
                            }
                            else
                            {
                                var nodeCopy = Clone(Nodes);
                                nodeCopy[entryIndex] = subNode;
                                return (cd, new Entries(EntryMap, NodeMap, Items, nodeCopy), v);
                            }

                        case TrieTag.Collision:
                            var nodeCopy2 = Clone(Nodes);
                            nodeCopy2[entryIndex] = subNode;
                            return (cd, new Entries(EntryMap, NodeMap, Items, nodeCopy2), v);
                        
                    }
                }
            }
            
            return default;
        }

        public override Option<(K Key, V Value)> Read(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer)
        {                                                                                         
            var hashIndex = Bit.Get(hash, section);
            var mask = Bit.Mask(hashIndex);

            if(Bit.Get(EntryMap, mask))
            {
                var entryIndex = Bit.Index(EntryMap, mask);
                if (equalityComparer.Equals(Items[entryIndex].Key, key))
                {
                    var (Key, Value) = Items[entryIndex];
                    return Some((Key, Value));
                }
            }
            else if (Bit.Get(NodeMap, mask))
            {
                var entryIndex = Bit.Index(NodeMap, mask);
                return Nodes[entryIndex].Read(key, hash, section.Next(), equalityComparer);
            }
            
            return default;
        }

        public override Option<(int CountDelta, Node Node, V? Old)> Update(UpdateContext env, (K Key, V Value) change, uint hash, Sec section, IEqualityComparer<K> equalityComparer)
        {
            var hashIndex = Bit.Get(hash, section);
            var mask = Bit.Mask(hashIndex);

            if (Bit.Get(EntryMap, mask))
            {
                var entryIndex = Bit.Index(EntryMap, mask);
                
                var currentEntry = Items[entryIndex];

                if (equalityComparer.Equals(currentEntry.Key, change.Key))
                {
                    if (env.Type == UpdateType.Add)
                    {
                        // Key already exists - so it's an error to add again
                        throw new ArgumentException($"Key already exists in map: {change.Key}");
                    }
                    else if (env.Type == UpdateType.TryAdd)
                    {
                        // Already added, so we don't continue to try
                        return default;
                    }

                    var (newItems, old) = SetItem(Items, entryIndex, change, env.Mutate);
                    return (0, new Entries(EntryMap, NodeMap, newItems, Nodes), old.Value);
                }
                else
                {
                    if (env.Type == UpdateType.SetItem)
                    {
                        // Key must already exist to set it
                        throw new ArgumentException($"Key already exists in map: {change.Key}");
                    }
                    else if (env.Type == UpdateType.TrySetItem)
                    {
                        // Key doesn't exist, so there's nothing to set
                        return default;
                    }

                    // Add
                    var node = Merge(change, currentEntry, hash, (uint)equalityComparer.GetHashCode(currentEntry.Key!), section);

                    //var newItems = Items.Filter(elem => !default(EqK).Equals(elem.Key, currentEntry.Key)).ToArray();
                    var newItems = new (K Key, V Value)[Items.Length - 1];
                    var i        = 0;
                    foreach(var elem in Items)
                    {
                        if(!equalityComparer.Equals(elem.Key, currentEntry.Key))
                        {
                            newItems[i] = elem;
                            i++;
                        }
                    }

                    var newEntryMap = Bit.Set(EntryMap, mask, false);
                    var newNodeMap = Bit.Set(NodeMap, mask, true);
                    
                    var nodeIndex = Bit.Index(NodeMap, mask);
                    
                    var newNodes = Insert(Nodes, nodeIndex, node);

                    return (1, new Entries(newEntryMap, newNodeMap, newItems, newNodes), default);
                }
            }
            else if (Bit.Get(NodeMap, mask))
            {
                var nodeIndex = Bit.Index(NodeMap, mask);

                var nodeToUpdate = Nodes[nodeIndex];
                var changed = nodeToUpdate.Update(env, change, hash, section.Next(), equalityComparer);
                if(changed)
                {
                    var (countDelta, newNode, ov) = changed.Value;
                    var (newNodes, _) = SetItem(Nodes, nodeIndex, newNode, env.Mutate);
                    Node entries = new Entries(EntryMap, NodeMap, Items, newNodes);
                    return (countDelta, entries, ov);
                }
                return default;
            }
            else
            {
                if (env.Type == UpdateType.SetItem)
                {
                    // Key must already exist to set it
                    throw new ArgumentException($"Key doesn't exist in map: {change.Key}");
                }
                else if (env.Type == UpdateType.TrySetItem)
                {
                    // Key doesn't exist, so there's nothing to set
                    return default;
                }

                var entryIndex = Bit.Index(EntryMap, mask);
                
                var entries = Bit.Set(EntryMap, mask, true);
                
                var newItems = Insert(Items, entryIndex, change);
                return (1, new Entries(entries, NodeMap, newItems, Nodes), default);
            }
        }

        public override IEnumerator<(K, V)> GetEnumerator()
        {
            foreach (var item in Items)
            {
                yield return item;
            }

            foreach (var node in Nodes)
            {
                foreach (var item in node)
                {
                    yield return item;
                }
            }
        }
    }

    /// <summary>
    /// Contains items that share the same hash but have different keys
    /// </summary>
    internal sealed class Collision : Node
    {
        public readonly (K Key, V Value)[] Items;
        public readonly uint Hash;

        public override TrieTag Type => TrieTag.Collision;

        public Collision((K Key, V Value)[] items, uint hash)
        {
            Items = items;
            Hash = hash;
        }

        public override Option<(K Key, V Value)> Read(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer)
        {
            foreach (var (Key, Value) in Items)
            {
                if (equalityComparer.Equals(Key, key))
                {
                    return (Key, Value);
                }
            }
            return default;
        }

        public override Option<(int CountDelta, Node Node, V Old)> Remove(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer)
        {
            var len = Items.Length;
            if (len      == 0) return default;
            else if (len == 1 && equalityComparer.Equals(Items[0].Key, key)) return (-1, EmptyNode.Default, Items[0].Value);
            else if (len == 2 && equalityComparer.Equals(Items[0].Key, key))
            {
                var env = new UpdateContext(UpdateType.Add, false);
                var (_, n, _) = EmptyNode.Default.Update(env, Items[1], hash, default, equalityComparer).Value;
                var ov = Items[0].Value;

                return (-1, n, ov);
            }
            else if (len == 2 && equalityComparer.Equals(Items[1].Key, key))
            {
                var env = new UpdateContext(UpdateType.Add, false);
                var (_, n, _) = EmptyNode.Default.Update(env, Items[0], hash, default, equalityComparer).Value;
                var ov = Items[1].Value;

                return (-1, n, ov);
            }
            
            var (left, right) = Items.Partition(item => equalityComparer.Equals(item.Key, key), 1);

            if (left.Length > 0)
            {
                Node collision = new Collision(right, hash);
                return (right.Length - Items.Length, collision, left[0].Value);
            }

            return default;
        }

        public override Option<(int CountDelta, Node Node, V? Old)> Update(UpdateContext env, (K Key, V Value) change, uint hash, Sec section, IEqualityComparer<K> equalityComparer)
        {
            var index = -1;
            for (var i = 0; i < Items.Length; i++)
            {
                if (equalityComparer.Equals(Items[i].Key, change.Key))
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                if (env.Type == UpdateType.Add)
                {
                    // Key already exists - so it's an error to add again
                    throw new ArgumentException($"Key already exists in map: {change.Key}");
                }
                else if (env.Type == UpdateType.TryAdd)
                {
                    // Already added, so we don't continue to try
                    return default;
                }

                var (newArr, ov) = SetItem(Items, index, change, false);
                return (0, new Collision(newArr, hash), ov.Value);
            }
            else
            {
                if (env.Type == UpdateType.SetItem)
                {
                    // Key must already exist to set it
                    throw new ArgumentException($"Key doesn't exist in map: {change.Key}");
                }
                else if (env.Type == UpdateType.TrySetItem)
                {
                    // Key doesn't exist, so there's nothing to set
                    return default;
                }

                var result = new (K, V)[Items.Length + 1];
                Array.Copy(Items, result, Items.Length);
                result[Items.Length] = change;
                return (1, new Collision(result, hash), default);
            }
        }

        public override IEnumerator<(K, V)> GetEnumerator() =>
            Items.AsEnumerable().GetEnumerator();
    }

    /// <summary>
    /// Empty node
    /// </summary>
    internal sealed class EmptyNode : Node
    {
        public static readonly EmptyNode Default = new();

        public override TrieTag Type => TrieTag.Empty;

        public override Option<(K Key, V Value)> Read(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer) =>
            default;

        public override Option<(int CountDelta, Node Node, V Old)> Remove(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer) =>
            default;

        public override Option<(int CountDelta, Node Node, V? Old)> Update(UpdateContext env, (K Key, V Value) change, uint hash, Sec section, IEqualityComparer<K> equalityComparer)
        {
            if (env.Type == UpdateType.SetItem)
            {
                // Key must already exist to set it
                throw new ArgumentException($"Key doesn't exist in map: {change.Key}");
            }
            else if (env.Type == UpdateType.TrySetItem)
            {
                // Key doesn't exist, so there's nothing to set
                return default;
            }

            var dataMap = Bit.Mask(Bit.Get(hash, section));
            return (1, new Entries(dataMap, 0, [change], []), default);
        }

        public override IEnumerator<(K, V)> GetEnumerator()
        {
            yield break;
        }   
    }

    /// <summary>
    /// Merges two key-value pairs into a single Node
    /// </summary>
    static Node Merge((K, V) pair1, (K, V) pair2, uint pair1Hash, uint pair2Hash, Sec section)
    {
        if (section.Offset >= 25)
        {
            return new Collision([pair1, pair2], pair1Hash);
        }
        else
        {
            var nextLevel  = section.Next();
            var pair1Index = Bit.Get(pair1Hash, nextLevel);
            var pair2Index = Bit.Get(pair2Hash, nextLevel);
            if (pair1Index == pair2Index)
            {
                var node    = Merge(pair1, pair2, pair1Hash, pair2Hash, nextLevel);
                var nodeMap = Bit.Mask(pair1Index);
                return new Entries(0, nodeMap, Array.Empty<(K, V)>(), [node]);
            }
            else
            {
                var dataMap = Bit.Mask(pair1Index);
                dataMap = Bit.Set(dataMap, Bit.Mask(pair2Index), true);
                return new Entries(dataMap, 0, pair1Index < pair2Index
                                                   ? [pair1, pair2]
                                                   : [pair2, pair1], Array.Empty<Node>());
            }
        }
    }

    public Iterable<(K Key, V Value)> AsIterable() =>
        Root.AsIterable();

    public IEnumerator<(K Key, V Value)> GetEnumerator() =>
        // ReSharper disable once NotDisposedResourceIsReturned
        Root.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        // ReSharper disable once NotDisposedResourceIsReturned
        Root.GetEnumerator();

    /// <summary>
    /// Sets the item at index. If mutate is true it sets the 
    /// value without copying the array, otherwise the operation is pure
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static (A[] Items, A Old) SetItem<A>(A[] items, int index, A value, bool mutate)
    {
        if (mutate)
        {
            var old = items[index]; 
            items[index] = value;
            return (items, old);
        }
        else
        {
            var old    = items[index]; 
            var result = new A[items.Length];
            Array.Copy(items, result, items.Length);
            result[index] = value;
            return (result, old);
        }
    }

    /// <summary>
    /// Clones an existing array
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static A[] Clone<A>(A[] items)
    {
        var len    = items.Length;
        var result = new A[len];
        Array.Copy(items, result, len);
        return result;
    }

    /// <summary>
    /// Inserts a new item in the array (immutably)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static A[] Insert<A>(A[] array, int index, A value)
    {
        var result = new A[array.Length + 1];
        Array.Copy(array, 0, result, 0, index);
        Array.Copy(array, index, result, index + 1, array.Length - index);
        result[index] = value;
        return result;
    }

    /// <summary>
    /// Returns a new array with the item at index removed
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static A[] RemoveAt<A>(A[] array, int index)
    {
        if (array.Length == 0)
        {
            return array;
        }

        var result = new A[array.Length - 1];
        if (index > 0)
        {
            Array.Copy(array, 0, result, 0, index);
        }
        if (index + 1 < array.Length)
        {
            Array.Copy(array, index + 1, result, index, array.Length - index - 1);
        }
        return result;
    }

    public override string ToString() =>
        _count < 50
            ? $"[{ string.Join(", ", AsIterable().Select(TupleToString)) }]"
            : $"[{ string.Join(", ", AsIterable().Select(TupleToString).Take(50)) } ... ]";

    string TupleToString((K Key, V Value) tuple) =>
        $"({tuple.Key}, {tuple.Value})";
    internal TrieMap<K, V> Clear() => 
        Empty(_equalityComparer);

    public Iterable<K> Keys =>
        AsIterable().Map(kv => kv.Key);

    public Iterable<V> Values =>
        AsIterable().Map(kv => kv.Value);
}

internal static class TrieMapExtensions
{
    public static TrieMap<K, V> Merge<K, V>(this TrieMap<K, V> lhs, TrieMap<K, V> rhs)
        where V : Semigroup<V>
    {
        var self = lhs;
        foreach (var (Key, Value) in rhs)
        {
            var ix = self.Find(Key);
            if (ix.IsSome)
            {
                self = self.SetItem(Key, ix.Value.Combine(Value));
            }
            else
            {
                self = self.Add(Key, Value);
            }
        }
        return self;
    }
}
