using System;
using LanguageExt.ClassInstances;

namespace LanguageExt;

/// <summary>
/// Entry added to a collection
/// </summary>
/// <typeparam name="A">Value type</typeparam>
public sealed class EntryAdded<A> :
    Change<A>, 
    IEquatable<EntryAdded<A>>
{
    /// <summary>
    /// Value that has been added
    /// </summary>
    public readonly A Value;

    internal EntryAdded(A value) =>
        Value = value;

    public override bool Equals(Change<A>? other) =>
        other is EntryAdded<A> rhs && Equals(rhs);

    public bool Equals(EntryAdded<A>? other) =>
        other is not null &&
        EqDefault<A>.Equals(Value, other.Value);

    public override int GetHashCode() =>
        Value?.GetHashCode() ?? FNV32.OffsetBasis;

    public void Deconstruct(out A value) =>
        value = Value;

    public override string ToString() => $"+{Value}";

    public override bool Equals(object? obj) => 
        Equals(obj as EntryAdded<A>);
}
