using FluentAssertions;
using Xunit;

namespace LanguageExt.Tests;

public class AtomMetadataTests
{
		[Fact]
    public void ConstructAndSwap()
    {
        var atom = Atom(5, Set("A", "B", "C"));

        atom.Swap((n, old) => old.Add("D"));
        atom.Swap((n, old) => old.Add("E"));
        atom.Swap((n, old) => old.Add("F"));

        Assert.True(atom == Set("A", "B", "C", "D", "E", "F"));
    }

    

    [Fact]
    public void AtomShouldFeedChangesOnlyOnDifference()
    {
        var atom = Atom(-5, 5);
        var count = 0;
        atom.Change += (ch) => count++;

        atom.Swap((n, old) => 6);
        atom.Swap((n, old) => 6);
        atom.Swap((n, old) => 7);
        atom.Swap((n, old) => None);
        atom.Swap((n, old) => 7);

        atom.Value.Should().Be(7);
        count.Should().Be(2);
    }
}
