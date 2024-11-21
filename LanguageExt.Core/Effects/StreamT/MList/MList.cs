using System;
using LanguageExt.Traits;

namespace LanguageExt;

public abstract record MList<A> : K<MList, A>
{
    public abstract MList<B> Map<B>(Func<A, B> f);

    public readonly static MList<A> Nil = new MNil<A>();
    
    public static MList<A> Cons<M>(A head, Func<K<M, MList<A>>> tail) 
        where M : Monad<M> => 
        new MCons<M, A>(head, tail);

    public K<M, MList<A>> Append<M>(K<M, MList<A>> ys)
        where M : Monad<M> =>
        this switch
        {
            MNil<A>                     => ys,
            MCons<M, A> (var h, var t)  => M.Pure(Cons(h, () => t().Append(ys))),
            _                           => throw new NotSupportedException()
        };
}

public sealed record MNil<A> : MList<A>
{
    public override MList<B> Map<B>(Func<A, B> f) => MList<B>.Nil;
}

public sealed record MCons<M, A>(A Head, Func<K<M, MList<A>>> Tail) : MList<A>
    where M : Monad<M>
{
    public override MList<B> Map<B>(Func<A, B> f) => 
        new MCons<M, B>(f(Head), () => Tail().Map(l => l.Map(f)));
}
