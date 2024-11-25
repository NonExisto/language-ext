using System;
using LanguageExt.Traits;

namespace LanguageExt;

/// <summary>
/// State put
/// </summary>
/// <remarks>
/// This is a convenience type that is created by the Prelude `put` function.  It avoids
/// the need for lots of generic parameters when used in `StateT` and `State` based
/// monads.
/// </remarks>
/// <typeparam name="S">State type</typeparam>
public readonly record struct Put<S>(S Value)
{
    /// <summary>
    /// Convert to a `Stateful`
    /// </summary>
    public K<M, Unit> ToStateful<M>()
        where M : Stateful<M, S> =>
        Stateful.put<M, S>(Value);
    
    /// <summary>
    /// Convert to a `StateT`
    /// </summary>
    public StateT<S, M, Unit> ToStateT<M>()
        where M : Monad<M>, Choice<M> =>
        Stateful.put<StateT<S, M>, S>(Value).As();
    
    /// <summary>
    /// Convert to a `State`
    /// </summary>
    public State<S, Unit> ToState() =>
        Stateful.put<State<S>, S>(Value).As();

    /// <summary>
    /// Monadic bind
    /// </summary>
    public StateT<S, M, C> SelectMany<M, B, C>(Func<Unit, StateT<S, M, B>> bind, Func<Unit, B, C> project)
        where M : Monad<M>, Choice<M> =>
        ToStateT<M>().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public StateT<S, M, C> SelectMany<M, B, C>(Func<Unit, K<StateT<S, M>, B>> bind, Func<Unit, B, C> project)
        where M : Monad<M>, Choice<M> =>
        ToStateT<M>().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public State<S, C> SelectMany<B, C>(Func<Unit, State<S, B>> bind, Func<Unit, B, C> project) =>
        ToState().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public State<S, C> SelectMany<B, C>(Func<Unit, K<State<S>, B>> bind, Func<Unit, B, C> project) =>
        ToState().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public RWST<R, W, S, M, C> SelectMany<R, W, M, B, C>(Func<Unit, RWST<R, W, S, M, B>> bind, Func<Unit, B, C> project) 
        where W : Monoid<W>
        where M : Stateful<M, S>, Monad<M>, Choice<M> =>
        ToStateful<RWST<R, W, S, M>>().SelectMany(bind, project).As();

    /// <summary>
    /// Monadic bind
    /// </summary>
    public RWST<R, W, S, M, C> SelectMany<R, W, M, B, C>(Func<Unit, K<RWST<R, W, S, M>, B>> bind, Func<Unit, B, C> project) 
        where W : Monoid<W>
        where M : Stateful<M, S>, Monad<M>, Choice<M> =>
        ToStateful<RWST<R, W, S, M>>().SelectMany(bind, project).As();
}

