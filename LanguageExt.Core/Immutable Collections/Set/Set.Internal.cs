using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using static LanguageExt.Prelude;
using System.Diagnostics.Contracts;
using LanguageExt.Traits;

namespace LanguageExt;

/// <summary>
/// Immutable set
/// AVL tree implementation
/// AVL tree is a self-balancing binary search tree. 
/// [wikipedia.org/wiki/AVL_tree](http://en.wikipedia.org/wiki/AVL_tree)
/// </summary>
/// <typeparam name="A">List item type</typeparam>
[Serializable]
internal sealed class SetInternal<A> :
    IReadOnlyCollection<A>,
    IEquatable<SetInternal<A>>
{
    public static SetInternal<A> Empty => new (null);
    readonly SetItem<A> set;
    private readonly IComparer<A> _comparer;
    int hashCode;

    /// <summary>
    /// Default ctor
    /// </summary>
    internal SetInternal(IComparer<A>? comparer)
    {
        set = SetItem<A>.Empty;
        _comparer = comparer ?? getRegisteredOrderComparerOrDefault<A>();
    }

    /// <summary>
    /// Ctor that takes a root element
    /// </summary>
    /// <param name="root"></param>
    /// <param name="comparer">Item comparer</param>
    internal SetInternal(SetItem<A> root, IComparer<A>? comparer)
    {
        set = root;
        _comparer = comparer ?? getRegisteredOrderComparerOrDefault<A>();
    }

    /// <summary>
    /// Ctor from an enumerable 
    /// </summary>
    public SetInternal(IEnumerable<A> items, IComparer<A>? comparer) :
        this(items, SetModule.AddOpt.TryAdd, comparer)
    {
    }


    /// <summary>
    /// Ctor that takes an initial (distinct) set of items
    /// </summary>
    /// <param name="items">Data source</param>
    /// <param name="option">Duplicate strategy</param>
    /// <param name="comparer">Item comparer</param>
    internal SetInternal(IEnumerable<A> items, SetModule.AddOpt option, IComparer<A>? comparer)
    {
        set = SetItem<A>.Empty;
        _comparer = comparer ?? getRegisteredOrderComparerOrDefault<A>();
        var addMethod = Translate(option);

        foreach (var item in items)
        {
            set = addMethod(set, item, _comparer);
        }
    }

    /// <summary>
    /// Ctor that takes an initial (distinct) set of items
    /// </summary>
    /// <param name="items">Data source</param>
    /// <param name="option">Duplicate strategy</param>
    /// <param name="comparer">Item comparer</param>
    internal SetInternal(ReadOnlySpan<A> items, SetModule.AddOpt option, IComparer<A>? comparer)
    {
        set = SetItem<A>.Empty;
        _comparer = comparer ?? getRegisteredOrderComparerOrDefault<A>();

        var addMethod = Translate(option);

        foreach (var item in items)
        {
            set = addMethod(set, item, _comparer);
        }
    }

    private static Func<SetItem<A>, A, IComparer<A>, SetItem<A>> Translate(SetModule.AddOpt option){
        Func<SetItem<A>, A, IComparer<A>, SetItem<A>> addMethod = option switch
        {
            SetModule.AddOpt.ThrowOnDuplicate => SetModule.Add,
            SetModule.AddOpt.TryUpdate => SetModule.AddOrUpdate,
            SetModule.AddOpt.TryAdd => SetModule.TryAdd,
            _ => throw new NotSupportedException() 
        };
        return addMethod;
    }

    private SetInternal<A> Wrap(SetItem<A> root) => new(root, _comparer);

    [Pure]
    public bool HasSameComparer(IComparer<A> comparer) => 
        ReferenceEquals(_comparer, comparer);

    public override int GetHashCode() =>
        hashCode == 0
            ? hashCode = FNV32.Hash(getRegisteredEqualityComparerOrDefault<A>(), AsIterable())
            : hashCode;

    public Iterable<A> AsIterable()
    {
        IEnumerable<A> Yield()
        {
            using var iter = GetEnumerator();
            while (iter.MoveNext())
            {
                yield return iter.Current;
            }
        }
        return Iterable.createRange(Yield());
    }

    public Iterable<A> Skip(int amount)
    {
        return Iterable.createRange(Go());
        IEnumerable<A> Go()
        {
            using var iter = new SetModule.SetEnumerator<A>(set, false, amount);
            while (iter.MoveNext())
            {
                yield return iter.Current;
            }
        }
    }

    /// <summary>
    /// Number of items in the set
    /// </summary>
    [Pure]
    public int Count =>
        set.Count;

    [Pure]
    public Option<A> Min => 
        set.IsEmpty
            ? None
            : SetModule.Min(set);

    [Pure]
    public Option<A> Max =>
        set.IsEmpty
            ? None
            : SetModule.Max(set);

    /// <summary>
    /// Add an item to the set
    /// </summary>
    /// <param name="value">Value to add to the set</param>
    /// <returns>New set with the item added</returns>
    [Pure]
    public SetInternal<A> Add(A value) =>
        new (SetModule.Add(set,value, _comparer), _comparer);

    /// <summary>
    /// Attempt to add an item to the set.  If an item already
    /// exists then return the Set as-is.
    /// </summary>
    /// <param name="value">Value to add to the set</param>
    /// <returns>New set with the item maybe added</returns>
    [Pure]
    public SetInternal<A> TryAdd(A value) =>
        Contains(value)
            ? this
            : Add(value);

    /// <summary>
    /// Add an item to the set.  If an item already
    /// exists then replace it.
    /// </summary>
    /// <param name="value">Value to add to the set</param>
    /// <returns>New set with the item maybe added</returns>
    [Pure]
    public SetInternal<A> AddOrUpdate(A value) =>
        Wrap(SetModule.AddOrUpdate(set, value, _comparer));

    [Pure]
    public SetInternal<A> AddRange(IEnumerable<A> xs)
    {
        if(Count == 0)
        {
            return new SetInternal<A>(xs, SetModule.AddOpt.ThrowOnDuplicate, _comparer);
        }

        var set = this;
        foreach(var x in xs)
        {
            set = set.Add(x);
        }
        return set;
    }

    [Pure]
    public SetInternal<A> TryAddRange(IEnumerable<A> xs)
    {
        if (Count == 0)
        {
            return new SetInternal<A>(xs, SetModule.AddOpt.TryAdd, _comparer);
        }

        var set = this;
        foreach (var x in xs)
        {
            set = set.TryAdd(x);
        }
        return set;
    }

    [Pure]
    public SetInternal<A> AddOrUpdateRange(IEnumerable<A> xs)
    {
        if (Count == 0)
        {
            return new SetInternal<A>(xs, SetModule.AddOpt.TryUpdate, _comparer);
        }

        var set = this;
        foreach (var x in xs)
        {
            set = set.AddOrUpdate(x);
        }
        return set;
    }

    /// <summary>
    /// Attempts to find an item in the set.  
    /// </summary>
    /// <param name="value">Value to find</param>
    /// <returns>Some(T) if found, None otherwise</returns>
    [Pure]
    public Option<A> Find(A value) =>
        SetModule.TryFind(set, value, _comparer);

    /// <summary>
    /// Retrieve the value from predecessor item to specified key
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <returns>Found key</returns>
    [Pure]
    public Option<A> FindPredecessor(A key) => SetModule.TryFindPredecessor<A>(set, key, _comparer);

    /// <summary>
    /// Retrieve the value from exact key, or if not found, the predecessor item 
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <returns>Found key</returns>
    [Pure]
    public Option<A> FindOrPredecessor(A key) => SetModule.TryFindOrPredecessor<A>(set, key, _comparer);

    /// <summary>
    /// Retrieve the value from next item to specified key
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <returns>Found key</returns>
    [Pure]
    public Option<A> FindSuccessor(A key) => SetModule.TryFindSuccessor<A>(set, key, _comparer);

    /// <summary>
    /// Retrieve the value from exact key, or if not found, the next item 
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <returns>Found key</returns>
    [Pure]
    public Option<A> FindOrSuccessor(A key) => SetModule.TryFindOrSuccessor<A>(set, key, _comparer);

    /// <summary>
    /// Retrieve a range of values 
    /// </summary>
    /// <param name="keyFrom">Range start (inclusive)</param>
    /// <param name="keyTo">Range to (inclusive)</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException the keyFrom or keyTo are null</exception>
    /// <returns>Range of values</returns>
    [Pure]
    public Iterable<A> FindRange(A keyFrom, A keyTo)
    {
        ArgumentNullException.ThrowIfNull(keyFrom);
        ArgumentNullException.ThrowIfNull(keyTo);
        return _comparer.Compare(keyFrom, keyTo) > 0
                   ? SetModule.FindRange(set, keyTo, keyFrom, _comparer).AsIterable()
                   : SetModule.FindRange(set, keyFrom, keyTo, _comparer).AsIterable();
    }


    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    [Pure]
    public SetInternal<A> Intersect(IEnumerable<A> other)
    {
        var root = SetItem<A>.Empty;
        foreach (var item in other)
        {
            if (Contains(item))
            {
                root = SetModule.TryAdd(root, item, _comparer);
            }
        }
        return Wrap(root);
    }

    /// <summary>
    /// Returns this - other.  Only the items in this that are not in 
    /// other will be returned.
    /// </summary>
    [Pure]
    public SetInternal<A> Except(SetInternal<A> rhs)
    {
        var root = SetItem<A>.Empty;
        foreach (var item in this)
        {
            if (!rhs.Contains(item))
            {
                root = SetModule.TryAdd(root, item, _comparer);
            }
        }
        return Wrap(root);
    }

    /// <summary>
    /// Returns this - other.  Only the items in this that are not in 
    /// other will be returned.
    /// </summary>
    [Pure]
    public SetInternal<A> Except(IEnumerable<A> other) =>
        Except(new SetInternal<A>(other, _comparer));

    /// <summary>
    /// Only items that are in one set or the other will be returned.
    /// If an item is in both, it is dropped.
    /// </summary>
    [Pure]
    public SetInternal<A> SymmetricExcept(SetInternal<A> rhs)
    {
        var root = SetItem<A>.Empty;

        foreach (var item in this)
        {
            if (!rhs.Contains(item))
            {
                root = SetModule.TryAdd<A>(root, item, _comparer);
            }
        }

        foreach (var item in rhs)
        {
            if (!Contains(item))
            {
                root = SetModule.TryAdd<A>(root, item, _comparer);
            }
        }

        return Wrap(root);
    }

    /// <summary>
    /// Only items that are in one set or the other will be returned.
    /// If an item is in both, it is dropped.
    /// </summary>
    [Pure]
    public SetInternal<A> SymmetricExcept(IEnumerable<A> other) =>
        SymmetricExcept(new SetInternal<A>(other, _comparer));

    /// <summary>
    /// Finds the union of two sets and produces a new set with 
    /// the results
    /// </summary>
    /// <param name="other">Other set to union with</param>
    /// <returns>A set which contains all items from both sets</returns>
    [Pure]
    public SetInternal<A> Union(IEnumerable<A> other)
    {
        var root = SetItem<A>.Empty;

        foreach(var item in this)
        {
            root = SetModule.TryAdd(root, item, _comparer);
        }

        foreach (var item in other)
        {
            root = SetModule.TryAdd(root, item, _comparer);
        }

        return Wrap(root);
    }

    /// <summary>
    /// Get enumerator
    /// </summary>
    /// <returns>IEnumerator T</returns>
    [Pure]
    public IEnumerator<A> GetEnumerator() =>
        new SetModule.SetEnumerator<A>(set, false, 0);

    /// <summary>
    /// Removes an item from the set (if it exists)
    /// </summary>
    /// <param name="value">Value to check</param>
    /// <returns>New set with item removed</returns>
    [Pure]
    public SetInternal<A> Remove(A value) =>
        Wrap(SetModule.Remove(set, value, _comparer));

    /// <summary>
    /// Maps the values of this set into a new set of values using the
    /// mapper function to transform the source values.
    /// </summary>
    /// <typeparam name="B">Mapped element type</typeparam>
    /// <param name="f">Mapping function</param>
    /// <param name="comparer">Optional item comparer</param>
    /// <returns>Mapped Set</returns>
    [Pure]
    public SetInternal<B> Map<B>(Func<A, B> f, IComparer<B>? comparer)  =>
        new (AsIterable().Map(f), comparer);

    /// <summary>
    /// Maps the values of this set into a new set of values using the
    /// mapper function to transform the source values.
    /// </summary>
    /// <param name="f">Mapping function</param>
    /// <returns>Mapped Set</returns>
    [Pure]
    public SetInternal<A> Map(Func<A, A> f) =>
        new (AsIterable().Map(f), null);

    /// <summary>
    /// Filters items from the set using the predicate.  If the predicate
    /// returns True for any item then it remains in the set, otherwise
    /// it's dropped.
    /// </summary>
    /// <param name="pred">Predicate</param>
    /// <returns>Filtered enumerable</returns>
    [Pure]
    public SetInternal<A> Filter(Func<A, bool> pred) =>
        new (AsIterable().Filter(pred), SetModule.AddOpt.TryAdd, _comparer);

    /// <summary>
    /// Check the existence of an item in the set using a 
    /// predicate.
    /// </summary>
    /// <remarks>Note this scans the entire set.</remarks>
    /// <param name="pred">Predicate</param>
    /// <returns>True if predicate returns true for any item</returns>
    [Pure]
    public bool Exists(Func<A, bool> pred) =>
        SetModule.Exists(set, pred);

    /// <summary>
    /// Returns True if the value is in the set
    /// </summary>
    /// <param name="value">Value to check</param>
    /// <returns>True if the item 'value' is in the Set 'set'</returns>
    [Pure]
    public bool Contains(A value) =>
        SetModule.Contains(set, value, _comparer);

    /// <summary>
    /// Returns true if both sets contain the same elements
    /// </summary>
    /// <param name="other">Other distinct set to compare</param>
    /// <returns>True if the sets are equal</returns>
    [Pure]
    public bool SetEquals(IEnumerable<A> other)
    {
        var rhs = new SetInternal<A>(other, _comparer);
        if (rhs.Count != Count) return false;
        foreach (var item in rhs)
        {
            if (!Contains(item)) return false;
        }
        return true;
    }

    /// <summary>
    /// True if the set has no elements
    /// </summary>
    [Pure]
    public bool IsEmpty => 
        Count == 0;

    /// <summary>
    /// Returns True if 'other' is a proper subset of this set
    /// </summary>
    /// <returns>True if 'other' is a proper subset of this set</returns>
    [Pure]
    public bool IsProperSubsetOf(IEnumerable<A> other)
    {
        if (IsEmpty)
        {
            return other.Any();
        }

        var otherSet = new Set<A>(other);
        if (Count >= otherSet.Count)
        {
            return false;
        }

        int  matches    = 0;
        bool extraFound = false;
        foreach (A item in otherSet)
        {
            if (Contains(item))
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
    [Pure]
    public bool IsProperSupersetOf(IEnumerable<A> other)
    {
        if (IsEmpty)
        {
            return false;
        }

        int matchCount = 0;
        foreach (A item in other)
        {
            matchCount++;
            if (!Contains(item))
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
    [Pure]
    public bool IsSubsetOf(IEnumerable<A> other)
    {
        if (IsEmpty)
        {
            return true;
        }

        var otherSet = new SetInternal<A>(other, _comparer);
        int matches  = 0;
        foreach (A item in otherSet)
        {
            if (Contains(item))
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
    [Pure]
    public bool IsSupersetOf(IEnumerable<A> other)
    {
        foreach (A item in other)
        {
            if (!Contains(item))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns True if other overlaps this set
    /// </summary>
    /// <param name="other">Other collection to check</param>
    /// <returns>True if other overlaps this set</returns>
    [Pure]
    public bool Overlaps(IEnumerable<A> other)
    {
        if (IsEmpty)
        {
            return false;
        }

        foreach (A item in other)
        {
            if (Contains(item))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Copy the items from the set into the specified array
    /// </summary>
    /// <param name="array">Array to copy to</param>
    /// <param name="index">Index into the array to start</param>
    public void CopyTo(A[] array, int index)
    {
        ArgumentNullException.ThrowIfNull(array);
        if (index < 0 || index + Count > array.Length) throw new ArgumentOutOfRangeException(nameof(index));
        
        foreach (var element in this)
        {
            array[index++] = element;
        }
    }

    /// <summary>
    /// Copy the items from the set into the specified array
    /// </summary>
    /// <param name="array">Array to copy to</param>
    /// <param name="index">Index into the array to start</param>
    public void CopyTo(Array array, int index)
    {
        ArgumentNullException.ThrowIfNull(array);
        if (index < 0 || index + Count > array.Length) throw new ArgumentOutOfRangeException(nameof(index));
        
        foreach (var element in this)
        {
            array.SetValue(element, index++);
        }
    }

    /// <summary>
    /// Add operator + performs a union of the two sets
    /// </summary>
    /// <param name="lhs">Left hand side set</param>
    /// <param name="rhs">Right hand side set</param>
    /// <returns>Unioned set</returns>
    [Pure]
    public static SetInternal<A> operator +(SetInternal<A> lhs, SetInternal<A> rhs) =>
        lhs.Append(rhs);

    /// <summary>
    /// Append performs a union of the two sets
    /// </summary>
    /// <param name="rhs">Right hand side set</param>
    /// <returns>Unioned set</returns>
    [Pure]
    public SetInternal<A> Append(SetInternal<A> rhs) =>
        Union(rhs.AsIterable());

    /// <summary>
    /// Subtract operator - performs a subtract of the two sets
    /// </summary>
    /// <param name="lhs">Left hand side set</param>
    /// <param name="rhs">Right hand side set</param>
    /// <returns>Subtracted set</returns>
    [Pure]
    public static SetInternal<A> operator -(SetInternal<A> lhs, SetInternal<A> rhs) =>
        lhs.Subtract(rhs);

    /// <summary>
    /// Subtract operator - performs a subtract of the two sets
    /// </summary>
    /// <param name="rhs">Right hand side set</param>
    /// <returns>Subtracted set</returns>
    [Pure]
    public SetInternal<A> Subtract(SetInternal<A> rhs)
    {
        if (Count     == 0) return Empty;
        if (rhs.Count == 0) return this;

        if (rhs.Count < Count)
        {
            var self = this;
            foreach (var item in rhs)
            {
                self = self.Remove(item);
            }
            return self;
        }
        else
        {
            var root = SetItem<A>.Empty;
            foreach (var item in this)
            {
                if (!rhs.Contains(item))
                {
                    root = SetModule.TryAdd<A>(root, item, _comparer);
                }
            }
            return new SetInternal<A>(root, _comparer);
        }
    }

    /// <summary>
    /// Equality test
    /// </summary>
    /// <param name="other">Other set to test</param>
    /// <returns>True if sets are equal</returns>
    [Pure]
    public bool Equals(SetInternal<A>? other) =>
        other is not null && SetEquals(other.AsIterable());

    [Pure]
    public int CompareTo(SetInternal<A> other) => 
        this.collectionCompare(other, _comparer);

    [Pure]
    public int CompareTo(SetInternal<A> other, IComparer<A> comparer) => 
        this.collectionCompare(other, comparer);

    IEnumerator IEnumerable.GetEnumerator() =>
        new SetModule.SetEnumerator<A>(set, false, 0);

    public override bool Equals(object? obj) => Equals(obj as SetInternal<A>);
    public SetInternal<A> Clear() => new (_comparer);
}

[Serializable]
internal sealed class SetItem<K>
{
    public static readonly SetItem<K> Empty = new (0, 0, default!, default!, default!);

    public bool IsEmpty => Count == 0;
    public int Count;
    public byte Height;
    public SetItem<K> Left;
    public SetItem<K> Right;

    /// <summary>
    /// Ctor
    /// </summary>
    internal SetItem(byte height, int count, K key, SetItem<K> left, SetItem<K> right)
    {
        Count = count;
        Height = height;
        Key = key;
        Left = left;
        Right = right;
    }

    [Pure]
    internal int BalanceFactor =>
        Count == 0
            ? 0
            : Right.Height - Left.Height;

    [Pure]
    public K Key
    {
        get;
        internal set;
    }
}

internal static class SetModule
{
    public enum AddOpt
    {
        ThrowOnDuplicate,
        TryAdd,
        TryUpdate
    }

    [Pure]
    public static bool Exists<K>(SetItem<K> node, Func<K, bool> pred) =>
        !node.IsEmpty && (pred(node.Key) || Exists(node.Left, pred) || Exists(node.Right, pred));

    [Pure]
    public static SetItem<K> Add<K>(SetItem<K> node, K key, IComparer<K> comparer) 
    {
        if (node.IsEmpty)
        {
            return new SetItem<K>(1, 1, key, SetItem<K>.Empty, SetItem<K>.Empty);
        }
        var cmp = comparer.Compare(key, node.Key);
        return cmp switch
        {
            < 0 => Balance(Make(node.Key, Add(node.Left, key, comparer), node.Right)),
            > 0 => Balance(Make(node.Key, node.Left, Add(node.Right, key, comparer))),
            _ => throw new ArgumentException("An element with the same key already exists in the set")
        };
    }

    [Pure]
    public static SetItem<K> TryAdd<K>(SetItem<K> node, K key, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            return new SetItem<K>(1, 1, key, SetItem<K>.Empty, SetItem<K>.Empty);
        }
        var cmp = comparer.Compare(key, node.Key);
        return cmp switch
        {
            < 0 => Balance(Make(node.Key, TryAdd(node.Left, key, comparer), node.Right)),
            > 0 => Balance(Make(node.Key, node.Left, TryAdd(node.Right, key, comparer))),
            _ => node
        };
    }

    [Pure]
    public static SetItem<K> AddOrUpdate<K>(SetItem<K> node, K key, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            return new SetItem<K>(1, 1, key, SetItem<K>.Empty, SetItem<K>.Empty);
        }
        var cmp = comparer.Compare(key, node.Key);
        return cmp switch
        {
            < 0 => Balance(Make(node.Key, TryAdd(node.Left, key, comparer), node.Right)),
            > 0 => Balance(Make(node.Key, node.Left, TryAdd(node.Right, key, comparer))),
            _ => new SetItem<K>(node.Height, node.Count, key, node.Left, node.Right)
        };
    }

    [Pure]
    public static SetItem<K> AddTreeToRight<K>(SetItem<K> node, SetItem<K> toAdd) =>
        node.IsEmpty
            ? toAdd
            : Balance(Make(node.Key, node.Left, AddTreeToRight(node.Right, toAdd)));

    [Pure]
    public static SetItem<K> Remove<K>(SetItem<K> node, K key, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            return node;
        }
        var cmp = comparer.Compare(key, node.Key);
        return cmp switch
        {
            < 0 => Balance(Make(node.Key, Remove(node.Left, key, comparer), node.Right)),
            > 0 => Balance(Make(node.Key, node.Left, Remove(node.Right, key, comparer))),
            _ => Balance(AddTreeToRight(node.Left, node.Right))
        };
    }

    [Pure]
    public static bool Contains<K>(SetItem<K> node, K key, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            return false;
        }
        var cmp = comparer.Compare(key, node.Key);
        return cmp switch
        {
            < 0 => Contains(node.Left, key, comparer),
            > 0 => Contains(node.Right, key, comparer),
            _ => true
        };
    }

    [Pure]
    public static K Find<K>(SetItem<K> node, K key, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            throw new ArgumentException("Key not found in set");
        }
        var cmp = comparer.Compare(key, node.Key);
        return cmp switch
        {
            < 0 => Find(node.Left, key, comparer),
            > 0 => Find(node.Right, key, comparer),
            _ => node.Key
        };
    }

    /// <summary>
    /// TODO: I suspect this is suboptimal, it would be better with a custom Enumerator 
    /// that maintains a stack of nodes to retrace.
    /// </summary>
    [Pure]
    public static IEnumerable<K> FindRange<K>(SetItem<K> node, K a, K b, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            yield break;
        }
        if (comparer.Compare(node.Key, a) < 0)
        {
            foreach (var item in FindRange(node.Right, a, b, comparer))
            {
                yield return item;
            }
        }
        else if (comparer.Compare(node.Key, b) > 0)
        {
            foreach (var item in FindRange(node.Left, a, b, comparer))
            {
                yield return item;
            }
        }
        else
        {
            foreach (var item in FindRange(node.Left, a, b, comparer))
            {
                yield return item;
            }
            yield return node.Key;
            foreach (var item in FindRange(node.Right, a, b, comparer))
            {
                yield return item;
            }
        }
    }

    [Pure]
    public static Option<K> TryFind<K>(SetItem<K> node, K key, IComparer<K> comparer)
    {
        if (node.IsEmpty)
        {
            return None;
        }
        var cmp = comparer.Compare(key, node.Key);
        return cmp switch
        {
            < 0 => TryFind(node.Left, key, comparer),
            > 0 => TryFind(node.Right, key, comparer),
            _ => Optional(node.Key)
        };
    }

    [Pure]
    public static SetItem<K> Skip<K>(SetItem<K> node, int amount)
    {
        if (amount == 0 || node.IsEmpty)
        {
            return node;
        }
        if (amount >= node.Count)
        {
            return SetItem<K>.Empty;
        }
        if (!node.Left.IsEmpty && node.Left.Count == amount)
        {
            return Balance(Make(node.Key, SetItem<K>.Empty, node.Right));
        }
        if (!node.Left.IsEmpty && node.Left.Count == amount - 1)
        {
            return node.Right;
        }
        if (node.Left.IsEmpty)
        {
            return Skip(node.Right, amount - 1);
        }

        var newleft   = Skip(node.Left, amount);
        var remaining = amount - node.Left.Count - newleft.Count;
        if (remaining > 0)
        {
            return Skip(Balance(Make(node.Key, newleft, node.Right)), remaining);
        }
        else
        {
            return Balance(Make(node.Key, newleft, node.Right));
        }
    }

    [Pure]
    public static SetItem<K> Make<K>(K k, SetItem<K> l, SetItem<K> r) =>
        new ((byte)(1 + Math.Max(l.Height, r.Height)), l.Count + r.Count + 1, k, l, r);

    [Pure]
    public static SetItem<K> Balance<K>(SetItem<K> node) =>
        node.BalanceFactor >= 2
            ? node.Right.BalanceFactor < 0
                  ? DblRotLeft(node)
                  : RotLeft(node)
            : node.BalanceFactor <= -2
                ? node.Left.BalanceFactor > 0
                      ? DblRotRight(node)
                      : RotRight(node)
                : node;

    [Pure]
    public static SetItem<K> RotRight<K>(SetItem<K> node) =>
        node.IsEmpty || node.Left.IsEmpty
            ? node
            : Make(node.Left.Key, node.Left.Left, Make(node.Key, node.Left.Right, node.Right));

    [Pure]
    public static SetItem<K> RotLeft<K>(SetItem<K> node) =>
        node.IsEmpty || node.Right.IsEmpty
            ? node
            : Make(node.Right.Key, Make(node.Key, node.Left, node.Right.Left), node.Right.Right);

    [Pure]
    public static SetItem<K> DblRotRight<K>(SetItem<K> node) =>
        node.IsEmpty
            ? node
            : RotRight(Make(node.Key, RotLeft(node.Left), node.Right));

    [Pure]
    public static SetItem<K> DblRotLeft<K>(SetItem<K> node) =>
        node.IsEmpty
            ? node
            : RotLeft(Make(node.Key, node.Left, RotRight(node.Right)));

    internal static Option<A> Max<A>(SetItem<A> node) =>
        node.Right.IsEmpty
            ? node.Key
            : Max(node.Right);

    internal static Option<A> Min<A>(SetItem<A> node) =>
        node.Left.IsEmpty
            ? node.Key
            : Min(node.Left);

    internal static Option<A> TryFindPredecessor<A>(SetItem<A> root, A key, IComparer<A> comparer)
    {
        Option<A> predecessor = None;
        var       current     = root;

        if (root.IsEmpty)
        {
            return None;
        }

        do
        {
            var cmp = comparer.Compare(key, current.Key);
            if (cmp < 0)
            {
                current = current.Left;
            }
            else if (cmp > 0)
            {
                predecessor = current.Key;
                current = current.Right;
            }
            else
            {
                break;
            }
        }
        while (!current.IsEmpty);

        if(!current.IsEmpty && !current.Left.IsEmpty)
        {
            predecessor = Max(current.Left);
        }

        return predecessor;
    }

    internal static Option<A> TryFindOrPredecessor<A>(SetItem<A> root, A key, IComparer<A> comparer)
    {
        Option<A> predecessor = None;
        var       current     = root;

        if (root.IsEmpty)
        {
            return None;
        }

        do
        {
            var cmp = comparer.Compare(key, current.Key);
            if (cmp < 0)
            {
                current = current.Left;
            }
            else if (cmp > 0)
            {
                predecessor = current.Key;
                current = current.Right;
            }
            else
            {
                return current.Key;
            }
        }
        while (!current.IsEmpty);

        if (!current.IsEmpty && !current.Left.IsEmpty)
        {
            predecessor = Max(current.Left);
        }

        return predecessor;
    }

    internal static Option<A> TryFindSuccessor<A>(SetItem<A> root, A key, IComparer<A> comparer)
    {
        Option<A> successor = None;
        var       current   = root;

        if (root.IsEmpty)
        {
            return None;
        }

        do
        {
            var cmp = comparer.Compare(key, current.Key);
            if (cmp < 0)
            {
                successor = current.Key;
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

        return successor;
    }

    internal static Option<A> TryFindOrSuccessor<A>(SetItem<A> root, A key, IComparer<A> comparer)
    {
        Option<A> successor = None;
        var       current   = root;

        if (root.IsEmpty)
        {
            return None;
        }

        do
        {
            var cmp = comparer.Compare(key, current.Key);
            if (cmp < 0)
            {
                successor = current.Key;
                current = current.Left;
            }
            else if (cmp > 0)
            {
                current = current.Right;
            }
            else
            {
                return current.Key;
            }
        }
        while (!current.IsEmpty);

        if (!current.IsEmpty && !current.Right.IsEmpty)
        {
            successor = Min(current.Right);
        }

        return successor;
    }

    public sealed class SetEnumerator<K> : IEnumerator<K>
    {
        internal struct NewStack : New<SetItem<K>[]>
        {
            public static SetItem<K>[] Create() =>
                new SetItem<K>[32];
        }

        int stackDepth;
        SetItem<K>[]? stack;
        readonly SetItem<K> map;
        int left;
        readonly bool rev;
        readonly int start;

        public SetEnumerator(SetItem<K> root, bool rev, int start)
        {
            this.rev = rev;
            this.start = start;
            map = root;
            stack = Pool<NewStack, SetItem<K>[]>.Pop();
            NodeCurrent = default!;
            Reset();
        }

        private SetItem<K> NodeCurrent
        {
            get;
            set;
        }

        public K Current => NodeCurrent.Key;
        object IEnumerator.Current => NodeCurrent.Key!;

        public void Dispose()
        {
            if (stack is not null)
            {
                Pool<NewStack, SetItem<K>[]>.Push(stack);
                stack = default!;
            }
        }

        private SetItem<K> Next(SetItem<K> node) =>
            rev ? node.Left : node.Right;

        private SetItem<K> Prev(SetItem<K> node) =>
            rev ? node.Right : node.Left;

        private void Push(SetItem<K> node)
        {
            while (!node.IsEmpty)
            {
                stack![stackDepth] = node;
                stackDepth++;
                node = Prev(node);
            }
        }

        public bool MoveNext()
        {
            if (left > 0 && stackDepth > 0)
            {
                stackDepth--;
                NodeCurrent = stack![stackDepth];
                Push(Next(NodeCurrent));
                left--;
                return true;
            }

            NodeCurrent = default!;
            return false;
        }

        public void Reset()
        {
            var skip = rev ? map.Count - start - 1 : start;

            stackDepth = 0;
            NodeCurrent = map;
            left = map.Count;

            while (!NodeCurrent.IsEmpty && skip != Prev(NodeCurrent).Count)
            {
                if (skip < Prev(NodeCurrent).Count)
                {
                    stack![stackDepth] = NodeCurrent;
                    stackDepth++;
                    NodeCurrent = Prev(NodeCurrent);
                }
                else
                {
                    skip -= Prev(NodeCurrent).Count + 1;
                    NodeCurrent = Next(NodeCurrent);
                }
            }

            if (!NodeCurrent.IsEmpty)
            {
                stack![stackDepth] = NodeCurrent;
                stackDepth++;
            }
        }
    }
}
