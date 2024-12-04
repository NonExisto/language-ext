﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using LanguageExt.Traits;

namespace LanguageExt
{
    /// <summary>
    /// Fowler–Noll–Vo 32bit hash function
    /// </summary>
    /// <remarks>
    /// [Fowler–Noll–Vo hash function](https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function)
    /// </remarks>
    internal static class FNV32
    {
        /// <summary>
        /// Offset basis for a FNV-1 or FNV-1a 32 bit hash
        /// </summary>
        public const int OffsetBasis = -2128831035;

        /// <summary>
        /// Prime for FNV-1 or FNV-1a 32 bit hash
        /// </summary>
        public const int Prime = 16777619;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Next(int hashA, int hashB)
        {
            unchecked
            {
                return (hashA ^ hashB) * Prime;
            }
        }

        /// <summary>
        /// Calculate the hash code for an array
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash<HashA, A>(A[]? items, int offsetBasis = OffsetBasis) where HashA : Hashable<A>
        {
            int hash = offsetBasis;
            if (items == null) return hash;


            foreach (var item in items)
            {
                hash = Next(HashA.GetHashCode(item), hash);
            }
            return hash;

        }

        /// <summary>
        /// Calculate the hash code for an array slice
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash<HashA, A>(A[]? items, int start, int length, int offsetBasis = OffsetBasis) where HashA : Hashable<A>
        {
            int hash = offsetBasis;
            if (items == null) return hash;


            var span = new ReadOnlySpan<A>(items, start, length);
            foreach (var item in span)
            {
                hash = Next(HashA.GetHashCode(item), hash);
            }
            return hash;

        }

        /// <summary>
        /// Calculate the hash code for an enumerable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash<HashA, A>(IEnumerable<A>? items, int offsetBasis = OffsetBasis) where HashA : Hashable<A>
        {
            int hash = offsetBasis;
            if (items == null) return hash;


            foreach (var item in items)
            {
                hash = Next(HashA.GetHashCode(item), hash);
            }
            return hash;

        }

        /// <summary>
        /// Calculate the hash code for an enumerable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash<A>([DisallowNull]IEqualityComparer<A> equalityComparer, IEnumerable<A>? items, int offsetBasis = OffsetBasis)
        {
            int hash = offsetBasis;
            if (items == null) return hash;


            foreach (A item in items)
            {
                hash = Next(equalityComparer.GetHashCode(item!), hash);
            }
            return hash;

        }

        /// <summary>
        /// Calculate the hash code for an enumerable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash<T, V>([DisallowNull]IEqualityComparer<T> equalityComparer, IEnumerable<(T key, V value)>? items, int offsetBasis = OffsetBasis)
        {
            int hash = offsetBasis;
            if (items == null) return hash;

            var valueComparer = Prelude.getRegisteredEqualityComparerOrDefault<V>();

            foreach (var (key, value) in items)
            {
                hash = Next(Next(equalityComparer.GetHashCode(key!), valueComparer.GetHashCode(value!)), hash);
            }
            return hash;

        }
    }
}
