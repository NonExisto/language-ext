﻿using LanguageExt.Traits;
using System.Diagnostics.Contracts;

namespace LanguageExt.ClassInstances
{
    /// <summary>
    /// Floating point equality
    /// </summary>
    public struct EqDouble : Eq<double>
    {
        /// <summary>
        /// Equality test
        /// </summary>
        /// <param name="a">The left hand side of the equality operation</param>
        /// <param name="b">The right hand side of the equality operation</param>
        /// <returns>True if x and y are equal</returns>
        [Pure]
        public static bool Equals(double a, double b) =>
            a.Equals(b);

        /// <summary>
        /// Get hash code of the value
        /// </summary>
        /// <param name="x">Value to get the hash code of</param>
        /// <returns>The hash code of x</returns>
        [Pure]
        public static int GetHashCode(double x) =>
            HashableDouble.GetHashCode(x);
    }
}
