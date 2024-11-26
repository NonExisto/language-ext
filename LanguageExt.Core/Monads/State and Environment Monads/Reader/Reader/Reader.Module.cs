﻿using System;

namespace LanguageExt;

public sealed partial class Reader
{
    public static Reader<Env, A> pure<Env, A>(A value) =>  
        Reader<Env, A>.Pure(value);

    public static Reader<Env, Env> ask<Env>() => 
        Reader<Env, Env>.Asks(Prelude.identity);

    public static Reader<Env, A> asks<Env, A>(Func<Env, A> f) =>  
        Reader<Env, A>.Asks(f);

    public static Reader<Env, A> asksM<Env, A>(Func<Env, Reader<Env, A>> f) =>
        Reader<Env, A>.AsksM(f);

    public static Reader<Env, A> local<Env, A>(Func<Env, Env> f, Reader<Env, A> ma) => 
        ma.As().Local(f);
}
