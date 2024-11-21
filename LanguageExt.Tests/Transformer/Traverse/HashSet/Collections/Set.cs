using FluentAssertions;
using Xunit;
using Rint = LanguageExt.Tests.ReverseNumber<int>;

namespace LanguageExt.Tests.Transformer.Traverse.HashSetT.Collections;

public class SetHashSet
{
    [Fact]
    public void EmptyEmptyIsEmptyEmpty()
    {
        Set<HashSet<int>> ma = Empty;

        var mb = ma.Traverse(mx => mx).As();

        var mc = HashSet.singleton(Set<int>.Empty);
        
        Assert.True(mb == mc);
    }
    
    [Fact]
    public void SetHashSetCrossProduct()
    {
        var ma = Set(HashSet(1, 2), HashSet(10, 20, 30));

        var mb = ma.Traverse(mx => mx).As();


        var mc = HashSet(Set(1, 10), Set(1, 20), Set(1, 30), Set(2, 10), Set(2, 20), Set(2, 30));
        
        Assert.True(mb == mc);

        foreach (var set in mb)
        {
            set.Should<int>().BeInAscendingOrder();
        }
    }

    [Fact]
    public void SetHashSetCrossProductReverse()
    {
        var ma = Set(HashSet<Rint>(1, 2), HashSet<Rint>(10, 20, 30));

        var mb = ma.Traverse(mx => mx).As();


        var mc = HashSet(
            Set<Rint>(1, 10), 
            Set<Rint>(1, 20), 
            Set<Rint>(1, 30), 
            Set<Rint>(2, 10), 
            Set<Rint>(2, 20), 
            Set<Rint>(2, 30));
        
        Assert.True(mb == mc);

        foreach (var set in mb)
        {
            set.Select(x => x.Value).Should<int>().BeInDescendingOrder();
        }
    }
    
            
    [Fact]
    public void SetOfEmptiesAndNonEmptiesIsEmpty()
    {
        var ma = Set(HashSet<int>(), HashSet(1, 2, 3));

        var mb = ma.Traverse(mx => mx).As();


        var mc = HashSet<Set<int>>.Empty;
        
        Assert.True(mb == mc);
    }
    
    [Fact]
    public void SetOfEmptiesIsEmpty()
    {
        var ma = Set(HashSet<int>(), HashSet<int>());

        var mb = ma.Traverse(mx => mx).As();


        var mc = HashSet<Set<int>>.Empty;
        
        Assert.True(mb == mc);
    }
}
