using Xunit;
using G = System.Collections.Generic;

namespace LanguageExt.Tests.Transformer.Traverse.Lst.Collections
{
    using static Prelude;

    public class IEnumerableLst
    {
        G.IEnumerable<T> mkEnum<T>(params T[] ts) => ts;

        [Fact]
        public void EmptyEmptyIsEmptyEmpty()
        {
            var ma = Iterable<Lst<int>>();

            var mb = ma.Traverse(mx => mx).As();

            var mc = LanguageExt.List.singleton<Iterable<int>>(Empty);
            
            Assert.True(mb == mc);
        }
        
        [Fact]
        public void EnumLstCrossProduct()
        {
            var ma = mkEnum(List(1, 2), List(10, 20, 30)).AsIterable();

            var mb = ma.Traverse(mx => mx).As();

            var mc = List(
                mkEnum(1, 10), 
                mkEnum(1, 20), 
                mkEnum(1, 30), 
                mkEnum(2, 10), 
                mkEnum(2, 20), 
                mkEnum(2, 30));
            
            Assert.True(mb.Map(toList) == mc.Map(toList));
            
        }
                
        [Fact]
        public void SeqOfEmptiesAndNonEmptiesIsEmpty()
        {
            var ma = mkEnum(List<int>(), List(1, 2, 3)).AsIterable();

            var mb = ma.Traverse(mx => mx).As();


            var mc = Lst<Iterable<int>>.Empty;
            
            Assert.True(mb == mc);
        }
        
        [Fact]
        public void SeqOfEmptiesIsEmpty()
        {
            var ma = mkEnum(List<int>(), List<int>()).AsIterable();

            var mb = ma.Traverse(mx => mx).As();


            var mc = Lst<Iterable<int>>.Empty;
            
            Assert.True(mb == mc);
        }
    }
}
