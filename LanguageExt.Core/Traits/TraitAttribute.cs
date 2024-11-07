using System;

namespace LanguageExt.Attributes;

[AttributeUsage(AttributeTargets.Interface)]
public sealed class TraitAttribute : Attribute
{
    public readonly string NameFormat;
    public TraitAttribute(string nameFormat) =>
        NameFormat = nameFormat;
}
