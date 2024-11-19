using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LanguageExt.Traits.Resolve;

public static class OrdResolve<A>
{
    public static string? ResolutionError;
  
    public static Func<A, A, int> CompareFunc = null!;
    
    public static int Compare(A lhs, A rhs) =>
        CompareFunc(lhs, rhs);

    public static bool Exists => 
        ResolutionError is null;

    static OrdResolve()
    {
        var source = typeof(A);

        if(source.IsGenericType && source.GetGenericTypeDefinition() == typeof(K<,>))
        {
            MakeTraitDefault(source);
            return;
        }
        
        if (typeof(Delegate).IsAssignableFrom(source))
        {
            MakeDelegateDefault();
            return;
        }

        MakeComparer(source);
    }
    
    static void MakeComparer(Type source)
    {
        var impl = Resolver.Find(source, "Ord");
        if (impl is null)
        {
            ResolutionError = $"Trait implementation not found for: {source.Name}";
            MakeDefault();
            return;
        }
        
        // Compare
        
        var compareMethod = Resolver.GetStaticPublicMethodWithGivenArguments(impl, "Compare", source, source);
        if (compareMethod is null)
        {
            ResolutionError = $"static `Compare` method not found for: {impl.Name}";
            MakeDefault();
            return;
        }

        
        CompareFunc      = compareMethod.ToFunc2<A,A,int>();
    }
    
    static void MakeDefault()
    {
        CompareFunc      = Comparer<A>.Default.Compare;
    }

    static void MakeDelegateDefault()
    {
        CompareFunc = (x, y) => ((object?)x, (object?)y) switch
                                {
                                    (Delegate dx, Delegate dy) => dx.Method.MetadataToken.CompareTo(dy.Method.MetadataToken),
                                    _                          => -1
                                };
    }

    static void MakeTraitDefault(Type source)
    {
        var gens = source.GetGenericArguments();

        var typeF = gens[0];
        var fname = typeF.FullName ?? string.Empty;
        var tick  = fname.IndexOf('`');
        var iname = tick >= 0 ? fname[..tick] : fname;

        var tgens = typeF.GetGenericArguments();
        var gtype = typeF.Assembly.GetType($"{iname}`{tgens.Length + 1}")!;

        var elementTypeA = gens[1];
        var ngens = tgens.Concat([elementTypeA]).ToArray();
        var type  = gtype.MakeGenericType(ngens);

        var resolver = typeof(OrdResolve<>).MakeGenericType(type);
        var comp = (Delegate)resolver.GetField(nameof(CompareFunc))!.GetValue(null)!;
                      
        // TOOOOOO SLOOOOOOOOW
        CompareFunc     = (x,y) => (int)comp.DynamicInvoke(x,y)!;
    }
}
