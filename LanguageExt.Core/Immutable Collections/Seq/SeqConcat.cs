using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LanguageExt.Common;
using LanguageExt.UnsafeValueAccess;

namespace LanguageExt;

internal sealed class SeqConcat<A>(Seq<SeqInternal<A>> ms) : SeqInternal<A>
{
    internal readonly Seq<SeqInternal<A>> ms = ms;
    int selfHash;

    public override A this[int index]
    {
        get
        {
            var r = At(index);
            if (r.IsSome) return r.Value!;
            throw new IndexOutOfRangeException();
        }
    }

    public override Option<A> At(int index)
    {
        if (index < 0) return default;
        var ms1 = ms;
        while (!ms1.IsEmpty)
        {
            var head = ms1.Head.ValueUnsafe() ?? throw new InvalidOperationException();
            var r    = head.At(index);
            if (r.IsSome) return r;
            index -= head.Count;
            ms1 = ms1.Tail;
        }
        return default;
    }

    public override SeqType Type =>
        SeqType.Concat;

    public override A Head 
    {
        get 
        {
            foreach (var s in ms)
            {
                foreach (var a in s)
                {
                    return a;
                }
            } 
            throw Exceptions.SequenceEmpty;
        }
    }

    public override SeqInternal<A> Tail =>
        new SeqLazy<A>(Skip(1));

    public override bool IsEmpty => 
        ms.ForAll(s => s.IsEmpty);

    public override SeqInternal<A> Init
    {
        get
        {
            var take = Count - 1;
            return take <= 0
                       ? SeqEmptyInternal<A>.Default
                       : Take(take);
        }
    }

    public override A Last
    {
        get 
        {
            foreach (var s in ms.Reverse())
            {
                foreach (var a in s.Reverse())
                {
                    return a;
                }
            } 
            throw Exceptions.SequenceEmpty;
        }
    }

    public override int Count => 
        ms.Sum(s => s.Count);

    public SeqConcat<A> AddSeq(SeqInternal<A> ma) =>
        new (ms.Add(ma));

    public SeqConcat<A> AddSeqRange(Seq<SeqInternal<A>> ma) =>
        new (ms.Concat(ma));

    public SeqConcat<A> ConsSeq(SeqInternal<A> ma) =>
        new (ma.Cons(ms));

    public override SeqInternal<A> Add(A value)
    {
        var last = ms.Last.ValueUnsafe()?.Add(value) ?? throw new NotSupportedException();
        return new SeqConcat<A>(ms.Take(ms.Count - 1).Add(last));
    }

    public override SeqInternal<A> Cons(A value)
    {
        var head = ms.Head.ValueUnsafe()?.Cons(value) ?? throw new NotSupportedException();
        return new SeqConcat<A>(head.Cons(ms.Skip(1)));
    }

    public override bool Exists(Func<A, bool> f) =>
        ms.Exists(s => s.Exists(f));
        
    public override S Fold<S>(S state, Func<S, A, S> f) =>
        ms.Fold(state, (s, x) => x.Fold(s, f));

    public override S FoldBack<S>(S state, Func<S, A, S> f) =>
        ms.FoldBack(state, (s, x) => x.FoldBack(s, f));

    public override bool ForAll(Func<A, bool> f) =>
        ms.ForAll(s => s.ForAll(f));

    public override IEnumerator<A> GetEnumerator()
    {
        foreach(var s in ms)
        {
            foreach(var a in s)
            {
                yield return a;
            }
        }
    }

    public override Unit Iter(Action<A> f)
    {
        foreach (var s in ms)
        {
            foreach (var a in s)
            {
                f(a);
            }
        }
        return default;
    }

    public override SeqInternal<A> Skip(int amount) =>
        new SeqLazy<A>(((IEnumerable<A>)this).Skip(amount));

    public override SeqInternal<A> Strict()
    {
        foreach(var s in ms)
        {
            s.Strict();
        }
        return this;
    }

    public override SeqInternal<A> Take(int amount)
    {
        IEnumerable<A> Yield()
        {
            using var iter = GetEnumerator();
            for(; amount > 0 && iter.MoveNext(); amount--)
            {
                yield return iter.Current;
            }
        }
        return new SeqLazy<A>(Yield());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() =>
        selfHash == 0
            ? selfHash = GetHashCode(FNV32.OffsetBasis)
            : selfHash;        

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode(int hash)
    {
        foreach (var seq in ms)
        {
            hash = seq.GetHashCode(hash);
        }
        return hash;
    }
}
