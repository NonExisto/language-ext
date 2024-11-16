﻿using Xunit;
using System;

namespace LanguageExt.Tests
{

    public class OptionTests
    {
        [Fact]
        public void SomeGeneratorTestsObject()
        {
            var optional = Some(123);

            optional.Match(Some: i => Assert.Equal(123, i),
                           None: () => Assert.Fail("Shouldn't get here"));

            int c = optional.Match(Some: i => i + 1,
                                   None: () => 0);

            Assert.Equal(124, c);
        }

        [Fact]
        public void SomeGeneratorTestsFunction()
        {
            var optional = Some(123);

            match(optional, Some: i => Assert.Equal(123, i),
                            None: () => Assert.Fail("Shouldn't get here"));

            int c = match(optional, Some: i => i + 1,
                                    None: () => 0);

            Assert.Equal(124, c);
        }

        [Fact]
        public void NoneGeneratorTestsObject()
        {
            Option<int> optional = None;

            optional.Match(Some: i => Assert.Fail("Shouldn't get here"),
                           None: () => Assert.True(true));

            int c = optional.Match(Some: i => i + 1,
                                   None: () => 0);

            Assert.Equal(0, c);
        }

        [Fact]
        public void NoneGeneratorTestsFunction()
        {
            Option<int> optional = None;

            match(optional, Some: i => Assert.Fail("Shouldn't get here"),
                            None: () => Assert.True(true));

            int c = match(optional, Some: i => i + 1,
                                    None: () => 0);

            Assert.Equal(0, c);
        }

        [Fact]
        public void SomeLinqTest()
        {
            var two = Some(2);
            var four = Some(4);
            var six = Some(6);

            var expr = from x in two
                       from y in four
                       from z in six
                       select x + y + z;

            match(expr,
                Some: v => Assert.Equal(12, v),
                None: failwith("Shouldn't get here"));
        }

        [Fact]
        public void NoneLinqTest()
        {
            var two = Some(2);
            var four = Some(4);
            var six = Some(6);
            Option<int> none = None;

            match(from x in two
                  from y in four
                  from _ in none
                  from z in six
                  select x + y + z,
                   Some: v => failwith<int>("Shouldn't get here"),
                   None: () => Assert.True(true));
        }

        [Fact]
        public void OptionFluentSomeNoneTest()
        {
            int res1 = GetValue(true)
                        .Some(x => x + 10)
                        .None(0);

            int res2 = GetValue(false)
                        .Some(x => x + 10)
                        .None(() => 0);

            Assert.Equal(1010, res1);
            Assert.Equal(0, res2);
        }
        [Fact]
        public void NullableTest()
        {
            var res = GetNullable(true)
                        .Some(v => v)
                        .None(() => 0);

            Assert.Equal(1000, res);
        }

        [Fact]
        public void NullableDenySomeNullTest() => Assert.Throws<ValueIsNullException>(
                    () =>
                    {
                        GetNullable(false);
                    }
                );

        [Fact]
        public void BiIterSomeTest()
        {
            var x = Some(3);
            int way = 0;
            var dummy = x.BiIter(_ => way = 1, () => way = 2);
            Assert.Equal(1, way);
        }

        [Fact]
        public void BiIterNoneTest()
        {
            var x = Option<int>.None;
            int way = 0;
            var dummy = x.BiIter(_ => way = 1, () => way = 2);
            Assert.Equal(2, way);
        }

        [Fact]
        public void IfNoneSideEffect()
        {
            int sideEffectResult = 0;

            Action sideEffectNone = () => sideEffectResult += 1;

            Assert.Equal(0, Option<string>.Some("test").IfNone(sideEffectNone).Return(sideEffectResult));
            Assert.Equal(1, Option<string>.None.IfNone(sideEffectNone).Return(sideEffectResult));
        }

        [Fact]
        public void ISomeSideEffect()
        {
            int sideEffectResult = 0;

            Action<string> sideEffectSome = _ => sideEffectResult += 2;

            Assert.Equal(0, Option<string>.None.IfSome(sideEffectSome).Return(sideEffectResult));
            Assert.Equal(2, Option<string>.Some("test").IfSome(sideEffectSome).Return(sideEffectResult));
        }

        [Fact]
        public void OptionToFin()
        {
            var e = LanguageExt.Common.Error.New("Example error");
            var some = Some(123);
            var none = Option<int>.None;

            var mx = FinSucc(123);
            var my = some.ToFin();
            var me = none.ToFin(e);

            var e2 = mx.Equals(my);
            var e1 = mx == my;
            var e3 = mx.Equals((object)my);
            
            Assert.True(e1);
            Assert.True(e2);
            Assert.True(e3);
            Assert.True(me.IsFail);
        }

        [Fact]
        public void OptionShouldBeTrue()
        {
            var some = Some(0);
            var none = Option<int>.None;
            if(some || none)
            {
                Assert.True(true);
            }
            else
            {
                Assert.False(true);
            }

            
            if(none)
            {
                Assert.True(false);
            }
            else
            {
                Assert.False(false);
            }

        }

        [Fact]
        public void OptionShouldBeFalse()
        {
            var some = Some(0);
            var none = Option<int>.None;
            if(none && some)
            {
                Assert.True(false);
            }
            else
            {
                Assert.False(false);
            }

            
            if(some && none)
            {
                Assert.True(false);
            }
            else
            {
                Assert.False(false);
            }

        }

        private static Option<int> GetNullable(bool select) =>
            select
                ? Some((int?)1000)
                : Some((int?)null);

        private static Option<int> GetValue(bool select) =>
            select
                ? Some(1000)
                : None;
    }
}
