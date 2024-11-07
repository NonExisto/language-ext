﻿using LanguageExt.Traits;
using static LanguageExt.Prelude;
using static LanguageExt.Bit;
using System;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace LanguageExt;

/// <summary>
/// Implementation of the CHAMP trie hash map data structure (Compressed Hash Array Map Trie)
/// https://michael.steindorfer.name/publications/phd-thesis-efficient-immutable-collections.pdf
/// </summary>
/// <remarks>
/// Used by internally by `LanguageExt.HashSet`
/// </remarks>
internal sealed class TrieSet<EqK, K> :
    IEquatable<TrieSet<EqK, K>>,
    IReadOnlyCollection<K>
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

    public static readonly TrieSet<EqK, K> Empty = new (EmptyNode.Default, 0);

    readonly Node Root;
    readonly int count;
    int hash;

    /// <summary>
    /// Ctor
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    TrieSet(Node root, int count)
    {
        Root = root;
        this.count = count;
    }

    public TrieSet(ReadOnlySpan<K> items, bool tryAdd = true)
    {
        Root = EmptyNode.Default;
        var type = tryAdd ? UpdateType.TryAdd : UpdateType.AddOrUpdate;
        foreach (var item in items)
        {
            var hash    = (uint)EqK.GetHashCode(item);
            Sec section = default;
            var (countDelta, newRoot) = Root.Update((type, true), item, hash, section);
            count += countDelta;
            Root = newRoot;
        }
    }

    public TrieSet(IEnumerable<K> items, bool tryAdd = true)
    {
        Root = EmptyNode.Default;
        var type = tryAdd ? UpdateType.TryAdd : UpdateType.AddOrUpdate;
        foreach (var item in items)
        {
            var hash    = (uint)EqK.GetHashCode(item);
            Sec section = default;
            var (countDelta, newRoot) = Root.Update((type, true), item, hash, section);
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
    public TrieSet<EqK, K> Add(K key) =>
        Update(key, UpdateType.Add, false);

    /// <summary>
    /// Try to add an item to the map.  If it already exists, do
    /// nothing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<EqK, K> TryAdd(K key) =>
        Update(key, UpdateType.TryAdd, false);

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<EqK, K> AddOrUpdate(K key) =>
        Update(key, UpdateType.AddOrUpdate, false);

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<EqK, K> AddRange(IEnumerable<K> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.Add(item);
        }
        return self;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<EqK, K> TryAddRange(IEnumerable<K> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.TryAdd(item);
        }
        return self;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<EqK, K> AddOrUpdateRange(IEnumerable<K> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.AddOrUpdate(item);
        }
        return self;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<EqK, K> SetItems(IEnumerable<K> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.SetItem(item);
        }
        return self;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<EqK, K> TrySetItems(IEnumerable<K> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.TrySetItem(item);
        }
        return self;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<EqK, K> RemoveRange(IEnumerable<K> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.Remove(item);
        }
        return self;
    }

    /// <summary>
    /// Set an item that already exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<EqK, K> SetItem(K key) =>
        Update(key, UpdateType.SetItem, false);

    /// <summary>
    /// Try to set an item that already exists in the map.  If none
    /// exists, do nothing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<EqK, K> TrySetItem(K key) =>
        Update(key, UpdateType.TrySetItem, false);

    /// <summary>
    /// Remove an item from the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<EqK, K> Remove(K key)
    {
        var hash    = (uint)EqK.GetHashCode(key);
        Sec section = default;
        var (countDelta, newRoot) = Root.Remove(key, hash, section);
        return ReferenceEquals(newRoot, Root)
                   ? this
                   : new TrieSet<EqK, K>(newRoot, count + countDelta);
    }

    /// <summary>
    /// Indexer
    /// </summary>
    public K this[K key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var (found, newKey) = FindInternal(key);
            return found
                       ? newKey
                       : throw new ArgumentException($"Key doesn't exist in map: {key}");
        }
    }

    /// <summary>
    /// Get the hash code of the items in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() =>
        hash == 0
            ? (hash = FNV32.Hash<EqK, K>(AsEnumerable()))
            : hash;

    /// <summary>
    /// Returns the whether the `key` exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(K key) =>
        FindInternal(key).Found;

    /// <summary>
    /// Returns the value associated with `key`.  Or None, if no key exists
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<K> Find(K key)
    {
        var (found, newKey) = FindInternal(key);
        return found
                   ? Some(newKey)
                   : default;
    }

    /// <summary>
    /// Returns the value associated with `key`.  Or None, if no key exists
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    (bool Found, K Key) FindInternal(K key)
    {
        var hash    = (uint)EqK.GetHashCode(key);
        Sec section = default;
        return Root.Read(key, hash, section);
    }

    /// <summary>
    /// Associative union
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<EqK, K> Append(TrieSet<EqK, K> rhs) =>
        TryAddRange(rhs.AsEnumerable());

    /// <summary>
    /// Subtract
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<EqK, K> Subtract(TrieSet<EqK, K> rhs)
    {
        var lhs = this;
        foreach (var item in rhs)
        {
            lhs = lhs.Remove(item);
        }
        return lhs;
    }

    /// <summary>
    /// Equality
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(TrieSet<EqK, K> lhs, TrieSet<EqK, K> rhs) =>
        lhs.Equals(rhs);

    /// <summary>
    /// Non equality
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(TrieSet<EqK, K> lhs, TrieSet<EqK, K> rhs) =>
        !(lhs == rhs);

    /// <summary>
    /// Equality
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? rhs) =>
        rhs is TrieSet<EqK, K> map && Equals(map);

    /// <summary>
    /// Equality
    /// </summary>
    public bool Equals(TrieSet<EqK, K>? rhs)
    {
        if (ReferenceEquals(this, rhs)) return true;
        if (ReferenceEquals(rhs, null)) return false;
        if (Count != rhs.Count) return false;
        using var iterA = GetEnumerator();
        using var iterB = rhs.GetEnumerator();
        while (iterA.MoveNext() && iterB.MoveNext())
        {
            if (!EqK.Equals(iterA.Current, iterB.Current)) return false;
        }
        return true;
    }

    /// <summary>
    /// Update an item in the map - can mutate if needed
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    TrieSet<EqK, K> Update(K key, UpdateType type, bool mutate)
    {
        var hash    = (uint)EqK.GetHashCode(key);
        Sec section = default;
        var (countDelta, newRoot) = Root.Update((type, mutate), key, hash, section);
        return ReferenceEquals(newRoot, Root)
                   ? this
                   : new TrieSet<EqK, K>(newRoot, count + countDelta);
    }

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

        int  matches    = 0;
        bool extraFound = false;
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
    public bool IsProperSupersetOf(IEnumerable<K> other)
    {
        if (IsEmpty)
        {
            return false;
        }

        int matchCount = 0;
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
    public bool IsSubsetOf(IEnumerable<K> other)
    {
        if (IsEmpty)
        {
            return true;
        }

        int matches = 0;
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
    public TrieSet<EqK, K> Intersect(IEnumerable<K> other)
    {
        var res = new List<K>();
        foreach (var item in other)
        {
            Find(item).Do(res.Add);
        }
        return new TrieSet<EqK, K>(res);
    }

    /// <summary>
    /// Returns this - other.  Only the items in this that are not in 
    /// other will be returned.
    /// </summary>
    public TrieSet<EqK, K> Except(IEnumerable<K> other)
    {
        var self = this;
        foreach (var item in other)
        {
            self = self.Remove(item);
        }
        return self;
    }

    /// <summary>
    /// Only items that are in one set or the other will be returned.
    /// If an item is in both, it is dropped.
    /// </summary>
    public TrieSet<EqK, K> SymmetricExcept(IEnumerable<K> rhs) =>
        SymmetricExcept(new TrieSet<EqK, K>(rhs, true));

    /// <summary>
    /// Only items that are in one set or the other will be returned.
    /// If an item is in both, it is dropped.
    /// </summary>
    public TrieSet<EqK, K> SymmetricExcept(TrieSet<EqK, K> rhs)
    {
        var res = new List<K>();

        foreach (var item in this)
        {
            if (!rhs.ContainsKey(item))
            {
                res.Add(item);
            }
        }

        foreach (var item in rhs)
        {
            if (!ContainsKey(item))
            {
                res.Add(item);
            }
        }

        return new TrieSet<EqK, K>(res);
    }

    /// <summary>
    /// Nodes in the CHAMP hash trie map can be in one of three states:
    /// 
    ///     Empty - nothing in the map
    ///     Entries - contains items and sub-nodes
    ///     Collision - keeps track of items that have different keys but the same hash
    /// 
    /// </summary>
    internal interface Node : IEnumerable<K>
    {
        Tag Type { get; }
        (bool Found, K Key) Read(K key, uint hash, Sec section);
        (int CountDelta, Node Node) Update((UpdateType Type, bool Mutate) env, K change, uint hash, Sec section);
        (int CountDelta, Node Node) Remove(K key, uint hash, Sec section);
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
        public readonly K[] Items;
        public readonly Node[] Nodes;

        public Tag Type => Tag.Entries;

        public Entries(uint entryMap, uint nodeMap, K[] items, Node[] nodes)
        {
            EntryMap = entryMap;
            NodeMap = nodeMap;
            Items = items;
            Nodes = nodes;
        }

        public (int CountDelta, Node Node) Remove(K key, uint hash, Sec section)
        {
            var hashIndex = Bit.Get(hash, section);
            var mask      = Mask(hashIndex);

            if (Bit.Get(EntryMap, mask))
            {
                // If key belongs to an entry
                var ind = Index(EntryMap, mask);
                if (EqK.Equals(Items[ind], key))
                {
                    return (-1, 
                            new Entries(
                                Bit.Set(EntryMap, mask, false), 
                                NodeMap,
                                RemoveAt(Items, ind), 
                                Nodes));
                }
                else
                {
                    return (0, this);
                }
            }
            else if (Bit.Get(NodeMap, mask))
            {
                //If key lies in a sub-node
                var ind = Index(NodeMap, mask);
                var (cd, subNode) = Nodes[ind].Remove(key, hash, section.Next());
                if (cd == 0) return (0, this);

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
                                            Mask(Bit.Get((uint)EqK.GetHashCode(subEntries.Items[0]), section)),
                                            0,
                                            Clone(subEntries.Items),
                                            System.Array.Empty<Node>()
                                        ));
                            }
                            else
                            {
                                return (cd, 
                                        new Entries(
                                            Bit.Set(EntryMap, mask, true), 
                                            Bit.Set(NodeMap, mask, false),
                                            Insert(Items, Index(EntryMap, mask), subEntries.Items[0]),
                                            RemoveAt(Nodes, ind)));
                            }
                        }
                        else
                        {
                            var nodeCopy = Clone(Nodes);
                            nodeCopy[ind] = subNode;
                            return (cd, new Entries(EntryMap, NodeMap, Items, nodeCopy));
                        }

                    case Tag.Collision:
                        var nodeCopy2 = Clone(Nodes);
                        nodeCopy2[ind] = subNode;
                        return (cd, new Entries(EntryMap, NodeMap, Items, nodeCopy2));

                    default:
                        return (cd, this);
                }
            }
            else
            {
                return (0, this);
            }
        }

        public (bool Found, K Key) Read(K key, uint hash, Sec section)
        {                                                                                         
            // var hashIndex = Bit.Get(hash, section);
            // Mask(hashIndex)
            var mask = (uint)(1 << (int)((hash & (uint)(Sec.Mask << section.Offset)) >> section.Offset));

            // if(Bit.Get(EntryMap, mask))
            if ((EntryMap & mask) == mask)                                                        
            {
                // var entryIndex = Index(EntryMap, mask);
                var entryIndex = BitCount((int)EntryMap & (((int)mask) - 1));                     
                if (EqK.Equals(Items[entryIndex], key))
                {
                    var item = Items[entryIndex];
                    return (true, item);
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
                var entryIndex = BitCount((int)NodeMap & (((int)mask) - 1));                     
                return Nodes[entryIndex].Read(key, hash, section.Next());
            }
            else
            {
                return default;
            }
        }

        public (int CountDelta, Node Node) Update((UpdateType Type, bool Mutate) env, K change, uint hash, Sec section)
        {
            // var hashIndex = Bit.Get(hash, section);
            // var mask = Mask(hashIndex);
            var mask = (uint)(1 << (int)((hash & (uint)(Sec.Mask << section.Offset)) >> section.Offset));

            //if (Bit.Get(EntryMap, mask))
            if((EntryMap & mask) == mask)
            {
                //var entryIndex = Index(EntryMap, mask);
                var entryIndex   = BitCount((int)EntryMap & (((int)mask) - 1));
                var currentEntry = Items[entryIndex];

                if (EqK.Equals(currentEntry, change))
                {
                    if (env.Type == UpdateType.Add)
                    {
                        // Key already exists - so it's an error to add again
                        throw new ArgumentException($"Key already exists in map: {change}");
                    }
                    else if (env.Type == UpdateType.TryAdd)
                    {
                        // Already added, so we don't continue to try
                        return (0, this);
                    }

                    var newItems = SetItem(Items, entryIndex, change, env.Mutate);
                    return (0, new Entries(EntryMap, NodeMap, newItems, Nodes));
                }
                else
                {
                    if (env.Type == UpdateType.SetItem)
                    {
                        // Key must already exist to set it
                        throw new ArgumentException($"Key already exists in map: {change}");
                    }
                    else if (env.Type == UpdateType.TrySetItem)
                    {
                        // Key doesn't exist, so there's nothing to set
                        return (0, this);
                    }

                    // Add
                    var node = Merge(change, currentEntry, hash, (uint)EqK.GetHashCode(currentEntry), section);

                    //var newItems = Items.Filter(elem => !EqK.Equals(elem.Key, currentEntry.Key)).ToArray();
                    var newItems = new K[Items.Length - 1];
                    var i        = 0;
                    foreach(var elem in Items)
                    {
                        if(!EqK.Equals(elem, currentEntry))
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
                    var nodeIndex = BitCount((int)NodeMap & (((int)mask) - 1));

                    var newNodes = Insert(Nodes, nodeIndex, node);

                    return (1, new Entries(
                                newEntryMap, 
                                newNodeMap, 
                                newItems, 
                                newNodes));
                }
            }
            else if (Bit.Get(NodeMap, mask))
            {
                // var nodeIndex = Index(NodeMap, mask);
                var nodeIndex = BitCount((int)NodeMap & (((int)mask) - 1));

                var nodeToUpdate = Nodes[nodeIndex];
                var (cd, newNode) = nodeToUpdate.Update(env, change, hash, section.Next());
                var newNodes = SetItem(Nodes, nodeIndex, newNode, env.Mutate);
                return (cd, new Entries(EntryMap, NodeMap, Items, newNodes));
            }
            else
            {
                if (env.Type == UpdateType.SetItem)
                {
                    // Key must already exist to set it
                    throw new ArgumentException($"Key doesn't exist in map: {change}");
                }
                else if (env.Type == UpdateType.TrySetItem)
                {
                    // Key doesn't exist, so there's nothing to set
                    return (0, this);
                }

                // var entryIndex = Index(EntryMap, mask);
                var entryIndex = BitCount((int)EntryMap & (((int)mask) - 1));

                // var entries = Bit.Set(EntryMap, mask, true);
                var entries = EntryMap | mask;

                var newItems = Insert(Items, entryIndex, change);
                return (1, new Entries(entries, NodeMap, newItems, Nodes));
            }
        }

        public IEnumerator<K> GetEnumerator()
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
        public readonly K[] Items;
        public readonly uint Hash;

        public Tag Type => Tag.Collision;

        public Collision(K[] items, uint hash)
        {
            Items = items;
            Hash = hash;
        }

        public (bool Found, K Key) Read(K key, uint hash, Sec section)
        {
            foreach (var kv in Items)
            {
                if (EqK.Equals(kv, key))
                {
                    return (true, kv);
                }
            }
            return default;
        }

        public (int CountDelta, Node Node) Remove(K key, uint hash, Sec section)
        {
            var len = Items.Length;
            if (len      == 0) return (0, this);
            else if (len == 1) return (-1, EmptyNode.Default);
            else if (len == 2)
            {
                var (_, n) = EqK.Equals(Items[0], key)
                                 ? EmptyNode.Default.Update((UpdateType.Add, false), Items[1], hash, default)
                                 : EmptyNode.Default.Update((UpdateType.Add, false), Items[0], hash, default);

                return (-1, n);
            }
            else
            {
                static IEnumerable<K> Yield(K[] items, K ikey)
                {
                    foreach (var item in items)
                    {
                        if (!EqK.Equals(item, ikey))
                        {
                            yield return item;
                        }
                    }
                }

                var result = Yield(Items, key).ToArray();

                return (result.Length - Items.Length, new Collision(result, hash));
            }
        }

        public (int CountDelta, Node Node) Update((UpdateType Type, bool Mutate) env, K change, uint hash, Sec section)
        {
            var index = -1;
            for (var i = 0; i < Items.Length; i++)
            {
                if (EqK.Equals(Items[i], change))
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
                    throw new ArgumentException($"Key already exists in map: {change}");
                }
                else if (env.Type == UpdateType.TryAdd)
                {
                    // Already added, so we don't continue to try
                    return (0, this);
                }

                var newArr = SetItem(Items, index, change, false);
                return (0, new Collision(newArr, hash));
            }
            else
            {
                if (env.Type == UpdateType.SetItem)
                {
                    // Key must already exist to set it
                    throw new ArgumentException($"Key doesn't exist in map: {change}");
                }
                else if (env.Type == UpdateType.TrySetItem)
                {
                    // Key doesn't exist, so there's nothing to set
                    return (0, this);
                }

                var result = new K[Items.Length + 1];
                System.Array.Copy(Items, result, Items.Length);
                result[Items.Length] = change;
                return (1, new Collision(result, hash));
            }
        }

        public IEnumerator<K> GetEnumerator() =>
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

        public (bool Found, K Key) Read(K key, uint hash, Sec section) =>
            default;

        public (int CountDelta, Node Node) Remove(K key, uint hash, Sec section) =>
            (0, this);

        public (int CountDelta, Node Node) Update((UpdateType Type, bool Mutate) env, K change, uint hash, Sec section)
        {
            if (env.Type == UpdateType.SetItem)
            {
                // Key must already exist to set it
                throw new ArgumentException($"Key doesn't exist in map: {change}");
            }
            else if (env.Type == UpdateType.TrySetItem)
            {
                // Key doesn't exist, so there's nothing to set
                return (0, this);
            }

            var dataMap = Mask(Bit.Get(hash, section));
            return (1, new Entries(dataMap, 0, [change], System.Array.Empty<Node>()));
        }

        public IEnumerator<K> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield break;
        }
    }

    /// <summary>
    /// Merges two keys into a single Node
    /// </summary>
    static Node Merge(K key1, K key2, uint pair1Hash, uint pair2Hash, Sec section)
    {
        if (section.Offset >= 25)
        {
            return new Collision([key1, key2], pair1Hash);
        }
        else
        {
            var nextLevel  = section.Next();
            var pair1Index = Bit.Get(pair1Hash, nextLevel);
            var pair2Index = Bit.Get(pair2Hash, nextLevel);
            if (pair1Index == pair2Index)
            {
                var node    = Merge(key1, key2, pair1Hash, pair2Hash, nextLevel);
                var nodeMap = Mask(pair1Index);
                return new Entries(0, nodeMap, System.Array.Empty<K>(), new[] { node });
            }
            else
            {
                var dataMap = Mask(pair1Index);
                dataMap = Bit.Set(dataMap, Mask(pair2Index), true);
                return new Entries(dataMap, 0, pair1Index < pair2Index
                                                   ? new[] { key1, key2 }
                                                   : new[] { key2, key1 }, System.Array.Empty<Node>());
            }
        }
    }

    public Iterable<K> AsEnumerable() =>
        Root.AsIterable();

    public IEnumerator<K> GetEnumerator() =>
        Root.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        Root.GetEnumerator();

    

    /// <summary>
    /// Finds the number of 1-bits below the bit at `location`
    /// This function is used to find where in the array of entries or nodes 
    /// the item should be inserted
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Index(uint bitmap, uint location) =>
        BitCount((int)bitmap & ((int)location - 1));

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
    static A[] SetItem<A>(A[] items, int index, A value, bool mutate)
    {
        if (mutate)
        {
            items[index] = value;
            return items;
        }
        else
        {
            var nitems = new A[items.Length];
            System.Array.Copy(items, nitems, items.Length);
            nitems[index] = value;
            return nitems;
        }
    }

    /// <summary>
    /// Clones an existing array
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static A[] Clone<A>(A[] items)
    {
        var len    = items.Length;
        var nitems = new A[len];
        System.Array.Copy(items, nitems, len);
        return nitems;
    }

    /// <summary>
    /// Inserts a new item in the array (immutably)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static A[] Insert<A>(A[] array, int index, A value)
    {
        var newArray = new A[array.Length + 1];
        System.Array.Copy(array, 0, newArray, 0, index);
        System.Array.Copy(array, index, newArray, index + 1, array.Length - index);
        newArray[index] = value;
        return newArray;
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
            System.Array.Copy(array, 0, result, 0, index);
        }
        if (index + 1 < array.Length)
        {
            System.Array.Copy(array, index + 1, result, index, array.Length - index - 1);
        }
        return result;
    }

    public override string ToString() =>
        count < 50
            ? $"[{ string.Join(", ", AsEnumerable()) }]"
            : $"[{ string.Join(", ", AsEnumerable().Take(50)) } ... ]";

    public bool TryGetValue(K key, out K value)
    {
        var ov = Find(key);
        if (ov.IsSome)
        {
            value = (K)ov;
            return true;
        }
        else
        {
            value = default!;
            return false;
        }
    }

    IEnumerator<K> IEnumerable<K>.GetEnumerator() =>
        AsEnumerable().GetEnumerator();
}
