using static LanguageExt.Prelude;
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
internal sealed class TrieSet<K> :
    IEquatable<TrieSet<K>>,
    IReadOnlyCollection<K>
{
    public static TrieSet<K> Empty(IEqualityComparer<K>? equalityComparer = null) => 
        new (equalityComparer ?? getRegisteredEqualityComparerOrDefault<K>(), EmptyNode.Default, 0);

    readonly Node Root;
    readonly int _count;
    private readonly IEqualityComparer<K> _equalityComparer;
    int hash;

    /// <summary>
    /// Ctor
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    TrieSet(IEqualityComparer<K> equalityComparer, Node root, int count)
    {
        Root = root;
        _count = count;
        _equalityComparer = equalityComparer;
    }

    public TrieSet(ReadOnlySpan<K> items, bool tryAdd = true, IEqualityComparer<K>? equalityComparer = null)
    {
        Root = EmptyNode.Default;
        _equalityComparer = equalityComparer ?? getRegisteredEqualityComparerOrDefault<K>();
        var env = new UpdateContext( tryAdd ? UpdateType.TryAdd : UpdateType.AddOrUpdate, true);
        foreach (K item in items)
        {
            var hash    = (uint)_equalityComparer.GetHashCode(item!);
            Sec section = default;
            var changed = Root.Update(env, item, hash, section, _equalityComparer);
            if (changed)
            {
                _count += changed.Value.CountDelta;
                Root = changed.Value.Node;
            }
            
        }
    }

    public TrieSet(IEnumerable<K> items, bool tryAdd = true, IEqualityComparer<K>? equalityComparer = null)
    {
        Root = EmptyNode.Default;
        _equalityComparer = equalityComparer ?? getRegisteredEqualityComparerOrDefault<K>();
        var env = new UpdateContext( tryAdd ? UpdateType.TryAdd : UpdateType.AddOrUpdate, true);
        foreach (K item in items)
        {
            var hash    = (uint)_equalityComparer.GetHashCode(item!);
            Sec section = default;
            var changed = Root.Update(env, item, hash, section, _equalityComparer);
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
    public TrieSet<K> Add(K key) =>
        Update(key, new(UpdateType.Add, false));

    /// <summary>
    /// Try to add an item to the map.  If it already exists, do
    /// nothing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<K> TryAdd(K key) =>
        Update(key, new(UpdateType.TryAdd, false));

    /// <summary>
    /// Add an item to the map, if it exists update the value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<K> AddOrUpdate(K key) =>
        Update(key, new(UpdateType.AddOrUpdate, false));

    /// <summary>
    /// Add a range of values to the map
    /// If any items already exist an exception will be thrown
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<K> AddRange(IEnumerable<K> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.Add, false);
        foreach (var item in items)
        {
            self = self.Update(item, env);
        }
        return self;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<K> TryAddRange(IEnumerable<K> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.TryAdd, false);
        foreach (var item in items)
        {
            self = self.Update(item, env);
        }
        return self;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<K> AddOrUpdateRange(IEnumerable<K> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.AddOrUpdate, false);
        foreach (var item in items)
        {
            self = self.Update(item, env);
        }
        return self;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<K> SetItems(IEnumerable<K> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.SetItem, false);
        foreach (var item in items)
        {
            self = self.Update(item, env);
        }
        return self;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<K> TrySetItems(IEnumerable<K> items)
    {
        var self = this;
        var env = new UpdateContext(UpdateType.TrySetItem, false);
        foreach (var item in items)
        {
            self = self.Update(item, env);
        }
        return self;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<K> RemoveRange(IEnumerable<K> items)
    {
        var self = this;
        foreach (var item in items)
        {
            self = self.Remove(item!);
        }
        return self;
    }

    /// <summary>
    /// Set an item that already exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<K> SetItem(K key) =>
        Update(key, new(UpdateType.SetItem, false));

    /// <summary>
    /// Try to set an item that already exists in the map.  If none
    /// exists, do nothing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<K> TrySetItem(K key) =>
        Update(key, new(UpdateType.TrySetItem, false));

    /// <summary>
    /// Remove an item from the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<K> Remove(K key)
    {
        var hash    = (uint)_equalityComparer.GetHashCodeOrDefault(key);
        Sec section = default;
        var changed = Root.Remove(key, hash, section, _equalityComparer);
        return changed
                   ? new TrieSet<K>(_equalityComparer, changed.Value.Node, _count + changed.Value.CountDelta)
                   : this;
    }

    /// <summary>
    /// Indexer
    /// </summary>
    public K this[K key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => FindInternal(key).IfNone(() => throw new ArgumentException($"Key doesn't exist in map: {key}"));
    }

    /// <summary>
    /// Get the hash code of the items in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() =>
        hash == 0
            ? (hash = FNV32.Hash(_equalityComparer, AsEnumerable()))
            : hash;

    /// <summary>
    /// Returns the whether the `key` exists in the map
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(K key) =>
        FindInternal(key).IsSome;

    /// <summary>
    /// Returns the value associated with `key`.  Or None, if no key exists
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<K> Find(K key) => 
        FindInternal(key);

    /// <summary>
    /// Returns the value associated with `key`.  Or None, if no key exists
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Option<K> FindInternal(K key)
    {
        var hash    = (uint)_equalityComparer.GetHashCodeOrDefault(key);
        Sec section = default;
        return Root.Read(key, hash, section, _equalityComparer);
    }

    /// <summary>
    /// Associative union
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<K> Append(TrieSet<K> rhs) =>
        TryAddRange(rhs.AsEnumerable());

    /// <summary>
    /// Subtract
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TrieSet<K> Subtract(TrieSet<K> rhs)
    {
        var lhs = this;
        foreach (var item in rhs)
        {
            lhs = lhs.Remove(item!);
        }
        return lhs;
    }

    /// <summary>
    /// Equality
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(TrieSet<K> lhs, TrieSet<K> rhs) =>
        lhs.Equals(rhs);

    /// <summary>
    /// Non equality
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(TrieSet<K> lhs, TrieSet<K> rhs) =>
        !(lhs == rhs);

    /// <summary>
    /// Equality
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? rhs) =>
        rhs is TrieSet<K> map && Equals(map);

    /// <summary>
    /// Equality
    /// </summary>
    public bool Equals(TrieSet<K>? rhs) => 
        this.collectionEquals(rhs, _equalityComparer, true);

    /// <summary>
    /// Update an item in the map - can mutate if needed
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    TrieSet<K> Update(K key, UpdateContext env)
    {
        var hash    = (uint)_equalityComparer.GetHashCodeOrDefault(key);
        Sec section = default;
        var changed = Root.Update(env, key, hash, section, _equalityComparer);
        return changed
                   ? new TrieSet<K>(_equalityComparer, changed.Value.Node, _count + changed.Value.CountDelta)
                   : this;
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
            if (ContainsKey(item!))
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
            if (!ContainsKey(item!))
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
            if (ContainsKey(item!))
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
            if (!ContainsKey(item!))
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
            if (ContainsKey(item!))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    public TrieSet<K> Intersect(IEnumerable<K> other)
    {
        var res = new List<K>();
        foreach (var item in other)
        {
            Find(item!).Do(res.Add);
        }
        return new TrieSet<K>(res, equalityComparer: _equalityComparer);
    }

    /// <summary>
    /// Returns this - other.  Only the items in this that are not in 
    /// other will be returned.
    /// </summary>
    public TrieSet<K> Except(IEnumerable<K> other)
    {
        var self = this;
        foreach (var item in other)
        {
            self = self.Remove(item!);
        }
        return self;
    }

    /// <summary>
    /// Only items that are in one set or the other will be returned.
    /// If an item is in both, it is dropped.
    /// </summary>
    public TrieSet<K> SymmetricExcept(IEnumerable<K> rhs) =>
        SymmetricExcept(new TrieSet<K>(rhs, true));

    /// <summary>
    /// Only items that are in one set or the other will be returned.
    /// If an item is in both, it is dropped.
    /// </summary>
    public TrieSet<K> SymmetricExcept(TrieSet<K> rhs)
    {
        var res = new List<K>();

        foreach (var item in this)
        {
            if (!rhs.ContainsKey(item!))
            {
                res.Add(item);
            }
        }

        foreach (var item in rhs)
        {
            if (!ContainsKey(item!))
            {
                res.Add(item);
            }
        }

        return new TrieSet<K>(res, equalityComparer: _equalityComparer);
    }

    /// <summary>
    /// Nodes in the CHAMP hash trie map can be in one of three states:
    /// 
    ///     Empty - nothing in the map
    ///     Entries - contains items and sub-nodes
    ///     Collision - keeps track of items that have different keys but the same hash
    /// 
    /// </summary>
    internal abstract class  Node : IEnumerable<K>
    {
        public abstract TrieTag Type { get; }
        public abstract Option<K> Read(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer);
        public abstract Option<(int CountDelta, Node Node)> Update(UpdateContext env, K change, uint hash, Sec section, IEqualityComparer<K> equalityComparer);
        public abstract Option<(int CountDelta, Node Node)> Remove(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer);
        public abstract IEnumerator<K> GetEnumerator();
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
        public readonly K[] Items;
        public readonly Node[] Nodes;

        public override TrieTag Type => TrieTag.Entries;

        public Entries(uint entryMap, uint nodeMap, K[] items, Node[] nodes)
        {
            EntryMap = entryMap;
            NodeMap = nodeMap;
            Items = items;
            Nodes = nodes;
        }

        public override Option<(int CountDelta, Node Node)> Remove(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer)
        {
            var hashIndex = Bit.Get(hash, section);
            var mask      = Bit.Mask(hashIndex);

            if (Bit.Get(EntryMap, mask))
            {
                // If key belongs to an entry
                var ind = Bit.Index(EntryMap, mask);
                if (equalityComparer.Equals(Items[ind], key))
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
                    return default;
                }
            }
            else if (Bit.Get(NodeMap, mask))
            {
                //If key lies in a sub-node
                var ind = Bit.Index(NodeMap, mask);
                var removed = Nodes[ind].Remove(key, hash, section.Next(), equalityComparer);
                if (removed)
                {
                    var (cd, subNode) = removed.Value;

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
                                                Bit.Mask(Bit.Get((uint)equalityComparer.GetHashCode(subEntries.Items[0]!), section)),
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
                                                Insert(Items, Bit.Index(EntryMap, mask), subEntries.Items[0]),
                                                RemoveAt(Nodes, ind)));
                                }
                            }
                            else
                            {
                                var nodeCopy = Clone(Nodes);
                                nodeCopy[ind] = subNode;
                                return (cd, new Entries(EntryMap, NodeMap, Items, nodeCopy));
                            }

                        case TrieTag.Collision:
                            var nodeCopy2 = Clone(Nodes);
                            nodeCopy2[ind] = subNode;
                            return (cd, new Entries(EntryMap, NodeMap, Items, nodeCopy2));
                    }
                }
            }
            
            return default;
            
        }

        public override Option<K> Read(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer)
        {                                                                                         
            var hashIndex = Bit.Get(hash, section);
            var mask = Bit.Mask(hashIndex);

            if(Bit.Get(EntryMap, mask))
            {
                var entryIndex = Bit.Index(EntryMap, mask);
                if (equalityComparer.Equals(Items[entryIndex], key))
                {
                    var item = Items[entryIndex];
                    return item;
                }
                
                return default;
            }
            else if (Bit.Get(NodeMap, mask))
            {
                var entryIndex = Bit.Index(NodeMap, mask);
                return Nodes[entryIndex].Read(key, hash, section.Next(), equalityComparer);
            }
            else
            {
                return default;
            }
        }

        public override Option<(int CountDelta, Node Node)> Update(UpdateContext env, K change, uint hash, Sec section, IEqualityComparer<K> equalityComparer)
        {
            var hashIndex = Bit.Get(hash, section);
            var mask = Bit.Mask(hashIndex);
            
            if (Bit.Get(EntryMap, mask))
            {
                var entryIndex = Bit.Index(EntryMap, mask);
                K currentEntry = Items[entryIndex];

                if (equalityComparer.Equals(currentEntry, change))
                {
                    if (env.Type == UpdateType.Add)
                    {
                        // Key already exists - so it's an error to add again
                        throw new ArgumentException($"Key already exists in map: {change}");
                    }
                    else if (env.Type == UpdateType.TryAdd)
                    {
                        // Already added, so we don't continue to try
                        return default;
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
                        return default;
                    }

                    // Add
                    var node = Merge(change, currentEntry, hash, (uint)equalityComparer.GetHashCode(currentEntry!), section);

                    //var newItems = Items.Filter(elem => !EqK.Equals(elem.Key, currentEntry.Key)).ToArray();
                    var newItems = new K[Items.Length - 1];
                    var i        = 0;
                    foreach(var elem in Items)
                    {
                        if(!equalityComparer.Equals(elem, currentEntry))
                        {
                            newItems[i] = elem;
                            i++;
                        }
                    }

                    var newEntryMap = Bit.Set(EntryMap, mask, false);
                    var newNodeMap = Bit.Set(NodeMap, mask, true);
                    var nodeIndex = Bit.Index(NodeMap, mask);
                    
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
                var nodeIndex = Bit.Index(NodeMap, mask);
                
                var nodeToUpdate = Nodes[nodeIndex];
                var changed = nodeToUpdate.Update(env, change, hash, section.Next(), equalityComparer);
                if(changed)
                {
                    var newNodes = SetItem(Nodes, nodeIndex, changed.Value.Node, env.Mutate);
                    return (changed.Value.CountDelta, new Entries(EntryMap, NodeMap, Items, newNodes));
                }
                return default;
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
                    return default;
                }

                var entryIndex = Bit.Index(EntryMap, mask);
                var entries = Bit.Set(EntryMap, mask, true);
                
                var newItems = Insert(Items, entryIndex, change);
                return (1, new Entries(entries, NodeMap, newItems, Nodes));
            }
        }

        public override IEnumerator<K> GetEnumerator()
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
        public readonly K[] Items;
        public readonly uint Hash;

        public override TrieTag Type => TrieTag.Collision;

        public Collision(K[] items, uint hash)
        {
            Items = items;
            Hash = hash;
        }

        public override Option<K> Read(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer)
        {
            foreach (var kv in Items)
            {
                if (equalityComparer.Equals(kv, key))
                {
                    return kv;
                }
            }
            return default;
        }

        public override Option<(int CountDelta, Node Node)> Remove(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer)
        {
            var len = Items.Length;
            if (len      == 0) return default;
            else if (len == 1 && equalityComparer.Equals(Items[0], key)) return (-1, EmptyNode.Default);
            else if (len == 2 && equalityComparer.Equals(Items[0], key))
            {
                var env = new UpdateContext(UpdateType.Add, false);
                var (_, n) = EmptyNode.Default.Update(env, Items[1], hash, default, equalityComparer).Value;

                return (-1, n);
            }
            else if (len == 2 && equalityComparer.Equals(Items[1], key))
            {
                var env = new UpdateContext(UpdateType.Add, false);
                var (_, n) = EmptyNode.Default.Update(env, Items[0], hash, default, equalityComparer).Value;

                return (-1, n);
            }

            var (left, right) = Items.Partition(item => equalityComparer.Equals(item, key), 1);

            if (left.Length > 0)
            {
                Node collision = new Collision(right, hash);
                return (right.Length - Items.Length, collision);
            }

            return default;
        }

        public override Option<(int CountDelta, Node Node)> Update(UpdateContext env, K change, uint hash, Sec section, IEqualityComparer<K> equalityComparer)
        {
            var index = -1;
            for (var i = 0; i < Items.Length; i++)
            {
                if (equalityComparer.Equals(Items[i], change))
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
                    return default;
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
                    return default;
                }

                var result = new K[Items.Length + 1];
                System.Array.Copy(Items, result, Items.Length);
                result[Items.Length] = change;
                return (1, new Collision(result, hash));
            }
        }

        public override IEnumerator<K> GetEnumerator() =>
            Items.AsEnumerable().GetEnumerator();
    }

    /// <summary>
    /// Empty node
    /// </summary>
    internal sealed class EmptyNode : Node
    {
        public static readonly EmptyNode Default = new EmptyNode();

        public override TrieTag Type => TrieTag.Empty;

        public override Option<K> Read(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer) =>
            default;

        public override Option<(int CountDelta, Node Node)> Remove(K key, uint hash, Sec section, IEqualityComparer<K> equalityComparer) =>
            default;

        public override Option<(int CountDelta, Node Node)> Update(UpdateContext env, K change, uint hash, Sec section, IEqualityComparer<K> equalityComparer)
        {
            if (env.Type == UpdateType.SetItem)
            {
                // Key must already exist to set it
                throw new ArgumentException($"Key doesn't exist in map: {change}");
            }
            else if (env.Type == UpdateType.TrySetItem)
            {
                // Key doesn't exist, so there's nothing to set
                return default;
            }

            var dataMap = Bit.Mask(Bit.Get(hash, section));
            return (1, new Entries(dataMap, 0, [change], System.Array.Empty<Node>()));
        }

        public override IEnumerator<K> GetEnumerator()
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
                var nodeMap = Bit.Mask(pair1Index);
                return new Entries(0, nodeMap, System.Array.Empty<K>(), new[] { node });
            }
            else
            {
                var dataMap = Bit.Mask(pair1Index);
                dataMap = Bit.Set(dataMap, Bit.Mask(pair2Index), true);
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
            var newItems = new A[items.Length];
            System.Array.Copy(items, newItems, items.Length);
            newItems[index] = value;
            return newItems;
        }
    }

    /// <summary>
    /// Clones an existing array
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static A[] Clone<A>(A[] items)
    {
        var len    = items.Length;
        var newItems = new A[len];
        System.Array.Copy(items, newItems, len);
        return newItems;
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
        _count < 50
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

    public bool HasSameEqualityComparer(IEqualityComparer<K> equalityComparer) => 
        _equalityComparer == equalityComparer;

    IEnumerator<K> IEnumerable<K>.GetEnumerator() =>
        AsEnumerable().GetEnumerator();
    internal TrieSet<K> Clear() => 
        Empty(_equalityComparer);
}
