﻿using System;
using LanguageExt.Traits;

namespace LanguageExt;

public static partial class Prelude
{
    /// <summary>
    /// Functor map operation
    /// </summary>
    /// <remarks>
    /// Unwraps the value within the functor, passes it to the map function `f` provided, and
    /// then takes the mapped value and wraps it back up into a new functor.
    /// </remarks>
    /// <param name="ma">Functor to map</param>
    /// <param name="f">Mapping function</param>
    /// <returns>Mapped functor</returns>
    public static HashMap<Key, B> map<Key, A, B>(Func<A, B> f, K<HashMap<Key>, A> ma) =>
        Functor.map(f, ma).As();
    
}    
