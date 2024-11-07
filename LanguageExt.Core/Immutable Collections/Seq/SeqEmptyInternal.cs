using System;
using System.Collections.Generic;
using LanguageExt.Common;

namespace LanguageExt;

internal sealed class SeqEmptyInternal<A> : SeqInternal<A>, IEnumerable<A>
{
    public static readonly SeqInternal<A> Default = new SeqEmptyInternal<A>();

    public override A this[int index] => 
        throw new IndexOutOfRangeException();

    public override Option<A> At(int index) => 
        default;

    public override A Head =>
        throw Exceptions.SequenceEmpty;

    public override SeqInternal<A> Tail =>
        this;

    public override bool IsEmpty => 
        true;

    public override SeqInternal<A> Init =>
        this;

    public override A Last =>
        throw Exceptions.SequenceEmpty;

    public override int Count => 
        0;

    public override SeqInternal<A> Add(A value) =>
        SeqStrict<A>.FromSingleValue(value);

    public override SeqInternal<A> Cons(A value) =>
        SeqStrict<A>.FromSingleValue(value);

    public override S Fold<S>(S state, Func<S, A, S> f) =>
        state;

    public override S FoldBack<S>(S state, Func<S, A, S> f) =>
        state;

    public override SeqInternal<A> Skip(int amount) =>
        this;

    public override SeqInternal<A> Strict() =>
        this;

    public override SeqInternal<A> Take(int amount) =>
        this;
        
    public override IEnumerator<A> GetEnumerator()
    {
        yield break;
    }

    public override Unit Iter(Action<A> f) =>
        default;

    public override bool Exists(Func<A, bool> f) => 
        false;

    public override bool ForAll(Func<A, bool> f) =>
        true;

    public override SeqType Type => SeqType.Empty;

    public override int GetHashCode() =>
        FNV32.OffsetBasis;

    public override int GetHashCode(int offsetBasis) =>
        offsetBasis;
}
