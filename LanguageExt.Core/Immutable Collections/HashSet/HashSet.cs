﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using LanguageExt.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt;

/// <summary>
/// Unsorted immutable hash-set
/// </summary>
/// <typeparam name="A">Key type</typeparam>
[CollectionBuilder(typeof(HashSet), nameof(HashSet.createRange))]
public readonly struct HashSet<A> :
    IReadOnlyCollection<A>,
    IEquatable<HashSet<A>>,
    IEqualityOperators<HashSet<A>, HashSet<A>, bool>,
    IAdditionOperators<HashSet<A>, HashSet<A>, HashSet<A>>,
    ISubtractionOperators<HashSet<A>, HashSet<A>, HashSet<A>>,
    IAdditiveIdentity<HashSet<A>, HashSet<A>>,
    Monoid<HashSet<A>>,
    K<HashSet, A>
{
    public static HashSet<A> Empty { get; } = new (TrieSet<A>.Empty());

    readonly TrieSet<A>? _value;
    internal TrieSet<A> Value => _value ?? TrieSet<A>.Empty();

    internal HashSet(IEqualityComparer<A> equalityComparer)
    {
        _value = TrieSet<A>.Empty(equalityComparer);
    }

    internal HashSet(TrieSet<A> value)
    {
        _value = value;
    }

    static HashSet<A> Wrap(TrieSet<A> value) =>
        new (value);

    /// <summary>
    /// Ctor that takes an initial (distinct) set of items
    /// </summary>
    /// <param name="items">Source items</param>
    /// <param name="tryAdd">Adding strategy</param>
    /// <param name="equalityComparer">Optional comparer</param>
    public HashSet(ReadOnlySpan<A> items, bool tryAdd = true, IEqualityComparer<A>? equalityComparer = null) =>
        _value = new TrieSet<A>(items, tryAdd, equalityComparer);

    /// <summary>
    /// Ctor that takes an initial (distinct) set of items
    /// </summary>
    /// <remarks>Used for deserialization purposes</remarks>
    public HashSet(IEnumerable<A> items) =>
        _value = new TrieSet<A>(items);

    /// <summary>
    /// Ctor that takes an initial (distinct) set of items
    /// </summary>
    public HashSet(IEnumerable<A> items, bool tryAdd, IEqualityComparer<A>? equalityComparer = null) =>
        _value = new TrieSet<A>(items, tryAdd, equalityComparer);

    /// <summary>
    /// Item at index lens
    /// </summary>
    [Pure]
    public static Lens<HashSet<A>, bool> item(A key) => Lens<HashSet<A>, bool>.New(
        Get: la => la.Contains(key!),
        Set: a => la => a ? la.AddOrUpdate(key!) : la.Remove(key!)
    );

    /// <summary>
    /// Lens map
    /// </summary>
    [Pure]
    public static Lens<HashSet<A>, HashSet<A>> map<B>(Lens<A, A> lens) => Lens<HashSet<A>, HashSet<A>>.New(
        Get: la => la.Map(lens.Get),
        Set: lb => la =>
                   {
                       foreach (var item in lb)
                       {
                           la = la.Find(item!).Match(Some: x => la.AddOrUpdate(lens.Set(x, item)!), None: () => la);
                       }
                       return la;
                   });

    /// <summary>
    /// 'this' accessor
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Optional value</returns>
    [Pure]
    public A this[A key] =>
        Value[key!];

    /// <summary>
    /// Is the set empty
    /// </summary>
    [Pure]
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value?.IsEmpty ?? true;
    }

    /// <summary>
    /// Number of items in the set
    /// </summary>
    [Pure]
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value?.Count ?? 0;
    }
        
    /// <summary>
    /// Impure iteration of the bound values in the structure
    /// </summary>
    /// <returns>
    /// Returns the original unmodified structure
    /// </returns>
    public HashSet<A> Do(Action<A> f)
    {
        _ = this.Iter(f);
        return this;
    }

    /// <summary>
    /// Maps the values of this set into a new set of values using the
    /// mapper function to transform the source values.
    /// </summary>
    /// <typeparam name="R">Mapped element type</typeparam>
    /// <param name="mapper">Mapping function</param>
    /// <returns>Mapped enumerable</returns>
    [Pure]
    public HashSet<R> Map<R>(Func<A, R> mapper)
    {
        static IEnumerable<R> Yield(TrieSet<A> map, Func<A, R> f)
        {
            foreach (var item in map)
            {
                yield return f(item);
            }
        }
        return new HashSet<R>(Yield(Value, mapper));
    }
    
    /// <summary>
    /// Map each element of a structure to an action, evaluate these actions from
    /// left to right, and collect the results.
    /// </summary>
    /// <param name="f"></param>
    /// <typeparam name="F">Applicative functor trait</typeparam>
    /// <typeparam name="B">Bound value (output)</typeparam>
    [Pure]
    public K<F, HashSet<B>> Traverse<F, B>(Func<A, K<F, B>> f) 
        where F : Applicative<F> =>
        F.Map(x => x.As(), Traversable.traverse(f, this));
    
    /// <summary>
    /// Map each element of a structure to an action, evaluate these actions from
    /// left to right, and collect the results.
    /// </summary>
    /// <param name="f"></param>
    /// <typeparam name="M">Monad trait</typeparam>
    /// <typeparam name="B">Bound value (output)</typeparam>
    [Pure]
    public K<M, HashSet<B>> TraverseM<M, B>(Func<A, K<M, B>> f) 
        where M : Monad<M> =>
        M.Map(x => x.As(), Traversable.traverseM(f, this));
    
    /// <summary>
    /// Filters items from the set using the predicate.  If the predicate
    /// returns True for any item then it remains in the set, otherwise
    /// it's dropped.
    /// </summary>
    /// <param name="pred">Predicate</param>
    /// <returns>Filtered enumerable</returns>
    [Pure]
    public HashSet<A> Filter(Func<A, bool> pred)
    {
        static IEnumerable<A> Yield(TrieSet<A> map, Func<A, bool> f)
        {
            foreach (var item in map)
            {
                if (f(item))
                {
                    yield return item;
                }
            }
        }
        return new HashSet<A>(Yield(Value, pred));
    }
    /// <summary>
    /// Maps the values of this set into a new set of values using the
    /// mapper function to transform the source values.
    /// </summary>
    /// <typeparam name="R">Mapped element type</typeparam>
    /// <param name="mapper">Mapping function</param>
    /// <returns>Mapped enumerable</returns>
    [Pure]
    public HashSet<R> Select<R>(Func<A, R> mapper) =>
        Map(mapper);

    /// <summary>
    /// Filters items from the set using the predicate.  If the predicate
    /// returns True for any item then it remains in the set, otherwise
    /// it's dropped.
    /// </summary>
    /// <param name="pred">Predicate</param>
    /// <returns>Filtered enumerable</returns>
    [Pure]
    public HashSet<A> Where(Func<A, bool> pred) =>
        Filter(pred);

    /// <summary>
    /// Add an item to the set
    /// </summary>
    /// <param name="key">Value to add to the set</param>
    /// <returns>New set with the item added</returns>
    [Pure]
    public HashSet<A> Add(A key) =>
        Wrap(Value.Add(key));

    /// <summary>
    /// Attempt to add an item to the set.  If an item already
    /// exists then return the Set as-is.
    /// </summary>
    /// <param name="key">Value to add to the set</param>
    /// <returns>New set with the item maybe added</returns>
    [Pure]
    public HashSet<A> TryAdd(A key) =>
        Wrap(Value.TryAdd(key));

    /// <summary>
    /// Add an item to the set.  If an item already
    /// exists then replace it.
    /// </summary>
    /// <param name="key">Value to add to the set</param>
    /// <returns>New set with the item maybe added</returns>
    [Pure]
    public HashSet<A> AddOrUpdate(A key) =>
        Wrap(Value.AddOrUpdate(key));

    /// <summary>
    /// Atomically adds a range of items to the set.
    /// </summary>
    /// <remarks>Null is not allowed for a Key</remarks>
    /// <param name="range">Range of keys to add</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if any of the keys already exist</exception>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException if any of the keys are null</exception>
    /// <returns>New HSet with the items added</returns>
    [Pure]
    public HashSet<A> AddRange(IEnumerable<A> range) =>
        Wrap(Value.AddRange(range));

    /// <summary>
    /// Atomically adds a range of items to the set.  If an item already exists, it's ignored.
    /// </summary>
    /// <remarks>Null is not allowed for a Key</remarks>
    /// <param name="range">Range of keys to add</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException if any of the keys are null</exception>
    /// <returns>New HSet with the items added</returns>
    [Pure]
    public HashSet<A> TryAddRange(IEnumerable<A> range) =>
        Wrap(Value.TryAddRange(range));

    /// <summary>
    /// Atomically adds a range of items to the set.  If an item already exists then replace it.
    /// </summary>
    /// <remarks>Null is not allowed for a Key</remarks>
    /// <param name="range">Range of keys to add</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException if any of the keys are null</exception>
    /// <returns>New HSet with the items added</returns>
    [Pure]
    public HashSet<A> AddOrUpdateRange(IEnumerable<A> range) =>
        Wrap(Value.AddOrUpdateRange(range));

    /// <summary>
    /// Atomically removes an item from the set
    /// If the key doesn't exists, the request is ignored.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>New map with the item removed</returns>
    [Pure]
    public HashSet<A> Remove(A key) =>
        Wrap(Value.Remove(key));

    /// <summary>
    /// Retrieve a value from the set by key
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <returns>Found value</returns>
    [Pure]
    public Option<A> Find(A key) =>
        Value.Find(key);

    /// <summary>
    /// Retrieve a value from the set by key as an enumerable
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <returns>Found value</returns>
    [Pure]
    public Seq<A> FindSeq(A key) =>
        Find(key).ToSeq();

    /// <summary>
    /// Retrieve a value from the set by key and pattern match the
    /// result.
    /// </summary>
    /// <param name="key">Key to find</param>
    /// <param name="Some">Existing item action</param>
    /// <param name="None">Missing item action</param>
    /// <returns>Found value</returns>
    [Pure]
    public R Find<R>(A key, Func<A, R> Some, Func<R> None) =>
        Find(key).Match(Some, None);

    /// <summary>
    /// Atomically updates an existing item
    /// </summary>
    /// <remarks>Null is not allowed for a key</remarks>
    /// <param name="key">Key</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException if the key is null</exception>
    /// <returns>New HSet with the item added</returns>
    [Pure]
    public HashSet<A> SetItem(A key) =>
        Wrap(Value.SetItem(key));

    /// <summary>
    /// Atomically updates an existing item, unless it doesn't exist, in which case 
    /// it is ignored
    /// </summary>
    /// <remarks>Null is not allowed for a key</remarks>
    /// <param name="key">Key</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException if the key is null</exception>
    /// <returns>New HSet with the item added</returns>
    [Pure]
    public HashSet<A> TrySetItem(A key) =>
        Wrap(Value.TrySetItem(key));

    /// <summary>
    /// Checks for existence of a key in the set
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <returns>True if an item with the key supplied is in the set</returns>
    [Pure]
    public bool Contains(A key) =>
        Value.ContainsKey(key);

    /// <summary>
    /// Clears all items from the set
    /// </summary>
    /// <remarks>Functionally equivalent to calling HSet.empty as the original structure is untouched</remarks>
    /// <returns>Empty HSet</returns>
    [Pure]
    public HashSet<A> Clear() =>
        new(Value.Clear());

    /// <summary>
    /// Atomically sets a series of items
    /// </summary>
    /// <param name="items">Items to set</param>
    /// <exception cref="ArgumentException">Throws ArgumentException if any of the keys aren't in the map</exception>
    /// <returns>New HSet with the items set</returns>
    [Pure]
    public HashSet<A> SetItems(IEnumerable<A> items) =>
        Wrap(Value.SetItems(items));

    /// <summary>
    /// Atomically sets a series of items.  If any of the items don't exist then they're silently ignored.
    /// </summary>
    /// <param name="items">Items to set</param>
    /// <returns>New HSet with the items set</returns>
    [Pure]
    public HashSet<A> TrySetItems(IEnumerable<A> items) =>
        Wrap(Value.TrySetItems(items));

    /// <summary>
    /// Atomically removes a list of keys from the set
    /// </summary>
    /// <param name="keys">Keys to remove</param>
    /// <returns>New HSet with the items removed</returns>
    [Pure]
    public HashSet<A> RemoveRange(IEnumerable<A> keys) =>
        Wrap(Value.RemoveRange(keys));

    /// <summary>
    /// GetEnumerator - IEnumerable interface
    /// </summary>
    [Pure]
    public IEnumerator<A> GetEnumerator() =>
        Value.GetEnumerator();

    /// <summary>
    /// GetEnumerator - IEnumerable interface
    /// </summary>
    [Pure]
    IEnumerator IEnumerable.GetEnumerator() =>
        Value.GetEnumerator();

    [Pure]
    public Seq<A> ToSeq() =>
        toSeq(this);

    /// <summary>
    /// Format the collection as `[a, b, c, ...]`
    /// The ellipsis is used for collections over 50 items
    /// To get a formatted string with all the items, use `ToFullString`
    /// or `ToFullArrayString`.
    /// </summary>
    [Pure]
    public override string ToString() =>
        CollectionFormat.ToShortArrayString(this, Count);

    /// <summary>
    /// Format the collection as `a, b, c, ...`
    /// </summary>
    [Pure]
    public string ToFullString(string separator = ", ") =>
        CollectionFormat.ToFullString(this, separator);

    /// <summary>
    /// Format the collection as `[a, b, c, ...]`
    /// </summary>
    [Pure]
    public string ToFullArrayString(string separator = ", ") =>
        CollectionFormat.ToFullArrayString(this, separator);

    [Pure]
    public Iterable<A> AsIterable() =>
        IterableExtensions.AsIterable(this);

    /// <summary>
    /// Implicit conversion from an untyped empty list
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator HashSet<A>(SeqEmpty _) =>
        Empty;
        
    [Pure]
    public static HashSet<A> operator +(HashSet<A> lhs, HashSet<A> rhs) =>
        lhs.Combine(rhs);

    /// <summary>
    /// Choice operator
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HashSet<A> operator |(HashSet<A> x, K<HashSet, A> y) =>
        x.Choose(y).As();

    /// <summary>
    /// Choice operator
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HashSet<A> operator |(K<HashSet, A> x, HashSet<A> y) =>
        x.Choose(y).As();
    
    [Pure]
    public HashSet<A> Combine(HashSet<A> rhs) =>
        Wrap(Value.Append(rhs.Value));

    [Pure]
    public static HashSet<A> operator -(HashSet<A> lhs, HashSet<A> rhs) =>
        lhs.Subtract(rhs);

    [Pure]
    public HashSet<A> Subtract(HashSet<A> rhs) =>
        Wrap(Value.Subtract(rhs.Value));

    /// <summary>
    /// Equality
    /// </summary>
    [Pure]
    public static bool operator ==(HashSet<A> lhs, HashSet<A> rhs) =>
        lhs.Equals(rhs);

    /// <summary>
    /// In-equality
    /// </summary>
    [Pure]
    public static bool operator !=(HashSet<A> lhs, HashSet<A> rhs) =>
        !(lhs == rhs);

    /// <summary>
    /// Equality
    /// </summary>
    [Pure]
    public bool Equals(HashSet<A> other) =>
        Value.Equals(other.Value);

    /// <summary>
    /// Returns True if 'other' is a proper subset of this set
    /// </summary>
    /// <returns>True if 'other' is a proper subset of this set</returns>
    [Pure]
    public bool IsProperSubsetOf(IEnumerable<A> other) =>
        Value.IsProperSubsetOf(other);

    /// <summary>
    /// Returns True if 'other' is a proper superset of this set
    /// </summary>
    /// <returns>True if 'other' is a proper superset of this set</returns>
    [Pure]
    public bool IsProperSupersetOf(IEnumerable<A> other) =>
        Value.IsProperSupersetOf(other);

    /// <summary>
    /// Returns True if 'other' is a superset of this set
    /// </summary>
    /// <returns>True if 'other' is a superset of this set</returns>
    [Pure]
    public bool IsSubsetOf(IEnumerable<A> other) =>
        Value.IsSubsetOf(other);

    /// <summary>
    /// Returns True if other overlaps this set
    /// </summary>
    [Pure]
    public bool Overlaps(IEnumerable<A> other) =>
        Value.Overlaps(other);

    /// <summary>
    /// Returns True if 'other' is a superset of this set
    /// </summary>
    /// <returns>True if 'other' is a superset of this set</returns>
    [Pure]
    public bool IsSupersetOf(IEnumerable<A> rhs) =>
        Value.IsSupersetOf(rhs);

    /// <summary>
    /// Returns the elements that are in both this and other
    /// </summary>
    [Pure]
    public HashSet<A> Intersect(IEnumerable<A> rhs) =>
        Wrap(Value.Intersect(rhs));

    /// <summary>
    /// Returns this - other.  Only the items in this that are not in 
    /// other will be returned.
    /// </summary>
    [Pure]
    public HashSet<A> Except(IEnumerable<A> rhs) =>
        Wrap(Value.Except(rhs));

    /// <summary>
    /// Only items that are in one set or the other will be returned.
    /// If an item is in both, it is dropped.
    /// </summary>
    [Pure]
    public HashSet<A> SymmetricExcept(HashSet<A> rhs) =>
        Wrap(Value.SymmetricExcept(rhs.Value));

    /// <summary>
    /// Only items that are in one set or the other will be returned.
    /// If an item is in both, it is dropped.
    /// </summary>
    [Pure]
    public HashSet<A> SymmetricExcept(IEnumerable<A> rhs) =>
        Wrap(Value.SymmetricExcept(rhs));

    /// <summary>
    /// Finds the union of two sets and produces a new set with 
    /// the results
    /// </summary>
    /// <param name="rhs">Other set to union with</param>
    /// <returns>A set which contains all items from both sets</returns>
    [Pure]
    public HashSet<A> Union(IEnumerable<A> rhs) =>
        TryAddRange(rhs);

    /// <summary>
    /// Copy the items from the set into the specified array
    /// </summary>
    /// <param name="array">Array to copy to</param>
    /// <param name="index">Index into the array to start</param>
    public Unit CopyTo(A[] array, int index)
    {
        var       max  = array.Length;
        using var iter = GetEnumerator();
        for (var i = index; i < max && iter.MoveNext(); i++)
        {
            array[i] = iter.Current;
        }

        return default;
    }

    /// <summary>
    /// Copy the items from the set into the specified array
    /// </summary>
    /// <param name="array">Array to copy to</param>
    /// <param name="index">Index into the array to start</param>
    public Unit CopyTo(Array array, int index)
    {
        var       max  = array.Length;
        using var iter = GetEnumerator();
        for (var i = index; i < max && iter.MoveNext(); i++)
        {
            array.SetValue(iter.Current, i);
        }
        return default;
    }

    [Pure]
    public override bool Equals(object? obj) =>
        obj is HashSet<A> hs && Equals(hs);

    [Pure]
    public override int GetHashCode() =>
        Value.GetHashCode();

    [Pure]
    public bool HasSameEqualityComparer(IEqualityComparer<A> equalityComparer) => 
        Value.HasSameEqualityComparer(equalityComparer);

    [Pure]
    public HashSet<B> Bind<B>(Func<A, HashSet<B>> f)
    {
        var self = this;

        IEnumerable<B> Yield()
        {
            foreach (var x in self.AsIterable())
            {
                foreach (var y in f(x))
                {
                    yield return y;
                }
            }
        }
        return new HashSet<B>(Yield(), true);
    }

    [Pure]
    public HashSet<C> SelectMany<B, C>(Func<A, HashSet<B>> bind, Func<A, B, C> project)
    {
        var self = this;

        IEnumerable<C> Yield()
        {
            foreach (var x in self.AsIterable())
            {
                foreach (var y in bind(x))
                {
                    yield return project(x, y);
                }
            }
        }
        return new HashSet<C>(Yield(), true);
    }
    
    public static HashSet<A> AdditiveIdentity => 
        Empty;    
}
