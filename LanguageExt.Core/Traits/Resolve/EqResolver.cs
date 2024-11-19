using System;
using System.Collections.Generic;

namespace LanguageExt.Traits.Resolve;

public static class EqResolve<A>
{
    public static string? ResolutionError;

    public static Func<A, A, bool> EqualsFunc = null!;

    public static bool Equals(A lhs, A rhs) =>
        EqualsFunc(lhs, rhs);

    public static bool Exists => 
        ResolutionError is null;

    static EqResolve()
    {
        var source = typeof(A);
        
        var impl = Resolver.Find(source, "Eq");
        if (impl is null)
        {
            ResolutionError = $"Trait implementation not found for: {typeof(A).Name}";
            MakeDefault();
            return;
        }
        
        // Equals
        
        var equalsMethod = Resolver.GetStaticPublicMethodWithGivenArguments(impl, "Equals", source, source);
        if (equalsMethod is null)
        {
            ResolutionError = $"static `Equals` method not found for: {impl.Name}";
            MakeDefault();
            return;
        }
        EqualsFunc      = equalsMethod.ToFunc2<A, A, bool>();
    }

    static void MakeDefault()
    {
        EqualsFunc      = EqualityComparer<A>.Default.Equals;
    }
}
