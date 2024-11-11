using Xunit;

namespace LanguageExt.Tests.Transformer.Traverse.SetT.Collections
{
    public class ArrSet
    {
        [Fact]
        public void EmptyEmptyIsEmptyEmpty()
        {
            Arr<Set<int>> ma = Empty;
            var mb = ma.Traverse(mx => mx).As();

            var mc = Set.singleton(Arr<int>.Empty);
            
            Assert.True(mb == mc);
        }
        
        [Fact]
        public void ArrSetCrossProduct()
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
        }
        
        [Fact]
        public void ArrOfEmptiesAndNonEmptiesIsEmpty()
        {
            var ma = Set(List<int>(), List(1, 2, 3));
            var mb = ma.Traverse(mx => mx).As();

            var mc = List<Set<int>>();
            
            Assert.True(mb == mc);
        }
        
        [Fact]
        public void ArrOfEmptiesIsEmpty()
        {
            var ma = Set(List<int>(), List<int>());
            var mb = ma.Traverse(mx => mx).As();

            var mc = List<Set<int>>();
            
            Assert.True(mb == mc);
        }
    }
}
