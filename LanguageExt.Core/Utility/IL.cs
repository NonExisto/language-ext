using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static LanguageExt.Prelude;
using static LanguageExt.Reflect;

namespace LanguageExt;

public static class IL
{
    /// <summary>
    /// Emits the IL to instantiate a type of R with a single argument to 
    /// the constructor
    /// </summary>
    public static Func<R> Ctor<R>()
    {
        var ctorInfo = GetConstructor<R>()
           .IfNone(() => throw new ArgumentException($"Constructor not found for type {typeof(R).FullName}"));

        var expr = Expression.New(ctorInfo);
        var lambda = Expression.Lambda<Func<R>>(expr);
        return lambda.Compile();
    }

    /// <summary>
    /// Emits the IL to instantiate a type of R with a single argument to 
    /// the constructor
    /// </summary>
    public static Func<A, R> Ctor<A, R>()
    {
        var ctorInfo = GetConstructor<R, A>()
           .IfNone(() => throw new ArgumentException($"Constructor not found for type {typeof(R).FullName}"));

        var ctorParams = ctorInfo.GetParameters();


        var arg0 = Expression.Parameter(typeof(A), "arg0");
        var expr = Expression.New(ctorInfo, arg0);
        var lambda = Expression.Lambda<Func<A, R>>(expr, arg0);
        return lambda.Compile();

    }

    /// <summary>
    /// Emits the IL to instantiate a type of R with two arguments to 
    /// the constructor
    /// </summary>
    public static Func<A, B, R> Ctor<A, B, R>()
    {
        var ctorInfo = GetConstructor<R, A, B>()
           .IfNone(() => throw new ArgumentException($"Constructor not found for type {typeof(R).FullName}"));

        var ctorParams = ctorInfo.GetParameters();


        var arg0 = Expression.Parameter(typeof(A), "arg0");
        var arg1 = Expression.Parameter(typeof(B), "arg1");
        var expr = Expression.New(ctorInfo, arg0, arg1);
        var lambda = Expression.Lambda<Func<A, B, R>>(expr, arg0, arg1);
        return lambda.Compile();

    }

    /// <summary>
    /// Emits the IL to instantiate a type of R with three arguments to 
    /// the constructor
    /// </summary>
    public static Func<A, B, C, R> Ctor<A, B, C, R>()
    {
        var ctorInfo = GetConstructor<R, A, B, C>()
           .IfNone(() => throw new ArgumentException($"Constructor not found for type {typeof(R).FullName}"));

        var ctorParams = ctorInfo.GetParameters();


        var arg0 = Expression.Parameter(typeof(A), "arg0");
        var arg1 = Expression.Parameter(typeof(B), "arg1");
        var arg2 = Expression.Parameter(typeof(C), "arg2");
        var expr = Expression.New(ctorInfo, arg0, arg1, arg2);
        var lambda = Expression.Lambda<Func<A, B, C, R>>(expr, arg0, arg1, arg2);
        return lambda.Compile();

    }

    /// <summary>
    /// Emits the IL to instantiate a type of R with four arguments to 
    /// the constructor
    /// </summary>
    public static Func<A, B, C, D, R> Ctor<A, B, C, D, R>()
    {
        var ctorInfo = GetConstructor<R, A, B, C, D>()
           .IfNone(() => throw new ArgumentException($"Constructor not found for type {typeof(R).FullName}"));

        if (ctorInfo == null) throw new ArgumentException($"Constructor not found for type {typeof(R).FullName}");


        var arg0 = Expression.Parameter(typeof(A), "arg0");
        var arg1 = Expression.Parameter(typeof(B), "arg1");
        var arg2 = Expression.Parameter(typeof(C), "arg2");
        var arg3 = Expression.Parameter(typeof(D), "arg3");
        var expr = Expression.New(ctorInfo, arg0, arg1, arg2, arg3);
        var lambda = Expression.Lambda<Func<A, B, C, D, R>>(expr, arg0, arg1, arg2, arg3);
        return lambda.Compile();

    }

