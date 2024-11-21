using FluentAssertions;
using Xunit;
using Rint = LanguageExt.Tests.ReverseNumber<int>;

namespace LanguageExt.Tests.Transformer.Traverse.Lst.Collections;

public class SetLst
{
    [Fact]
    public void EmptyEmptyIsEmptyEmpty()
    {
        Set<Lst<int>> ma = Empty;

        var mb = ma.Traverse(mx => mx).As();

        var mc = List.singleton(Set<int>.Empty);
        
        Assert.True(mb == mc);
    }
    
    [Fact]
    public void SetLstCrossProduct()
    {
        var ma = Set(List(1, 2), List(10, 20, 30));

        var mb = ma.Traverse(mx => mx).As();
        

        var mc = List(
            Set(1, 10), 
            Set(1, 20), 
            Set(1, 30), 
            Set(2, 10), 
            Set(2, 20), 
            Set(2, 30));
        
        Assert.True(mb == mc);
        foreach (var set in mb)
        {
            set.Should<int>().BeInAscendingOrder();
        }
    }

    [Fact]
    public void SetLstCrossProductReverse()
    {
        var ma = Set(List<Rint>(1, 2), List<Rint>(10, 20, 30));

        var mb = ma.Traverse(mx => mx).As();
        

        var mc = List(
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
        var ma = Set(List<int>(), List(1, 2, 3));

        var mb = ma.Traverse(mx => mx).As();

        var mc = Lst<Set<int>>.Empty;
        
        Assert.True(mb == mc);
    }
    
    [Fact]
    public void SetOfEmptiesIsEmpty()
    {
        var ma = Set(List<int>(), List<int>());

        var mb = ma.Traverse(mx => mx).As();

        var mc = Lst<Set<int>>.Empty;
        
        Assert.True(mb == mc);
    }
}
