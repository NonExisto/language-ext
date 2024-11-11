﻿using System;
using System.Threading;
using static LanguageExt.PreludeRx;

using Xunit;

namespace LanguageExt.Tests;

public class DelayTests
{
    [Fact]
    public void DelayTest1()
    {
        var span = TimeSpan.FromMilliseconds(500);
        var till = DateTime.Now.Add(span);
        var v    = 0;

        delay(() => 1, span).Subscribe(x => v = x);

        while( DateTime.Now < till )
        {
            Assert.Equal(0, v);
            Thread.Sleep(10);
        }

        while (DateTime.Now < till.AddMilliseconds(200))
        {
            Thread.Sleep(10);
        }

        Assert.Equal(1, v);
    }
}
