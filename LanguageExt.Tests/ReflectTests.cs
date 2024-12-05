using System;
using Xunit;

namespace LanguageExt.Tests
{
    public class ReflectTests
    {
        public class TestClass
        {
            public readonly string? W;
            public readonly string? X;
            public readonly string? Y;
            public readonly string? Z;

            public TestClass()
            {
            }

            public TestClass(string w)
            {
                W = w;
            }

            public TestClass(string w, string x)
            {
                W = w;
                X = x;
            }

            public TestClass(string w, bool x)
            {
                W = w;
                X = x.ToString();
            }

            public TestClass(string w, string x, string y)
            {
                W = w;
                X = x;
                Y = y;
            }

            public TestClass(string w, string x, string y, string z)
            {
                W = w;
                X = x;
                Y = y;
                Z = z;
            }
        }

        [Fact]
        public void CtorOfArity1Test()
        {
            var ctor = IL.Ctor<string, TestClass>();

            var res = ctor("Hello");

            Assert.Equal("Hello", res.W);
        }

        [Fact]
        public void CtorOfArity2Test()
        {
            var ctor = IL.Ctor<string, string, TestClass>();

            var res = ctor("Hello", "World");

            Assert.Equal("Hello", res.W);
            Assert.Equal("World", res.X);
        }

        [Fact]
        public void CtorOfArity2Test2()
        {
            var ctor = IL.Ctor<string, bool, TestClass>();

            var res = ctor("Hello", true);

            Assert.Equal("Hello", res.W);
            Assert.Equal("True", res.X);
        }

        [Fact]
        public void CtorOfArity3Test()
        {
            var ctor = IL.Ctor<string, string, string, TestClass>();

            var res = ctor("Roland","TR", "909");

            Assert.Equal("Roland", res.W);
            Assert.Equal("TR", res.X);
            Assert.Equal("909", res.Y);
        }

        [Fact]
        public void CtorOfArity4Test()
        {
            var ctor = IL.Ctor<string, string, string, string, TestClass>();

            var res = ctor("Chandler", "Curve", "Bender", "EQ");

            Assert.Equal("Chandler", res.W);
            Assert.Equal("Curve", res.X);
            Assert.Equal("Bender", res.Y);
            Assert.Equal("EQ", res.Z);
        }

        [Fact]
        public void DateConstructTest()
        {
            var ticks = new DateTime(2017, 1, 1).Ticks;
            var ctor = IL.Ctor<long, DateTime>();

            DateTime res = ctor(ticks);

            Assert.True(res.Ticks == ticks);
        }
    }
}
