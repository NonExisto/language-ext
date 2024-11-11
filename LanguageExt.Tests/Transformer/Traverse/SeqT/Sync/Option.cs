using Xunit;

namespace LanguageExt.Tests.Transformer.Traverse.SeqT.Sync
{
    public class OptionSeq
    {
        [Fact]
        public void SomeEmptyIsEmpty()
        {
            var ma = Some<Seq<int>>(Empty);
            var mb = ma.Traverse(mx => mx).As();

            var mc = Seq<Option<int>>();

            Assert.True(mb == mc);
        }

        [Fact]
        public void SomeNonEmptySeqIsSeqSomes()
        {
            var ma = Some(Seq(1, 2, 3));
            var mb = ma.Traverse(mx => mx).As();

            var mc = Seq(Some(1), Some(2), Some(3));

            Assert.True(mb == mc);
        }
    }
}
