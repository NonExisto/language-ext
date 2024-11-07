namespace LanguageExt;

public abstract record IOResponse<A>;
public sealed record CompleteIO<A>(A Value) : IOResponse<A>;
public sealed record RecurseIO<A>(IO<A> Computation) : IOResponse<A>;

public static class IOResponse
{
    public static IOResponse<A> Complete<A>(A value) => new CompleteIO<A>(value);
    public static IOResponse<A> Recurse<A>(IO<A> computation) => new RecurseIO<A>(computation);
}
