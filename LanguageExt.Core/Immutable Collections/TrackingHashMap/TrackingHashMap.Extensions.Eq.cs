using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LanguageExt.Traits;

namespace LanguageExt;

public static partial class TrackingHashMapExtensions
{
    /// <summary>
    /// Create an immutable tracking hash-map
    /// </summary>
    [Pure]
    public static TrackingHashMap<EqK, K, V> ToTrackingHashMap<EqK, K, V>(this IEnumerable<(K, V)> items) 
        where EqK : Eq<K> =>
        TrackingHashMap.createRange<EqK, K, V>(items);

    /// <summary>
    /// Create an immutable tracking hash-map
    /// </summary>
    [Pure]
    public static TrackingHashMap<EqK, K, V> ToTrackingHashMap<EqK, K, V>(this IEnumerable<Tuple<K, V>> items)
        where EqK : Eq<K> =>
        TrackingHashMap.createRange<EqK, K, V>(items);

    /// <summary>
    /// Create an immutable tracking hash-map
    /// </summary>
    [Pure]
    public static TrackingHashMap<EqK, K, V> ToTrackingHashMap<EqK, K, V>(this IEnumerable<KeyValuePair<K, V>> items)
        where EqK : Eq<K> =>
        TrackingHashMap.createRange<EqK, K, V>(items);
}
