using Xunit;

namespace LanguageExt.Tests.Transformer.Traverse.HashSetT.Sync
{
    public class OptionHashSet
    {
        [Fact]
        public void NoneIsSingletonNone()
        {
            var ma = Option<HashSet<int>>.None;
            var mb = ma.Traverse(mx => mx).As();

            var mc = HashSet(Option<int>.None);

            Assert.True(mb == mc);
        }
        
        [Fact]
        public void SomeEmptyIsEmpty()
        {
            var ma = Some<HashSet<int>>(Empty);
            var mb = ma.Traverse(mx => mx).As();

            var mc = HashSet<Option<int>>();

            Assert.True(mb == mc);
        }
        
        [Fact]
        public void SomeNonEmptyHashSetIsHashSetSomes()
        {
            var ma = Some(HashSet(1, 2, 3));
            var mb = ma.Traverse(mx => mx).As();

            var mc = HashSet(Some(1), Some(2), Some(3)); 
            
            Assert.True(mb == mc);
        }
    }
}
