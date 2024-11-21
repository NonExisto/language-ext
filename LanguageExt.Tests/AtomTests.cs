using System.Linq;
using FluentAssertions;
using Xunit;

namespace LanguageExt.Tests;

public class AtomTests
{
    [Fact]
    public void ConstructAndSwap()
    {
        var atom = Atom(Set("A", "B", "C"));

        atom.Swap(old => old.Add("D"));
        atom.Swap(old => old.Add("E"));
        atom.Swap(old => old.Add("F"));

        Assert.True(atom == Set("A", "B", "C", "D", "E", "F"));
    }

    [Fact]
    public void AtomSeqEnumeration()
    {
        var xs   = Seq(1,2,3,4);
        var atom = AtomSeq(xs);
            
        Assert.Equal(atom.Sum(), xs.Sum());
    }

    [Fact]
    public void AtomShouldFeedChangesOnlyOnDifference()
    {
        var atom = Atom(5);
        var count = 0;
        atom.Change += (ch) => count++;

        atom.Swap(old => 6);
        atom.Swap(old => 6);
        atom.Swap(old => 7);
        atom.Swap(old => None);
        atom.Swap(old => 7);

        atom.Value.Should().Be(7);
        count.Should().Be(2);
    }
}
