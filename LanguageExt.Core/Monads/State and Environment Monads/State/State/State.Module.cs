﻿using System;

namespace LanguageExt;

/// <summary>
/// `MonadStateT` trait implementation for `StateT` 
/// </summary>
/// <typeparam name="S">State environment type</typeparam>
public partial class State<S>
{
    public static State<S, A> pure<A>(A value) => 
        State<S, A>.Pure(value);
}

/// <summary>
/// `MonadStateT` trait implementation for `StateT` 
/// </summary>
public sealed class State
{
    public static State<S, A> pure<S, A>(A value) =>  
        State<S, A>.Pure(value);

    public static State<S, S> get<S>() => 
        State<S, S>.Get;
    
    public static State<S, A> gets<S, A>(Func<S, A> f) => 
        State<S, A>.Gets(f);
    
    public static State<S, A> getsM<S, A>(Func<S, State<S, A>> f) => 
        State<S, A>.GetsM(f);

    public static State<S, Unit> put<S>(S state) =>  
        State<S, Unit>.Put(state);

    public static State<S, Unit> modify<S>(Func<S, S> f) =>  
        State<S, Unit>.Modify(f);
}
