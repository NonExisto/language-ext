using Xunit;

namespace LanguageExt.Tests.Transformer.Traverse.IEnumerableT.Collections
{
    public class ArrIEnumerable
    {
        [Fact]
        public void EmptyEmptyIsEmptyEmpty()
        {
            Arr<Iterable<int>> ma = Empty;

            var mb = ma.Traverse(mx => mx).As();

            var mc = Iterable.singleton(Arr.empty<int>());

            Assert.True(mb.ToSeq() == mc.ToSeq());
        }

        [Fact]
        public void ArrIEnumerableCrossProduct()
        {
            var ma = Array<Iterable<int>>([1, 2], [10, 20, 30]);
            var mb = ma.Traverse(mx => mx).As();
            var mc = Iterable.create(
                    Array(1, 10),
                    Array(1, 20),
                    Array(1, 30),
                    Array(2, 10),
                    Array(2, 20),
                    Array(2, 30));

            var r = mb == mc;
            Assert.True(r);
        }

        [Fact]
        public void ArrOfEmptiesAndNonEmptiesIsEmpty()
        {
            var ma = Array<Iterable<int>>([], [1, 2, 3]);

            var mb = ma.Traverse(mx => mx).As();

            var mc = Iterable.empty<Arr<int>>();

            Assert.True(mb.ToSeq() == mc.ToSeq());
        }

        [Fact]
        public void ArrOfEmptiesIsEmpty()
        {
            var ma = Array<Iterable<int>>([], []);

            var mb = ma.Traverse(mx => mx).As();


            var mc = Iterable.empty<Arr<int>>();

            Assert.True(mb.ToSeq() == mc.ToSeq());
        }
    }
}
