﻿using System.Diagnostics.Contracts;
using LanguageExt.Traits;

namespace LanguageExt.ClassInstances;

/// <summary>
/// Either type hashing
/// </summary>
public readonly struct HashableEither<HashableL, HashableR, L, R> : Hashable<Either<L, R>>
    where HashableL : Hashable<L>
    where HashableR : Hashable<R>
{
    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    public static int GetHashCode(Either<L, R> x) =>
        x switch
        {
            Either.Right<L, R> => HashableR.GetHashCode(x.RightValue),
            Either.Left<L, R>  => HashableL.GetHashCode(x.LeftValue),
            _                  => 0
        };
}
    
/// <summary>
/// Either type hashing
/// </summary>
public readonly struct HashableEither<L, R> : Hashable<Either<L, R>>
{
    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure] 
    public static int GetHashCode(Either<L, R> x) =>
        HashableEither<HashableDefault<L>, HashableDefault<R>, L, R>.GetHashCode(x);
}
