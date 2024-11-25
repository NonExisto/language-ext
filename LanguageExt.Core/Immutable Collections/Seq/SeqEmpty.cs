namespace LanguageExt;

/// <summary>
/// A unit type that represents `Seq.Empty`.  This type can be implicitly
/// converted to <see cref="Seq{A}"/>.
/// </summary>
public readonly struct SeqEmpty : System.IEquatable<SeqEmpty>
{
    public static readonly SeqEmpty Default;

    public override bool Equals(object? obj) => obj is SeqEmpty;

    public override int GetHashCode() => -7;

    public static bool operator ==(SeqEmpty left, SeqEmpty right) => true;

    public static bool operator !=(SeqEmpty left, SeqEmpty right) => false;

    public bool Equals(SeqEmpty other) => true;
}
