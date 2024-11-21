using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LanguageExt;

internal enum SeqType
{
    Empty,
    Lazy,
    Strict,
    Concat
}

internal abstract class SeqInternal<A> : IReadOnlyCollection<A>
{
    public abstract SeqType Type { get; }
    public abstract A this[int index] { get; }
    public abstract Option<A> At(int index);
    public abstract SeqInternal<A> Add(A value);
    public abstract SeqInternal<A> Cons(A value);
    public abstract A Head { get; }
    public abstract SeqInternal<A> Tail { get; }
    [MemberNotNullWhen(false, nameof(Head))]
    [MemberNotNullWhen(false, nameof(Last))]
    public abstract bool IsEmpty { get; }
    public abstract SeqInternal<A> Init { get; }
    public abstract A Last { get; }
    public abstract int Count { get; }
    public abstract S Fold<S>(S state, Func<S, A, S> f);
    public abstract S FoldBack<S>(S state, Func<S, A, S> f);
    public abstract SeqInternal<A> Skip(int amount);
    public abstract SeqInternal<A> Take(int amount);
    public abstract SeqInternal<A> Strict();
    public abstract Unit Iter(Action<A> f);
    public abstract bool Exists(Func<A, bool> f);
    public abstract bool ForAll(Func<A, bool> f);
    public abstract int GetHashCode(int offsetBasis);
    public abstract IEnumerator<A> GetEnumerator();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
