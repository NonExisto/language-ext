using LanguageExt.Traits;

namespace LanguageExt;
public static class CompositionsExt
{
    public static Compositions<A> Cons<A>(this A a, Compositions<A> ma) where A : Monoid<A> =>
        Compositions.cons(a, ma);
}