    /// <summary>
    /// Emits the IL to invoke a static method
    /// </summary>
    public static Option<Func<object, R>> Func1<TYPE, R>(Type arg1, Func<MethodInfo, bool>? methodPred = null)
    {
        methodPred ??= _ => true;

        var methodInfo = typeof(TYPE)
                        .GetTypeInfo()
                        .GetAllMethods(true)
                        .Where(x =>
                         {
                             if (!x.IsStatic) return false;
                             var ps = x.GetParameters();
                             if (ps.Length != 1) return false;
                             if (ps[0].ParameterType != arg1) return false;
                             return methodPred(x);
                         })
                        .FirstOrDefault();

        if (methodInfo == null) return None;

        var methodParams = methodInfo.GetParameters();


        var larg1 = Expression.Parameter(typeof(object), "arg1");
        var expr = Expression.Call(methodInfo, Expression.Convert(larg1, arg1));
        var lambda = Expression.Lambda<Func<object, R>>(expr, larg1);
        return lambda.Compile();

    }

    /// <summary>
    /// Emits the IL to invoke a static method with one argument
    /// </summary>
    public static Option<Func<A, R>> Func1<TYPE, A, R>(Func<MethodInfo, bool>? methodPred = null)
    {
        methodPred ??= _ => true;

        var methodInfo = typeof(TYPE)
                        .GetTypeInfo()
                        .GetAllMethods(true)
                        .Where(x =>
                        {
                            if (!x.IsStatic) return false;
                            var ps = x.GetParameters();
                            if (ps.Length != 1) return false;
                            if (ps[0].ParameterType != typeof(A)) return false;
                            return methodPred(x);
                        })
                        .FirstOrDefault();

        if (methodInfo == null) return None;

        return ToFunc1<A, R>(methodInfo);

    }

    public static Func<A, R> ToFunc1<A, R>(this MethodInfo methodInfo)
    {
        var larg0 = Expression.Parameter(typeof(A), "arg0");
        var expr = Expression.Call(methodInfo, larg0);
        var lambda = Expression.Lambda<Func<A, R>>(expr, larg0);
        return lambda.Compile();
    }

    /// <summary>
    /// Emits the IL to invoke a static method with two arguments
    /// </summary>
    public static Option<Func<A, B, R>> Func2<TYPE, A, B, R>(Func<MethodInfo, bool>? methodPred = null)
    {
        methodPred ??= _ => true;

        var methodInfo = typeof(TYPE)
                        .GetTypeInfo()
                        .GetAllMethods(true)
                        .Where(x =>
                        {
                            if (!x.IsStatic) return false;
                            var ps = x.GetParameters();
                            if (ps.Length != 2) return false;
                            if (ps[0].ParameterType != typeof(A)) return false;
                            if (ps[1].ParameterType != typeof(B)) return false;
                            return methodPred(x);
                        })
                        .FirstOrDefault();

        if (methodInfo == null) return None;
        return ToFunc2<A, B, R>(methodInfo);

    }

    public static Func<A, B, R> ToFunc2<A, B, R>(this MethodInfo methodInfo)
    {
        var larg0 = Expression.Parameter(typeof(A), "arg0");
        var larg1 = Expression.Parameter(typeof(B), "arg1");
        var expr = Expression.Call(methodInfo, larg0, larg1);
        var lambda = Expression.Lambda<Func<A, B, R>>(expr, larg0, larg1);
        return lambda.Compile();
    }

