using Xunit;

namespace LanguageExt.Tests
{
    public class PartialAndCurryingTests
    {
        [Fact]
        public void CurryTest()
        {
            var add = curry((int x, int y) => x + y);
            Assert.Equal(15, add(10)(5));
        }

        [Fact]
        public void PartialTest1()
        {
            var partial = curry((int x, int y) => x + y)(10);

            Assert.Equal(15, partial(5));
        }

        [Fact]
        public void PartialTest2()
        {
            var partial = par((int x, int y) => x + y, 10);

            Assert.Equal(15, partial(5));
        }

        [Fact]
        public void PartialTest3()
        {
            var partial = par((int x, int y, int c, int d) => x + y + c + d, 10, 10);

            Assert.Equal(30, partial(5, 5));
        }

        [Fact]
        public void CurryPartialTest()
        {
            var partial = curry(par((int x, int y, int c, int d) => x + y + c + d, 10, 10));

            Assert.Equal(30, partial(5)(5));
        }
    }
}
