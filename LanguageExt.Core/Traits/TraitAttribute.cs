using System;

namespace LanguageExt.Attributes;

[AttributeUsage(AttributeTargets.Interface)]
public sealed class TraitAttribute : Attribute
{
    public string NameFormat{get;}
    public TraitAttribute(string nameFormat) =>
        NameFormat = nameFormat;
}
