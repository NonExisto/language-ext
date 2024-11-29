using System.Linq;
using System.Reflection;

namespace LanguageExt
{
    static class Reflect
    {
        public static Option<ConstructorInfo> GetConstructor<TYPE>() =>
            typeof(TYPE)
                .GetTypeInfo()
                .DeclaredConstructors.FirstOrDefault(static x => !x.IsStatic && x.GetParameters().Length == 0);

        public static Option<ConstructorInfo> GetConstructor<TYPE, A>() =>
        (   
            from ctor in typeof(TYPE).GetTypeInfo().DeclaredConstructors
            let args = ctor.GetParameters()
            where !ctor.IsStatic && args.Length == 1
                && args[0].ParameterType == typeof(A)
            select ctor
        ).FirstOrDefault();
            

        public static Option<ConstructorInfo> GetConstructor<TYPE, A, B>() =>
        (   
            from ctor in typeof(TYPE).GetTypeInfo().DeclaredConstructors
            let args = ctor.GetParameters()
            where !ctor.IsStatic && args.Length == 2
                && args[0].ParameterType == typeof(A)
                && args[1].ParameterType == typeof(B)
            select ctor
        ).FirstOrDefault();

        public static Option<ConstructorInfo> GetConstructor<TYPE, A, B, C>() =>
        (   
            from ctor in typeof(TYPE).GetTypeInfo().DeclaredConstructors
            let args = ctor.GetParameters()
            where !ctor.IsStatic && args.Length == 3
                && args[0].ParameterType == typeof(A)
                && args[1].ParameterType == typeof(B)
                && args[2].ParameterType == typeof(C)
            select ctor
        ).FirstOrDefault();

        public static Option<ConstructorInfo> GetConstructor<TYPE, A, B, C, D>() =>
        (   
            from ctor in typeof(TYPE).GetTypeInfo().DeclaredConstructors
            let args = ctor.GetParameters()
            where !ctor.IsStatic && args.Length == 4
                && args[0].ParameterType == typeof(A)
                && args[1].ParameterType == typeof(B)
                && args[2].ParameterType == typeof(C)
                && args[3].ParameterType == typeof(D)
            select ctor
        ).FirstOrDefault();
    }
}
