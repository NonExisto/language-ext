using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LanguageExt.Traits.Resolve;

internal static class Resolver
{
    public static MethodInfo? GetStaticPublicMethodWithGivenArguments(Type type, string name, params Type[] types) =>
        type.GetMethod(name, BindingFlags.Static | BindingFlags.Public, types);
    
    public static Type? Find(Type elementType, string prefix = "")
    {
        var typeName = $"{prefix}{elementType.Name}";

        var typeByName = FindType(elementType.Assembly, typeName);
        if (typeByName is not null) return MakeGeneric(typeByName, elementType);
        var typeAsmName = elementType.Assembly.GetName();
        
        foreach (var name in GetAssemblies().Where(asm => asm != typeAsmName))
        {
            typeByName = FindType(LoadAssembly(name), typeName);
            if (typeByName != null) return MakeGeneric(typeByName, elementType);
        }
        return null;
    }

    static Type MakeGeneric(Type generic, Type elementType) =>
        generic.IsGenericType
            ? generic.MakeGenericType(elementType.IsGenericType ? elementType.GetGenericArguments() : [elementType])
            : generic;

    static TypeInfo? FindType(Assembly? asm, string name)
    {
        if (asm is null) return null;
        var types = asm.DefinedTypes
                       .Where(t => t.IsClass || t.IsValueType)
                       .Where(t => t.Name == name)
                       .ToArray();

        return types.Length switch
               {
                   0 => null,
                   1 => types[0],
                   _ => null
               };
    }

    static Assembly? LoadAssembly(AssemblyName name)
    {
        try
        {
            return Assembly.Load(name);
        }
        catch
        {
            return null;
        }
    }

    static IEnumerable<AssemblyName> GetAssemblies()
    {
        var asmNames = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                               .Concat(Assembly.GetCallingAssembly().GetReferencedAssemblies())
                               .Concat(Assembly.GetEntryAssembly()?.GetReferencedAssemblies() ?? [])
                               .Distinct();

        var init = new[]
                   {
                       Assembly.GetExecutingAssembly().GetName(),
                       Assembly.GetCallingAssembly().GetName(),
                       Assembly.GetEntryAssembly()?.GetName()
                   };

        foreach (var asm in init.Concat(asmNames).Where(n => n is not null).Distinct())
        {
            yield return asm!;
        }
    }
}