    /// <summary>
    /// Emits the IL to invoke a static method with three arguments
    /// </summary>
    public static Option<Func<A, B, C, R>> Func3<TYPE, A, B, C, R>(Func<MethodInfo, bool>? methodPred = null)
    {
        methodPred ??= _ => true;

        var methodInfo = typeof(TYPE)
                        .GetTypeInfo()
                        .GetAllMethods(true)
                        .Where(x =>
                        {
                            if (!x.IsStatic) return false;
                            var ps = x.GetParameters();
                            if (ps.Length != 3) return false;
                            if (ps[0].ParameterType != typeof(A)) return false;
                            if (ps[1].ParameterType != typeof(B)) return false;
                            if (ps[2].ParameterType != typeof(C)) return false;
                            return methodPred(x);
                        })
                        .FirstOrDefault();

        if (methodInfo == null) return None;
        return ToFunc3<A, B, C, R>(methodInfo);

    }

    public static Func<A, B, C, R> ToFunc3<A, B, C, R>(this MethodInfo methodInfo)
    {
        var larg0 = Expression.Parameter(typeof(A), "arg0");
        var larg1 = Expression.Parameter(typeof(B), "arg1");
        var larg2 = Expression.Parameter(typeof(C), "arg2");
        var expr = Expression.Call(methodInfo, larg0, larg1, larg2);
        var lambda = Expression.Lambda<Func<A, B, C, R>>(expr, larg0, larg1, larg2);
        return lambda.Compile();
    }

    /// <summary>
    /// Emits the IL to invoke a static method with four arguments
    /// </summary>
    public static Option<Func<A, B, C, D, R>> Func4<TYPE, A, B, C, D, R>(Func<MethodInfo, bool>? methodPred = null)
    {
        methodPred ??= _ => true;

        var methodInfo = typeof(TYPE)
                        .GetTypeInfo()
                        .GetAllMethods(true)
                        .Where(x =>
                        {
                            if (!x.IsStatic) return false;
                            var ps = x.GetParameters();
                            if (ps.Length != 4) return false;
                            if (ps[0].ParameterType != typeof(A)) return false;
                            if (ps[1].ParameterType != typeof(B)) return false;
                            if (ps[2].ParameterType != typeof(C)) return false;
                            if (ps[3].ParameterType != typeof(D)) return false;
                            return methodPred(x);
                        })
                        .FirstOrDefault();

        if (methodInfo == null) return None;
        return ToFunc4<A, B, C, D, R>(methodInfo);

    }

    public static Func<A, B, C, D, R> ToFunc4<A, B, C, D, R>(this MethodInfo methodInfo)
    {
        var larg0 = Expression.Parameter(typeof(A), "arg0");
        var larg1 = Expression.Parameter(typeof(B), "arg1");
        var larg2 = Expression.Parameter(typeof(C), "arg2");
        var larg3 = Expression.Parameter(typeof(D), "arg3");
        var expr = Expression.Call(methodInfo, larg0, larg1, larg2, larg3);
        var lambda = Expression.Lambda<Func<A, B, C, D, R>>(expr, larg0, larg1, larg2, larg3);
        return lambda.Compile();
    }

    public static Func<A, B>? GetPropertyOrField<A, B>(string name) =>
        GetProperty<A, B>(name) ?? GetField<A, B>(name);

    public static Func<A, B>? GetProperty<A, B>(string name)
    {
        var m = typeof(A).GetMethod($"get_{name}");
        if (m == null) return null;
        if (m.ReturnType != typeof(B)) return null;


        var larg0 = Expression.Parameter(typeof(A), "arg0");
        var expr = Expression.Property(larg0, m);
        var lambda = Expression.Lambda<Func<A, B>>(expr, larg0);
        return lambda.Compile();

    }

    public static Func<A, B>? GetField<A, B>(string name)
    {
        var fld = typeof(A).GetField(name);
        if (fld == null) return null;
        if (fld.FieldType != typeof(B)) return null;


        var larg0 = Expression.Parameter(typeof(A), "arg0");
        var expr = Expression.Field(larg0, fld);
        var lambda = Expression.Lambda<Func<A, B>>(expr, larg0);
        return lambda.Compile();

    }
}