using System;

namespace LanguageExt.Traits.Resolve;

public static class HashableResolve<A>
{
    public static string? ResolutionError;

    public static Func<A, int> GetHashCodeFunc = null!;
    
    public static int GetHashCode(A value) =>
        GetHashCodeFunc(value);

    public static bool Exists => 
        ResolutionError is null;
    
    static HashableResolve()
    {
        var source = typeof(A);
        
        var impl = Resolver.Find(source, "Hashable");
        if (impl is null)
        {
            ResolutionError = $"Trait implementation not found for: {typeof(A).Name}";
            MakeDefault();
            return;
        }
        
        var method = Resolver.GetStaticPublicMethodWithGivenArguments(impl, "GetHashCode", source);
        if (method is null)
        {
            ResolutionError = $"static `GetHashCode` method not found for: {impl.Name}";
            MakeDefault();
            return;
        }

        
        GetHashCodeFunc      = method.ToFunc1<A,int>();
    }
    
    static void MakeDefault()
    {
        GetHashCodeFunc      = DefaultGetHashCode;
    }

    static int DefaultGetHashCode(A value) =>
        value is null ? 0 : value.GetHashCode();
}
