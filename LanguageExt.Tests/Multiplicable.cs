using Xunit;
using LanguageExt.ClassInstances;

namespace LanguageExt.Tests
{
    public class Multiplicable
    {
        [Fact]
        public void OptionalNumericMultiply()
        {
            var x = Some(10);
            var y = Some(20);
            var z = product<TInt, int>(x, y);

            Assert.Equal(200, z);
        }
    }
}
