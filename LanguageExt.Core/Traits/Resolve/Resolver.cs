﻿using System;
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
    
    public static MethodInfo? GetHashCodeMethod(Type type)
    {
        Type[] traits = [typeof(HashableResolve<>), typeof(EqResolve<>), typeof(OrdResolve<>)];
        foreach (var trait in traits)
        {
            var impl   = trait.MakeGenericType(type);
            var exists = (bool?)impl.GetProperty("Exists")?.GetValue(null) ?? false;
            if (exists)
            {
                var method = impl.GetMethod("GetHashCode", BindingFlags.Static | BindingFlags.Public, [type]);
                if (method is not null) return method;
            }
        }
        return null;
    }

    public static MethodInfo GetHashCodeMethodAlways(Type type)
    {
        Type[] traits = [typeof(HashableResolve<>), typeof(EqResolve<>), typeof(OrdResolve<>)];
        foreach (var trait in traits)
        {
            var impl   = trait.MakeGenericType(type);
            var exists = (bool?)impl.GetProperty("Exists")?.GetValue(null) ?? false;
            if (exists)
            {
                var method = impl.GetMethod("GetHashCode", BindingFlags.Static | BindingFlags.Public, [type]);
                if (method is not null) return method;
            }
        }
        
        var impl2   = typeof(HashableResolve<>).MakeGenericType(type);
        return impl2.GetMethod("GetHashCode", BindingFlags.Static | BindingFlags.Public, [type]) ?? throw new InvalidOperationException();
    }

    public static MethodInfo? GetEqualsMethod(Type type)
    {
        Type[] traits = [typeof(EqResolve<>), typeof(OrdResolve<>)];
        foreach (var trait in traits)
        {
            var impl   = trait.MakeGenericType(type);
            var exists = (bool?)impl.GetProperty("Exists")?.GetValue(null) ?? false;
            if (exists)
            {
                var method = impl.GetMethod("Equals", BindingFlags.Static | BindingFlags.Public, [type, type]);
                if (method is not null) return method;
            }
        }
        return null;
    }

    public static MethodInfo GetEqualsMethodAlways(Type type)
    {
        Type[] traits = [typeof(EqResolve<>), typeof(OrdResolve<>)];
        foreach (var trait in traits)
        {
            var impl   = trait.MakeGenericType(type);
            var exists = (bool?)impl.GetProperty("Exists")?.GetValue(null) ?? false;
            if (exists)
            {
                var method = impl.GetMethod("Equals", BindingFlags.Static | BindingFlags.Public, [type, type]);
                if (method is not null) return method;
            }
        }
        
        var impl2 = typeof(EqResolve<>).MakeGenericType(type);
        return impl2.GetMethod("Equals", BindingFlags.Static | BindingFlags.Public, [type, type]) ?? throw new InvalidOperationException();
    }
    
    public static MethodInfo? GetCompareMethod(Type type)
    {
        Type[] traits = [typeof(OrdResolve<>)];
        foreach (var trait in traits)
        {
            var impl   = trait.MakeGenericType(type);
            var exists = (bool?)impl.GetProperty("Exists")?.GetValue(null) ?? false;
            if (exists)
            {
                var method = impl.GetMethod("Compare", BindingFlags.Static | BindingFlags.Public, [type, type]);
                if (method is not null) return method;
            }
        }
        return null;
    }    
    
    public static MethodInfo GetCompareMethodAlways(Type type)
    {
        var impl   = typeof(OrdResolve<>).MakeGenericType(type);
        var method = impl.GetMethod("Compare", BindingFlags.Static | BindingFlags.Public, [type, type]);
        return method ?? throw new InvalidOperationException();
    }    
}
