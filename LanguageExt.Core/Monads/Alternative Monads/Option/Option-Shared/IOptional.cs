using System;

namespace LanguageExt;

public interface IOptional
{
    bool IsSome { get; }

    bool IsNone { get; }

    R MatchUntyped<R>(Func<object?, R> Some, Func<R> None);

    Type GetUnderlyingType();
}