/// <summary>
/// State modify
/// </summary>
/// <remarks>
/// This is a convenience type that is created by the Prelude `modify` function.  It avoids
/// the need for lots of generic parameters when used in `StateT` and `State` based
/// monads.
/// </remarks>
/// <param name="f">Mapping from the environment</param>
/// <typeparam name="S">State type</typeparam>
public readonly record struct Modify<S>(Func<S, S> f)
{
    /// <summary>
    /// Convert with `Stateful`
    /// </summary>
    public K<M, Unit> ToStateful<M>()
        where M : Stateful<M, S> =>
        Stateful.modify<M, S>(f);
    
    /// <summary>
    /// Convert to a `StateT`
    /// </summary>
    public StateT<S, M, Unit> ToStateT<M>()
        where M : Monad<M>, Choice<M> =>
        StateT<S, M, Unit>.Modify(f);
    
    /// <summary>
    /// Convert to a `State`
    /// </summary>
    public State<S, Unit> ToState() =>
        State<S, Unit>.Modify(f);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public StateT<S, M, C> SelectMany<M, B, C>(Func<Unit, StateT<S, M, B>> bind, Func<Unit, B, C> project)
        where M : Monad<M>, Choice<M> =>
        ToStateT<M>().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public StateT<S, M, C> SelectMany<M, B, C>(Func<Unit, K<StateT<S, M>, B>> bind, Func<Unit, B, C> project)
        where M : Monad<M>, Choice<M> =>
        ToStateT<M>().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public State<S, C> SelectMany<B, C>(Func<Unit, State<S, B>> bind, Func<Unit, B, C> project) =>
        ToState().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public State<S, C> SelectMany<B, C>(Func<Unit, K<State<S>, B>> bind, Func<Unit, B, C> project) =>
        ToState().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public RWST<R, W, S, M, C> SelectMany<R, W, M, B, C>(Func<Unit, RWST<R, W, S, M, B>> bind, Func<Unit, B, C> project) 
        where W : Monoid<W>
        where M : Stateful<M, S>, Monad<M>, Choice<M> =>
        ToStateful<RWST<R, W, S, M>>().SelectMany(bind, project).As();

    /// <summary>
    /// Monadic bind
    /// </summary>
    public RWST<R, W, S, M, C> SelectMany<R, W, M, B, C>(Func<Unit, K<RWST<R, W, S, M>, B>> bind, Func<Unit, B, C> project) 
        where W : Monoid<W>
        where M : Stateful<M, S>, Monad<M>, Choice<M> =>
        ToStateful<RWST<R, W, S, M>>().SelectMany(bind, project).As();
}


/// <summary>
/// State modify
/// </summary>
/// <remarks>
/// This is a convenience type that is created by the Prelude `modify` function.  It avoids
/// the need for lots of generic parameters when used in `StateT` and `State` based
/// monads.
/// </remarks>
/// <param name="f">Mapping from the environment</param>
/// <typeparam name="A">Source bound value type</typeparam>
/// <typeparam name="S">State type</typeparam>
public readonly record struct Gets<S, A>(Func<S, A> f)
{
    /// <summary>
    /// Convert with `Stateful`
    /// </summary>
    public K<M, A> ToStateful<M>()
        where M : Stateful<M, S> =>
        Stateful.gets<M, S, A>(f);
    
    /// <summary>
    /// Convert ot a `StateT`
    /// </summary>
    public StateT<S, M, A> ToStateT<M>()
        where M : Monad<M>, Choice<M> =>
        StateT<S, M, A>.Gets(f);
    
    /// <summary>
    /// Convert ot a `State`
    /// </summary>
    public State<S, A> ToState() =>
        State<S, A>.Gets(f);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public StateT<S, M, C> SelectMany<M, B, C>(Func<A, StateT<S, M, B>> bind, Func<A, B, C> project)
        where M : Monad<M>, Choice<M> =>
        ToStateT<M>().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public StateT<S, M, C> SelectMany<M, B, C>(Func<A, K<StateT<S, M>, B>> bind, Func<A, B, C> project)
        where M : Monad<M>, Choice<M> =>
        ToStateT<M>().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public State<S, C> SelectMany<B, C>(Func<A, State<S, B>> bind, Func<A, B, C> project) =>
        ToState().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public State<S, C> SelectMany<B, C>(Func<A, K<State<S>, B>> bind, Func<A, B, C> project) =>
        ToState().SelectMany(bind, project);

    /// <summary>
    /// Monadic bind
    /// </summary>
    public RWST<R, W, S, M, C> SelectMany<R, W, M, B, C>(Func<A, RWST<R, W, S, M, B>> bind, Func<A, B, C> project) 
        where W : Monoid<W>
        where M : Stateful<M, S>, Monad<M>, Choice<M> =>
        ToStateful<RWST<R, W, S, M>>().SelectMany(bind, project).As();

    /// <summary>
    /// Monadic bind
    /// </summary>
    public RWST<R, W, S, M, C> SelectMany<R, W, M, B, C>(Func<A, K<RWST<R, W, S, M>, B>> bind, Func<A, B, C> project) 
        where W : Monoid<W>
        where M : Stateful<M, S>, Monad<M>, Choice<M> =>
        ToStateful<RWST<R, W, S, M>>().SelectMany(bind, project).As();
}
