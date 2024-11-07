using LanguageExt.Common;
using Xunit;

namespace LanguageExt.Tests.Transformer.Traverse.SeqT.Sync
{
    public class EitherSeq
    {
        [Fact]
        public void RightEmptyIsEmpty()
        {
            var ma = Right<Error, Seq<int>>(Empty);
            var mb = ma.Traverse(mx => mx).As();

            var mc = Seq<Either<Error, int>>();

            Assert.True(mb == mc);
        }

        [Fact]
        public void RightNonEmptySeqIsSeqRight()
        {
            var ma = Right<Error, Seq<int>>(Seq(1, 2, 3, 4));
            var mb = ma.Traverse(mx => mx).As();

            var mc = Seq(Right<Error, int>(1), Right<Error, int>(2), Right<Error, int>(3), Right<Error, int>(4));

            Assert.True(mb == mc);
        }
    }
}
