using LanguageExt.Traits;
using static LanguageExt.Prelude;
using static LanguageExt.Bit;
using System;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using LanguageExt.ClassInstances;
using Array = System.Array;

namespace LanguageExt;

/// <summary>
/// Implementation of the CHAMP trie hash map data structure (Compressed Hash Array Map Trie)
/// [efficient-immutable-collections.pdf](https://michael.steindorfer.name/publications/phd-thesis-efficient-immutable-collections.pdf)
/// </summary>
/// <remarks>
/// Used by internally by `LanguageExt.HashMap`
/// </remarks>
internal sealed class TrieMap<EqK, K, V> :
    IEnumerable<(K Key, V Value)>,
    IEquatable<TrieMap<EqK, K, V>>
    where EqK : Eq<K>
{
    internal enum UpdateType
    {
        Add,
        TryAdd,
        AddOrUpdate,
        SetItem,
        TrySetItem
    }

    internal enum Tag
    {
        Entries,
        Collision,
        Empty
    }

    public static readonly TrieMap<EqK, K, V> Empty = new (EmptyNode.Default, 0);
    internal static TrieMap<EqK, K, V> EmptyForMutating => new (new EmptyNode(), 0);

    readonly Node Root;
    readonly int count;
    int hash;

    /// <summary>
    /// Ctor
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    TrieMap(Node root, int count)
    {
        Root = root;
        this.count = count;
    }

    public TrieMap(IEnumerable<(K Key, V Value)> items, bool tryAdd = true)
    {
        Root = EmptyNode.Default;
        var type = tryAdd ? UpdateType.TryAdd : UpdateType.AddOrUpdate;
        foreach (var item in items)
        {
            var h       = (uint)EqK.GetHashCode(item.Key);
            Sec section = default;
            var (countDelta, newRoot, _) = Root.Update((type, true), item, h, section);
            count += countDelta;
            Root = newRoot;
        }
    }

    public TrieMap(ReadOnlySpan<(K Key, V Value)> items, bool tryAdd = true)
    {
        Root = EmptyNode.Default;
        var type = tryAdd ? UpdateType.TryAdd : UpdateType.AddOrUpdate;
        foreach (var item in items)
        {
            var h       = (uint)EqK.GetHashCode(item.Key);
            Sec section = default;
            var (countDelta, newRoot, _) = Root.Update((type, true), item, h, section);
            count += countDelta;
            Root = newRoot;
        }
    }

    /// <summary>
    /// True if no items in the map
    /// </summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => count == 0;
    }

    /// <summary>
    /// Number of items in the map
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => count;
    }

    /// <summary>
    /// Add an item to the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> Add(K key, V value) =>
        Update(key, value, UpdateType.Add, false);

    /// <summary>
    /// Add an item to the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, Change<V> Change) AddWithLog(K key, V value) =>
        UpdateWithLog(key, value, UpdateType.Add, false);

    /// <summary>
    /// Try to add an item to the map.  If it already exists, do
    /// nothing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> TryAdd(K key, V value) =>
        Update(key, value, UpdateType.TryAdd, false);

    /// <summary>
    /// Try to add an item to the map.  If it already exists, do
    /// nothing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, Change<V> Change) TryAddWithLog(K key, V value) =>
        UpdateWithLog(key, value, UpdateType.TryAdd, false);

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> AddOrUpdate(K key, V value) =>
        Update(key, value, UpdateType.AddOrUpdate, false);

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TrieMap<EqK, K, V> AddOrUpdateInPlace(K key, V value) =>
        Update(key, value, UpdateType.AddOrUpdate, true);

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, Change<V> Change) AddOrUpdateWithLog(K key, V value) =>
        UpdateWithLog(key, value, UpdateType.AddOrUpdate, false);

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> AddOrUpdate(K key, Func<V, V> Some, Func<V> None)
    {
        var (found, _, value) = FindInternal(key);
        return found
                   ? AddOrUpdate(key, Some(value!))
                   : AddOrUpdate(key, None());
    }

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, Change<V> Change) AddOrUpdateWithLog(K key, Func<V, V> Some, Func<V> None)
    {
        var (found, _, value) = FindInternal(key);
        return found
                   ? AddOrUpdateWithLog(key, Some(value!))
                   : AddOrUpdateWithLog(key, None());
    }

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> AddOrMaybeUpdate(K key, Func<V, V> Some, Func<Option<V>> None)
    {
        var (found, _, value) = FindInternal(key);
        return found
                   ? AddOrUpdate(key, Some(value!))
                   : None().Map(x => AddOrUpdate(key, x)).IfNone(this);
    }

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> AddOrUpdate(K key, Func<V, V> Some, V None)
    {
        var (found, _, value) = FindInternal(key);
        return found
                   ? AddOrUpdate(key, Some(value!))
                   : AddOrUpdate(key, None);
    }

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, Change<V> Change) AddOrUpdateWithLog(K key, Func<V, V> Some, V None)
    {
        var (found, _, value) = FindInternal(key);
        return found
                   ? AddOrUpdateWithLog(key, Some(value!))
                   : AddOrUpdateWithLog(key, None);
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> AddRange(IEnumerable<(K Key, V Value)> items)
    {
        var self = this;
        foreach (var (Key, Value) in items)
        {
            self = self.Add(Key, Value);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) AddRangeWithLog(IEnumerable<(K Key, V Value)> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var (Key, Value) in items)
        {
            var pair = self.AddWithLog(Key, Value);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(Key, pair.Change);
            }
        }
        return (self, changes);
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> AddRange(IEnumerable<Tuple<K, V>> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.Add(item.Item1, item.Item2);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) AddRangeWithLog(IEnumerable<Tuple<K, V>> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var item in items)
        {
            var pair = self.AddWithLog(item.Item1, item.Item2);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(item.Item1, pair.Change);
            }
        }
        return (self, changes);
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> AddRange(IEnumerable<KeyValuePair<K, V>> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.Add(item.Key, item.Value);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) AddRangeWithLog(IEnumerable<KeyValuePair<K, V>> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var item in items)
        {
            var pair = self.AddWithLog(item.Key, item.Value);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(item.Key, pair.Change);
            }
        }
        return (self, changes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> TryAddRange(IEnumerable<(K Key, V Value)> items)
    {
        var self = this;
        foreach (var (Key, Value) in items)
        {
            self = self.TryAdd(Key, Value);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) TryAddRangeWithLog(IEnumerable<(K Key, V Value)> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var (Key, Value) in items)
        {
            var pair = self.TryAddWithLog(Key, Value);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(Key, pair.Change);
            }
        }
        return (self, changes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> TryAddRange(IEnumerable<Tuple<K, V>> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.TryAdd(item.Item1, item.Item2);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) TryAddRangeWithLog(IEnumerable<Tuple<K, V>> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var item in items)
        {
            var pair = self.TryAddWithLog(item.Item1, item.Item2);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(item.Item1, pair.Change);
            }
        }
        return (self, changes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> TryAddRange(IEnumerable<KeyValuePair<K, V>> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.TryAdd(item.Key, item.Value);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) TryAddRangeWithLog(IEnumerable<KeyValuePair<K, V>> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var item in items)
        {
            var pair = self.TryAddWithLog(item.Key, item.Value);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(item.Key, pair.Change);
            }
        }
        return (self, changes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> AddOrUpdateRange(IEnumerable<(K Key, V Value)> items)
    {
        var self = this;
        foreach (var (Key, Value) in items)
        {
            self = self.AddOrUpdate(Key, Value);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) AddOrUpdateRangeWithLog(IEnumerable<(K Key, V Value)> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var (Key, Value) in items)
        {
            var pair = self.AddOrUpdateWithLog(Key, Value);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(Key, pair.Change);
            }
        }
        return (self, changes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> AddOrUpdateRange(IEnumerable<Tuple<K, V>> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.AddOrUpdate(item.Item1, item.Item2);
        }
        return self;
    }
       
    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) AddOrUpdateRangeWithLog(IEnumerable<Tuple<K, V>> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var item in items)
        {
            var pair = self.AddOrUpdateWithLog(item.Item1, item.Item2);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(item.Item1, pair.Change);
            }
        }
        return (self, changes);
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> AddOrUpdateRange(IEnumerable<KeyValuePair<K, V>> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.AddOrUpdate(item.Key, item.Value);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) AddOrUpdateRangeWithLog(IEnumerable<KeyValuePair<K, V>> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var item in items)
        {
            var pair = self.AddOrUpdateWithLog(item.Key, item.Value);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(item.Key, pair.Change);
            }
        }
        return (self, changes);
    }        

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> SetItems(IEnumerable<(K Key, V Value)> items)
    {
        var self = this;
        foreach (var (Key, Value) in items)
        {
            self = self.SetItem(Key, Value);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) SetItemsWithLog(IEnumerable<(K Key, V Value)> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var (Key, Value) in items)
        {
            var pair = self.SetItemWithLog(Key, Value);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(Key, pair.Change);
            }
        }
        return (self, changes);
    }        

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> SetItems(IEnumerable<KeyValuePair<K, V>> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.SetItem(item.Key, item.Value);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) SetItemsWithLog(IEnumerable<KeyValuePair<K, V>> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var item in items)
        {
            var pair = self.SetItemWithLog(item.Key, item.Value);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(item.Key, pair.Change);
            }
        }
        return (self, changes);
    }        

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> SetItems(IEnumerable<Tuple<K, V>> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.SetItem(item.Item1, item.Item2);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) SetItemsWithLog(IEnumerable<Tuple<K, V>> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var item in items)
        {
            var pair = self.SetItemWithLog(item.Item1, item.Item2);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(item.Item1, pair.Change);
            }
        }
        return (self, changes);
    }        

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> TrySetItems(IEnumerable<(K Key, V Value)> items)
    {
        var self = this;
        foreach (var (Key, Value) in items)
        {
            self = self.TrySetItem(Key, Value);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) TrySetItemsWithLog(IEnumerable<(K Key, V Value)> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var (Key, Value) in items)
        {
            var pair = self.TrySetItemWithLog(Key, Value);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(Key, pair.Change);
            }
        }
        return (self, changes);
    }        

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> TrySetItems(IEnumerable<KeyValuePair<K, V>> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.TrySetItem(item.Key, item.Value);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) TrySetItemsWithLog(IEnumerable<KeyValuePair<K, V>> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var item in items)
        {
            var pair = self.TrySetItemWithLog(item.Key, item.Value);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(item.Key, pair.Change);
            }
        }
        return (self, changes);
    }        

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> TrySetItems(IEnumerable<Tuple<K, V>> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.TrySetItem(item.Item1, item.Item2);
        }
        return self;
    }

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) TrySetItemsWithLog(IEnumerable<Tuple<K, V>> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var item in items)
        {
            var pair = self.TrySetItemWithLog(item.Item1, item.Item2);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(item.Item1, pair.Change);
            }
        }
        return (self, changes);
    }  
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> TrySetItems(IEnumerable<K> items, Func<V, V> Some)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.TrySetItem(item, Some);
        }
        return self;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) TrySetItemsWithLog(IEnumerable<K> items, Func<V, V> Some)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var item in items)
        {
            var pair = self.TrySetItemWithLog(item, Some);
            self = pair.Map;
            if (pair.Change.HasChanged)
            {
                changes = changes.AddOrUpdateInPlace(item, pair.Change);
            }
        }
        return (self, changes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> RemoveRange(IEnumerable<K> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.Remove(item);
        }
        return self;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V>, TrieMap<EqK, K, Change<V>> Changes) RemoveRangeWithLog(IEnumerable<K> items)
    {
        var self    = this;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
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
    public TrieMap<EqK, K, V> SetItem(K key, V value) =>
        Update(key, value, UpdateType.SetItem, false);

    /// <summary>
    /// Set an item that already exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, Change<V> Change) SetItemWithLog(K key, V value) =>
        UpdateWithLog(key, value, UpdateType.SetItem, false);
        
    /// <summary>
    /// Set an item that already exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> SetItem(K key, Func<V, V> Some)
    {
        var value = Find(key).Map(Some).IfNone(() => throw new ArgumentException($"Key doesn't exist in map: {key}"));
        return SetItem(key, value);
    }
        
    /// <summary>
    /// Set an item that already exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, Change<V> Change) SetItemWithLog(K key, Func<V, V> Some)
    {
        var value = Find(key).Map(Some).IfNone(() => throw new ArgumentException($"Key doesn't exist in map: {key}"));
        return SetItemWithLog(key, value);
    }

    /// <summary>
    /// Try to set an item that already exists in the map.  If none
    /// exists, do nothing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> TrySetItem(K key, V value) =>
        Update(key, value, UpdateType.TrySetItem, false);

    /// <summary>
    /// Try to set an item that already exists in the map.  If none
    /// exists, do nothing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, Change<V> Change) TrySetItemWithLog(K key, V value) =>
        UpdateWithLog(key, value, UpdateType.TrySetItem, false);

    /// <summary>
    /// Set an item that already exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> TrySetItem(K key, Func<V, V> Some) =>
        Find(key)
           .Map(Some)
           .Match(Some: v => SetItem(key, v),
                  None: () => this);

    /// <summary>
    /// Set an item that already exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, Change<V> Change) TrySetItemWithLog(K key, Func<V, V> Some) =>
        Find(key)
           .Map(Some)
           .Match(Some: v => SetItemWithLog(key, v),
                  None: () => (this, Change<V>.None));

    /// <summary>
    /// Remove an item from the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> Remove(K key)
    {
        var h       = (uint)EqK.GetHashCode(key);
        Sec section = default;
        var (countDelta, newRoot, _) = Root.Remove(key, h, section);
        return ReferenceEquals(newRoot, Root)
                   ? this
                   : new TrieMap<EqK, K, V>(newRoot, count + countDelta);
    }

    /// <summary>
    /// Remove an item from the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, Change<V> Change) RemoveWithLog(K key)
    {
        var h       = (uint)EqK.GetHashCode(key);
        Sec section = default;
        var (countDelta, newRoot, old) = Root.Remove(key, h, section);
        return ReferenceEquals(newRoot, Root)
                   ? (this, Change<V>.None)
                   : (new TrieMap<EqK, K, V>(newRoot, count + countDelta), 
                      countDelta == 0
                          ? Change<V>.None
                          : Change<V>.Removed(old!));
    }

    /// <summary>
    /// Indexer
    /// </summary>
    public V this[K key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var (found, _, value) = FindInternal(key);
            return found
                       ? value!
                       : throw new ArgumentException($"Key doesn't exist in map: {key}");
        }
    }

    /// <summary>
    /// Get a key value pair from a key
    /// </summary>
    public Option<(K Key, V Value)> GetOption(K key)
    {
        var (found, newKey, value) = FindInternal(key);
        return found
                   ? Some((newKey, value!))
                   : default;
    }

    /// <summary>
    /// Create an empty map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> Clear() =>
        Empty;

    /// <summary>
    /// Create an empty map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>>) ClearWithLog() =>
        (Empty, Map(Change<V>.Removed));

    /// <summary>
    /// Get the hash code of the items in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() =>
        hash == 0
            ? (hash = FNV32.Hash<HashableTuple<EqK, HashableDefault<V>, K, V>, (K, V)>(AsIterable()))
            : hash;

    /// <summary>
    /// Returns the whether the `key` exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(K key) =>
        FindInternal(key).Found;

    /// <summary>
    /// Returns the whether the `value` exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(V value) =>
        Contains<EqDefault<V>>(value);

    /// <summary>
    /// Returns the whether the `value` exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains<EqV>(V value) where EqV: Eq<V> =>
        Values.Exists(v => EqV.Equals(v, value));

    /// <summary>
    /// Returns the whether the `key` exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(K key, V value) =>
        Contains<EqDefault<V>>(key, value);

    /// <summary>
    /// Returns the whether the `key` exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains<EqV>(K key, V Value) where EqV : Eq<V> =>
        Find(key).Map(v => EqV.Equals(v, Value)).IfNone(false);

    /// <summary>
    /// Returns the value associated with `key`.  Or None, if no key exists
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<V> Find(K key)
    {
        var (found, _, value) = FindInternal(key);
        return found
                   ? Some(value!)
                   : default;
    }

    /// <summary>
    /// Returns the value associated with `key`.  Or None, if no key exists
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    (bool Found, K Key, V? Value) FindInternal(K key)
    {
        var h       = (uint)EqK.GetHashCode(key);
        Sec section = default;
        return Root.Read(key, h, section);
    }

    /// <summary>
    /// Returns the value associated with `key` then match the result
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public R Find<R>(K key, Func<V, R> Some, Func<R> None)
    {
        var (found, _, value) = FindInternal(key);
        return found
                   ? Some(value!)
                   : None();
    }

    /// <summary>
    /// Tries to find the value, if not adds it and returns the update map and/or value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, V Value) FindOrAdd(K key, Func<V> None) =>
        Find(key, Some: v => (this, v), None: () =>
                                              {
                                                  var v = None();
                                                  return (Add(key, v), v);
                                              });

    /// <summary>
    /// Tries to find the value, if not adds it and returns the update map and/or value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, V Value, Change<V> Change) FindOrAddWithLog(K key, Func<V> None)
    {
        var item = Find(key);
        if (item.IsSome)
        {
            return (this, item.Value!, Change<V>.None);
        }
        else
        {
            var v    = None();
            var self = AddWithLog(key, v);
            return (self.Map, v, self.Change);
        }
    }

    /// <summary>
    /// Tries to find the value, if not adds it and returns the update map and/or value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, V Value) FindOrAdd(K key, V value) =>
        Find(key, Some: v => (this, v), None: () => (Add(key, value), value));

    /// <summary>
    /// Tries to find the value, if not adds it and returns the update map and/or value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, V Value, Change<V> Change) FindOrAddWithLog(K key, V value)
    {
        var item = Find(key);
        if (item.IsSome)
        {
            return (this, item.Value!, Change<V>.None);
        }
        else
        {
            var self = AddWithLog(key, value);
            return (self.Map, value, self.Change);
        }
    }   
        
    /// <summary>
    /// Tries to find the value, if not adds it and returns the update map and/or value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, Option<V> Value) FindOrMaybeAdd(K key, Func<Option<V>> None) =>
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
    public (TrieMap<EqK, K, V> Map, V Value, Change<V> Change) FindOrMaybeAddWithLog(K key, Func<Option<V>> None)
    {
        var item = Find(key);
        if (item.IsSome)
        {
            return (this, item.Value!, Change<V>.None);
        }
        else
        {
            var v = None();
            if (v.IsSome)
            {
                var self = AddWithLog(key, v.Value!);
                return (self.Map, v.Value!, self.Change);
            }
            else
            {
                return (this, item.Value!, Change<V>.None);
            }
        }
    }        

    /// <summary>
    /// Tries to find the value, if not adds it and returns the update map and/or value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, Option<V> Value) FindOrMaybeAdd(K key, Option<V> value) =>
        Find(key, Some: v => (this, v), None: () =>
                                                  value.IsSome
                                                      ? (Add(key, (V)value), value)
                                                      : (this, value));

    /// <summary>
    /// Tries to find the value, if not adds it and returns the update map and/or value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, Option<V> Value, Change<V> Change) FindOrMaybeAddWithLog(K key, Option<V> value)
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
                var self = AddWithLog(key, value.Value!);
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
    public TrieMap<EqK, K, U> Map<U>(Func<V, U> f) =>
        new (AsIterable().Select(kv => (kv.Key, f(kv.Value))), false);

    /// <summary>
    /// Map from V to U
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, U> Map, TrieMap<EqK, K, Change<U>> Changes) MapWithLog<U>(Func<V, U> f)
    {
        var target  = TrieMap<EqK, K, U>.EmptyForMutating;
        var changes = TrieMap<EqK, K, Change<U>>.EmptyForMutating;
            
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
    public TrieMap<EqK, K, U> Map<U>(Func<K, V, U> f) =>
        new (AsIterable().Select(kv => (kv.Key, f(kv.Key, kv.Value))), false);

    /// <summary>
    /// Filter
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> Filter(Func<V, bool> f) =>
        new (AsIterable().Filter(kv => f(kv.Value)), false);

    /// <summary>
    /// Map from V to U
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) FilterWithLog(Func<V, bool> f)
    {
        var target  = EmptyForMutating;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
            
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
    public TrieMap<EqK, K, V> Filter(Func<K, V, bool> f) =>
        new (AsIterable().Filter(kv => f(kv.Key, kv.Value)), false);

    /// <summary>
    /// Map from V to U
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) FilterWithLog(Func<K, V, bool> f)
    {
        var target  = EmptyForMutating;
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
            
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
    public TrieMap<EqK, K, V> Append(TrieMap<EqK, K, V> rhs) =>
        TryAddRange(rhs.AsIterable());

    /// <summary>
    /// Associative union
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) AppendWithLog(TrieMap<EqK, K, V> rhs) =>
        TryAddRangeWithLog(rhs.AsIterable());

    /// <summary>
    /// Subtract
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> Subtract(TrieMap<EqK, K, V> rhs)
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
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) SubtractWithLog(TrieMap<EqK, K, V> rhs)
    {
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
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
    public static bool operator ==(TrieMap<EqK, K, V> lhs, TrieMap<EqK, K, V> rhs) =>
        lhs.Equals(rhs);

    /// <summary>
    /// Non equality
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(TrieMap<EqK, K, V> lhs, TrieMap<EqK, K, V> rhs) =>
        !(lhs == rhs);

    /// <summary>
    /// Equality
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? rhs) =>
        rhs is TrieMap<EqK, K, V> map && Equals<EqDefault<V>>(map);

    /// <summary>
    /// Equality
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(TrieMap<EqK, K, V>? rhs) =>
        rhs is not null && Equals<EqDefault<V>>(rhs);

    /// <summary>
    /// Equality
    /// </summary>
    public bool Equals<EqV>(TrieMap<EqK, K, V>? rhs)
        where EqV : Eq<V>
    {
        if (ReferenceEquals(this, rhs)) return true;
        if (rhs is null) return false;
        if (Count != rhs.Count) return false;
        using var iterA = GetEnumerator();
        using var iterB = rhs.GetEnumerator();
        while (iterA.MoveNext() && iterB.MoveNext())
        {
            if (!EqK.Equals(iterA.Current.Key, iterB.Current.Key)) return false;
        }
        using var iterA1 = GetEnumerator();
        using var iterB1 = rhs.GetEnumerator();
        while (iterA1.MoveNext() && iterB1.MoveNext())
        {
            if (!EqV.Equals(iterA1.Current.Value, iterB1.Current.Value)) return false;
        }
        return true;
    }
        
    /// <summary>
    /// Update an item in the map - can mutate if needed
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    TrieMap<EqK, K, V> Update(K key, V value, UpdateType type, bool mutate)
    {
        var h       = (uint)EqK.GetHashCode(key);
        Sec section = default;
        var (countDelta, newRoot, _) = Root.Update((type, mutate), (key, value), h, section);
        return ReferenceEquals(newRoot, Root)
                   ? this
                   : new TrieMap<EqK, K, V>(newRoot, count + countDelta);
    }

    /// <summary>
    /// Update an item in the map - can mutate if needed
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    (TrieMap<EqK, K, V> Map, Change<V> Change) UpdateWithLog(K key, V value, UpdateType type, bool mutate)
    {
        var h       = (uint)EqK.GetHashCode(key);
        Sec section = default;
        var (countDelta, newRoot, oldV) = Root.Update((type, mutate), (key, value), h, section);
        return ReferenceEquals(newRoot, Root)
                   ? (this, Change<V>.None)
                   : (new TrieMap<EqK, K, V>(newRoot, count + countDelta), 
                      countDelta == 0 
                          ? EqDefault<V>.Equals(oldV!, value)
                                ? Change<V>.None 
                                : Change<V>.Mapped(oldV, value)
                          : Change<V>.Added(value));
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
    public bool IsSubsetOf(TrieMap<EqK, K, V> other)
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
    public TrieMap<EqK, K, V> Intersect(IEnumerable<K> other)
    {
        var res = new List<(K, V)>();
        foreach (var item in other)
        {
            GetOption(item).Do(res.Add);
        }
        return new TrieMap<EqK, K, V>(res);
    }

    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) IntersectWithLog(IEnumerable<K> other)
    {
        var set     = new TrieSet<K>(other, equalityComparer: Traits.Eq.Comparer<EqK, K>());
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        var res     = EmptyForMutating;
            
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
    public TrieMap<EqK, K, V> Intersect(IEnumerable<(K Key, V Value)> other)
    {
        var res = new List<(K, V)>();
        foreach (var (Key, Value) in other)
        {
            GetOption(Key).Do(res.Add);
        }
        return new TrieMap<EqK, K, V>(res);
    }

    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) IntersectWithLog(
        IEnumerable<(K Key, V Value)> other) =>
        IntersectWithLog(other.Select(pair => pair.Key));

    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    public TrieMap<EqK, K, V> Intersect(
        IEnumerable<(K Key, V Value)> other,
        WhenMatched<K, V, V, V> Merge)
    {
        var t = EmptyForMutating;
        foreach (var (Key, Value) in other)
        {
            var px = Find(Key);
            if (px.IsSome)
            {
                var r = Merge(Key, px.Value!, Value);
                t = t.AddOrUpdateInPlace(Key, r);
            }
        }
        return t;
    }

    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) IntersectWithLog(
        TrieMap<EqK, K, V> other,
        WhenMatched<K, V, V, V> Merge)
    {
        var t = EmptyForMutating;
        var c = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
        foreach (var (Key, Value) in this)
        {
            var py = other.Find(Key);
            if (py.IsSome)
            {
                var r = Merge(Key, Value, py.Value!);
                t = t.AddOrUpdateInPlace(Key, r);
                if (!EqDefault<V>.Equals(Value, r))
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
    public TrieMap<EqK, K, V> Except(IEnumerable<K> other)
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
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) ExceptWithLog(IEnumerable<K> other)
    {
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
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
    public TrieMap<EqK, K, V> Except(IEnumerable<(K Key, V Value)> other)
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
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) ExceptWithLog(
        IEnumerable<(K Key, V Value)> other) =>
        ExceptWithLog(other.Select(p => p.Key));
 
    /// <summary>
    /// Only items that are in one set or the other will be returned.
    /// If an item is in both, it is dropped.
    /// </summary>
    public TrieMap<EqK, K, V> SymmetricExcept(TrieMap<EqK, K, V> rhs)
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
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) SymmetricExceptWithLog(TrieMap<EqK, K, V> rhs)
    {
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
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
    public TrieMap<EqK, K, V> SymmetricExcept(IEnumerable<(K Key, V Value)> rhs)
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
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) SymmetricExceptWithLog(IEnumerable<(K Key, V Value)> rhs)
    {
        var changes = TrieMap<EqK, K, Change<V>>.EmptyForMutating;
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
    public TrieMap<EqK, K, V> Union(IEnumerable<(K, V)> other) =>
        TryAddRange(other);

    /// <summary>
    /// Finds the union of two sets and produces a new set with 
    /// the results
    /// </summary>
    /// <param name="other">Other set to union with</param>
    /// <returns>A set which contains all items from both sets</returns>
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) UnionWithLog(IEnumerable<(K, V)> other) =>
        TryAddRangeWithLog(other);
        
    /// <summary>
    /// Union two maps.  
    /// </summary>
    /// <remarks>
    /// The `WhenMatched` merge function is called when keys are present in both map to allow resolving to a
    /// sensible value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieMap<EqK, K, V> Union(
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
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) UnionWithLog(
        IEnumerable<(K Key, V Value)> other,
        WhenMatched<K, V, V, V> Merge) =>
        UnionWithLog(other, MapLeft: static (_, v) => v, MapRight: static (_, v) => v, Merge);
        
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
    public TrieMap<EqK, K, V> Union<W>(
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
    public (TrieMap<EqK, K, V> Map, TrieMap<EqK, K, Change<V>> Changes) UnionWithLog<W>(
        IEnumerable<(K Key, W Value)> other, 
        WhenMissing<K, W, V> MapRight, 
        WhenMatched<K, V, W, V> Merge) =>
        UnionWithLog(other, MapLeft: static (_, v) => v, MapRight, Merge);

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
    public TrieMap<EqK, K, R> Union<W, R>(
        IEnumerable<(K Key, W Value)> other, 
        WhenMissing<K, V, R> MapLeft, 
        WhenMissing<K, W, R> MapRight, 
        WhenMatched<K, V, W, R> Merge)
    {
        var t = TrieMap<EqK, K, R>.EmptyForMutating;
        foreach(var (key, value) in other)
        {
            var px = Find(key);
            t = t.AddOrUpdateInPlace(key, px.IsSome 
                                              ? Merge(key, px.Value!, value) 
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
    public (TrieMap<EqK, K, R> Map, TrieMap<EqK, K, Change<R>> Changes) UnionWithLog<W, R>(
        IEnumerable<(K Key, W Value)> other, 
        WhenMissing<K, V, R> MapLeft,
        WhenMissing<K, W, R> MapRight, 
        WhenMatched<K, V, W, R> Merge)
    {
        var t = TrieMap<EqK, K, R>.EmptyForMutating;
        var c = TrieMap<EqK, K, Change<R>>.EmptyForMutating;
        foreach(var (key, value) in other)
        {
            var px = Find(key);
            if (px.IsSome)
            {
                var r = Merge(key, px.Value!, value);
                t = t.AddOrUpdateInPlace(key, r);
                if (!EqDefault<V, W>.Equals(px.Value, r))
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
            if (!EqDefault<V, W>.Equals(value, r))
            {
                c = c.AddOrUpdateInPlace(key, Change<R>.Mapped(value, r));
            }
        }

        return (t, c);
    }
        
    /// <summary>
    /// Nodes in the CHAMP hash trie map can be in one of three states:
    /// 
    ///     Empty - nothing in the map
    ///     Entries - contains items and sub-nodes
    ///     Collision - keeps track of items that have different keys but the same hash
    /// 
    /// </summary>
    internal interface Node : IEnumerable<(K, V)>
    {
        Tag Type { get; }
        (bool Found, K Key, V? Value) Read(K key, uint hash, Sec section);
        (int CountDelta, Node Node, V? Old) Update((UpdateType Type, bool Mutate) env, (K Key, V Value) change, uint hash, Sec section);
        (int CountDelta, Node Node, V? Old) Remove(K key, uint hash, Sec section);
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

        public Tag Type => Tag.Entries;

        public Entries(uint entryMap, uint nodeMap, (K, V)[] items, Node[] nodes)
        {
            EntryMap = entryMap;
            NodeMap = nodeMap;
            Items = items;
            Nodes = nodes;
        }

        public (int CountDelta, Node Node, V? Old) Remove(K key, uint hash, Sec section)
        {
            var hashIndex = Bit.Get(hash, section);
            var mask      = Mask(hashIndex);

            if (Bit.Get(EntryMap, mask))
            {
                // If key belongs to an entry
                var ind = Index(EntryMap, mask);
                if (EqK.Equals(Items[ind].Key, key))
                {
                    var v = Items[ind].Value;
                    return (-1, 
                            new Entries(
                                Bit.Set(EntryMap, mask, false), 
                                NodeMap,
                                RemoveAt(Items, ind), 
                                Nodes),
                            v
                           );
                }
                else
                {
                    return (0, this, default);
                }
            }
            else if (Bit.Get(NodeMap, mask))
            {
                //If key lies in a sub-node
                var ind = Index(NodeMap, mask);
                var (cd, subNode, v) = Nodes[ind].Remove(key, hash, section.Next());
                if (cd == 0) return (0, this, default);

                switch (subNode.Type)
                {
                    case Tag.Entries:

                        var subEntries = (Entries)subNode;

                        if (subEntries.Items.Length == 1 && subEntries.Nodes.Length == 0)
                        {
                            // If the node only has one subnode, make that subnode the new node
                            if (Items.Length == 0 && Nodes.Length == 1)
                            {
                                // Build a new Entries for this level with the sublevel mask fixed
                                return (cd, new Entries(
                                            Mask(Bit.Get((uint)EqK.GetHashCode(subEntries.Items[0].Key),
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
                                            Insert(Items, Index(EntryMap, mask), subEntries.Items[0]),
                                            RemoveAt(Nodes, ind)),
                                        v);
                            }
                        }
                        else
                        {
                            var nodeCopy = Clone(Nodes);
                            nodeCopy[ind] = subNode;
                            return (cd, new Entries(EntryMap, NodeMap, Items, nodeCopy), v);
                        }

                    case Tag.Collision:
                        var nodeCopy2 = Clone(Nodes);
                        nodeCopy2[ind] = subNode;
                        return (cd, new Entries(EntryMap, NodeMap, Items, nodeCopy2), v);

                    default:
                        return (0, this, default);
                }
            }
            else
            {
                return (0, this, default);
            }
        }

        public (bool Found, K Key, V? Value) Read(K key, uint hash, Sec section)
        {                                                                                         
            // var hashIndex = Bit.Get(hash, section);
            // Mask(hashIndex)
            var mask = (uint)(1 << (int)((hash & (uint)(Sec.Mask << section.Offset)) >> section.Offset));

            // if(Bit.Get(EntryMap, mask))
            if ((EntryMap & mask) == mask)                                                        
            {
                // var entryIndex = Index(EntryMap, mask);
                var entryIndex = Count((int)EntryMap & (((int)mask) - 1));                     
                if (EqK.Equals(Items[entryIndex].Key, key))
                {
                    var (Key, Value) = Items[entryIndex];
                    return (true, Key, Value);
                }
                else
                {
                    return default;
                }
            }
            // else if (Bit.Get(NodeMap, mask))
            else if ((NodeMap & mask) == mask)                                                   
            {
                // var entryIndex = Index(NodeMap, mask);
                var entryIndex = Count((int)NodeMap & ((int)mask - 1));                     
                return Nodes[entryIndex].Read(key, hash, section.Next());
            }
            else
            {
                return default;
            }
        }

        public (int CountDelta, Node Node, V? Old) Update((UpdateType Type, bool Mutate) env, (K Key, V Value) change, uint hash, Sec section)
        {
            // var hashIndex = Bit.Get(hash, section);
            // var mask = Mask(hashIndex);
            var mask = (uint)(1 << (int)((hash & (uint)(Sec.Mask << section.Offset)) >> section.Offset));

            //if (Bit.Get(EntryMap, mask))
            if((EntryMap & mask) == mask)
            {
                //var entryIndex = Index(EntryMap, mask);
                var entryIndex   = Count((int)EntryMap & ((int)mask - 1));
                var currentEntry = Items[entryIndex];

                if (EqK.Equals(currentEntry.Key, change.Key))
                {
                    if (env.Type == UpdateType.Add)
                    {
                        // Key already exists - so it's an error to add again
                        throw new ArgumentException($"Key already exists in map: {change.Key}");
                    }
                    else if (env.Type == UpdateType.TryAdd)
                    {
                        // Already added, so we don't continue to try
                        return (0, this, default);
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
                        return (0, this, default);
                    }

                    // Add
                    var node = Merge(change, currentEntry, hash, (uint)EqK.GetHashCode(currentEntry.Key), section);

                    //var newItems = Items.Filter(elem => !default(EqK).Equals(elem.Key, currentEntry.Key)).ToArray();
                    var newItems = new (K Key, V Value)[Items.Length - 1];
                    var i        = 0;
                    foreach(var elem in Items)
                    {
                        if(!EqK.Equals(elem.Key, currentEntry.Key))
                        {
                            newItems[i] = elem;
                            i++;
                        }
                    }

                    //var newEntryMap = Bit.Set(EntryMap, mask, false);
                    var newEntryMap = EntryMap & (~mask);

                    // var newNodeMap = Bit.Set(NodeMap, mask, true);
                    var newNodeMap = NodeMap | mask;

                    // var nodeIndex = Index(NodeMap, mask);
                    var nodeIndex = Count((int)NodeMap & ((int)mask - 1));

                    var newNodes = Insert(Nodes, nodeIndex, node);

                    return (1, new Entries(
                                newEntryMap, 
                                newNodeMap, 
                                newItems, 
                                newNodes), 
                            default);
                }
            }
            else if (Bit.Get(NodeMap, mask))
            {
                // var nodeIndex = Index(NodeMap, mask);
                var nodeIndex = Count((int)NodeMap & ((int)mask - 1));

                var nodeToUpdate = Nodes[nodeIndex];
                var (cd, newNode, ov) = nodeToUpdate.Update(env, change, hash, section.Next());
                var (newNodes, _) = SetItem(Nodes, nodeIndex, newNode, env.Mutate);
                return (cd, new Entries(EntryMap, NodeMap, Items, newNodes), ov);
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
                    return (0, this, default);
                }

                // var entryIndex = Index(EntryMap, mask);
                var entryIndex = Count((int)EntryMap & ((int)mask - 1));

                // var entries = Bit.Set(EntryMap, mask, true);
                var entries = EntryMap | mask;

                var newItems = Insert(Items, entryIndex, change);
                return (1, new Entries(entries, NodeMap, newItems, Nodes), default);
            }
        }

        public IEnumerator<(K, V)> GetEnumerator()
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

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }

    /// <summary>
    /// Contains items that share the same hash but have different keys
    /// </summary>
    internal sealed class Collision : Node
    {
        public readonly (K Key, V Value)[] Items;
        public readonly uint Hash;

        public Tag Type => Tag.Collision;

        public Collision((K Key, V Value)[] items, uint hash)
        {
            Items = items;
            Hash = hash;
        }

        public (bool Found, K Key, V Value) Read(K key, uint hash, Sec section)
        {
            foreach (var (Key, Value) in Items)
            {
                if (EqK.Equals(Key, key))
                {
                    return (true, Key, Value);
                }
            }
            return default;
        }

        public (int CountDelta, Node Node, V? Old) Remove(K key, uint hash, Sec section)
        {
            var len = Items.Length;
            if (len      == 0) return (0, this, default);
            else if (len == 1) return (-1, EmptyNode.Default, Items[0].Value);
            else if (len == 2)
            {
                var ((_, n, _), ov) = EqK.Equals(Items[0].Key, key)
                                          ? (EmptyNode.Default.Update((UpdateType.Add, false), Items[1], hash, default), Items[0].Value)
                                          : (EmptyNode.Default.Update((UpdateType.Add, false), Items[0], hash, default), Items[1].Value);

                return (-1, n, ov);
            }
            else
            {
                V? oldValue = default;
                IEnumerable<(K, V)> Yield((K Key, V Value)[] items, K ikey)
                {
                    foreach (var item in items)
                    {
                        if (EqK.Equals(item.Key, ikey))
                        {
                            oldValue = item.Value;
                        }
                        else
                        {
                            yield return item;
                        }
                    }
                }

                var result = Yield(Items, key).ToArray();

                return (result.Length - Items.Length, new Collision(result, hash), oldValue);
            }
        }

        public (int CountDelta, Node Node, V? Old) Update((UpdateType Type, bool Mutate) env, (K Key, V Value) change, uint hash, Sec section)
        {
            var index = -1;
            for (var i = 0; i < Items.Length; i++)
            {
                if (EqK.Equals(Items[i].Key, change.Key))
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
                    return (0, this, default);
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
                    return (0, this, default);
                }

                var result = new (K, V)[Items.Length + 1];
                Array.Copy(Items, result, Items.Length);
                result[Items.Length] = change;
                return (1, new Collision(result, hash), default);
            }
        }

        public IEnumerator<(K, V)> GetEnumerator() =>
            Items.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            Items.AsEnumerable().GetEnumerator();
    }

    /// <summary>
    /// Empty node
    /// </summary>
    internal sealed class EmptyNode : Node
    {
        public static readonly EmptyNode Default = new EmptyNode();

        public Tag Type => Tag.Empty;

        public (bool Found, K Key, V Value) Read(K key, uint hash, Sec section) =>
            default;

        public (int CountDelta, Node Node, V? Old) Remove(K key, uint hash, Sec section) =>
            (0, this, default);

        public (int CountDelta, Node Node, V? Old) Update((UpdateType Type, bool Mutate) env, (K Key, V Value) change, uint hash, Sec section)
        {
            if (env.Type == UpdateType.SetItem)
            {
                // Key must already exist to set it
                throw new ArgumentException($"Key doesn't exist in map: {change.Key}");
            }
            else if (env.Type == UpdateType.TrySetItem)
            {
                // Key doesn't exist, so there's nothing to set
                return (0, this, default);
            }

            var dataMap = Mask(Bit.Get(hash, section));
            return (1, new Entries(dataMap, 0, [change], Array.Empty<Node>()), default);
        }

        public IEnumerator<(K, V)> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
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
                var nodeMap = Mask(pair1Index);
                return new Entries(0, nodeMap, Array.Empty<(K, V)>(), [node]);
            }
            else
            {
                var dataMap = Mask(pair1Index);
                dataMap = Bit.Set(dataMap, Mask(pair2Index), true);
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
    /// Finds the number of 1-bits below the bit at `location`
    /// This function is used to find where in the array of entries or nodes 
    /// the item should be inserted
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Index(uint bitmap, uint location) =>
        Count((int)bitmap & ((int)location - 1));

    /// <summary>
    /// Returns the value used to index into the bit vector
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint Mask(int index) =>
        (uint)(1 << index);

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
        count < 50
            ? $"[{ string.Join(", ", AsIterable().Select(TupleToString)) }]"
            : $"[{ string.Join(", ", AsIterable().Select(TupleToString).Take(50)) } ... ]";

    string TupleToString((K Key, V Value) tuple) =>
        $"({tuple.Key}, {tuple.Value})";

    public Iterable<K> Keys =>
        AsIterable().Map(kv => kv.Key);

    public Iterable<V> Values =>
        AsIterable().Map(kv => kv.Value);
}

internal readonly struct Sec
{
    public const int Mask = 31;
    public readonly int Offset;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Sec(int offset) =>
        Offset = offset;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Sec Next() =>
        new (Offset + 5);
}

internal static class TrieMapExtensions
{
    public static TrieMap<EqK, K, V> Merge<EqK, K, V>(this TrieMap<EqK, K, V> lhs, TrieMap<EqK, K, V> rhs)
        where EqK : Eq<K>
        where V : Semigroup<V>
    {
        var self = lhs;
        foreach (var (Key, Value) in rhs)
        {
            var ix = self.Find(Key);
            if (ix.IsSome)
            {
                self = self.SetItem(Key, ix.Value!.Combine(Value));
            }
            else
            {
                self = self.Add(Key, Value);
            }
        }
        return self;
    }
}
