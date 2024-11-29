using System;
using static LanguageExt.PreludeRx;

using Xunit;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;

namespace LanguageExt.Tests;

public class DelayTests
{
    [Fact]
    public async Task DelayTest()
    {
        var span = TimeSpan.FromMilliseconds(200);
        var v    = 0;

        var observable = delay(() => 1, span);
        observable.Subscribe(x => v = x);
        
        Assert.Equal(0, v);
        await Task.Delay(50);
        Assert.Equal(0, v);

        await observable.ToTask();

        Assert.Equal(1, v);
    }
}
