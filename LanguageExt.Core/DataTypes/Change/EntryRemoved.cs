using System;
using LanguageExt.ClassInstances;

namespace LanguageExt;

/// <summary>
/// Existing entry removed 
/// </summary>
/// <typeparam name="A">Value type</typeparam>
public sealed class EntryRemoved<A> : 
    Change<A>, 
    IEquatable<EntryRemoved<A>>
{
    /// <summary>
    /// Value that was removed
    /// </summary>
    public readonly A OldValue;

    internal EntryRemoved(A oldValue) =>
        OldValue = oldValue;

    public override bool Equals(Change<A>? other) =>
        other is EntryRemoved<A> rhs && Equals(rhs);

    public override int GetHashCode() => 
        OldValue?.GetHashCode() ?? FNV32.OffsetBasis;
        
    public bool Equals(EntryRemoved<A>? other) =>
        other is not null &&
        EqDefault<A>.Equals(OldValue, other.OldValue);

    public void Deconstruct(out A oldValue)
    {
        oldValue = OldValue;
    }
        
    public override string ToString() => $"-{OldValue}";

    public override bool Equals(object? obj) => 
        Equals(obj as EntryRemoved<A>);
}
