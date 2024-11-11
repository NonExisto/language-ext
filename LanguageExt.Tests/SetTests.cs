using Xunit;
using System;

namespace LanguageExt.Tests
{
    public class SetTests
    {
        [Fact]
        public void SetKeyTypeTests()
        {
            var set = Set(StringComparer.OrdinalIgnoreCase, "one", "two", "three");

            Assert.True(set.Contains("one"));
            Assert.True(set.Contains("ONE"));

            Assert.True(set.Contains("two"));
            Assert.True(set.Contains("Two"));

            Assert.True(set.Contains("three"));
            Assert.True(set.Contains("thREE"));
        }


        [Fact]
        public void EqualsTest()
        {
            var a = Set(1, 2);
            var b = Set(1, 2, 3);

            Assert.False(Set(1, 2, 3).Equals(Set<int>()));
            Assert.False(Set<int>().Equals(Set(1, 2, 3)));
            Assert.True(Set<int>().Equals(Set<int>()));
            Assert.True(Set(1).Equals(Set(1)));
            Assert.True(Set(1, 2).Equals(Set(1, 2)));
            Assert.False(Set(1, 2).Equals(Set(1, 2, 3)));
            Assert.False(Set(1, 2, 3).Equals(Set(1, 2)));
        }

        [Fact]
        public void SetGeneratorTest()
        {
            var m1 = Set<int>();
            m1 = m1.Add(100);
            Assert.True(m1.Count == 1 && m1.Contains(100));
        }

        [Fact]
        public void SetAddInOrderTest()
        {
            var m = Set(1);
            m.Find(1).IfNone(() => failwith<int>("Broken"));

            m = Set(1, 2);
            m.Find(1).IfNone(() => failwith<int>("Broken"));
            m.Find(2).IfNone(() => failwith<int>("Broken"));

            m = Set(1, 2, 3);
            m.Find(1).IfNone(() => failwith<int>("Broken"));
            m.Find(2).IfNone(() => failwith<int>("Broken"));
            m.Find(3).IfNone(() => failwith<int>("Broken"));

            m = Set(1, 2, 3, 4);
            m.Find(1).IfNone(() => failwith<int>("Broken"));
            m.Find(2).IfNone(() => failwith<int>("Broken"));
            m.Find(3).IfNone(() => failwith<int>("Broken"));
            m.Find(4).IfNone(() => failwith<int>("Broken"));

            m = Set(1, 2, 3, 4, 5);
            m.Find(1).IfNone(() => failwith<int>("Broken"));
            m.Find(2).IfNone(() => failwith<int>("Broken"));
            m.Find(3).IfNone(() => failwith<int>("Broken"));
            m.Find(4).IfNone(() => failwith<int>("Broken"));
            m.Find(5).IfNone(() => failwith<int>("Broken"));
        }

        [Fact]
        public void SetAddInReverseOrderTest()
        {
            var m = Set(2, 1);
            m.Find(1).IfNone(() => failwith<int>("Broken"));
            m.Find(2).IfNone(() => failwith<int>("Broken"));

            m = Set(3, 2, 1);
            m.Find(1).IfNone(() => failwith<int>("Broken"));
            m.Find(2).IfNone(() => failwith<int>("Broken"));
            m.Find(3).IfNone(() => failwith<int>("Broken"));

            m = Set(4, 3, 2, 1);
            m.Find(1).IfNone(() => failwith<int>("Broken"));
            m.Find(2).IfNone(() => failwith<int>("Broken"));
            m.Find(3).IfNone(() => failwith<int>("Broken"));
            m.Find(4).IfNone(() => failwith<int>("Broken"));

            m = Set(5, 4, 3, 2, 1);
            m.Find(1).IfNone(() => failwith<int>("Broken"));
            m.Find(2).IfNone(() => failwith<int>("Broken"));
            m.Find(3).IfNone(() => failwith<int>("Broken"));
            m.Find(4).IfNone(() => failwith<int>("Broken"));
            m.Find(5).IfNone(() => failwith<int>("Broken"));
        }

        [Fact]
        public void MapAddInMixedOrderTest()
        {
            var m = Set(5, 1, 3, 2, 4);
            m.Find(1).IfNone(() => failwith<int>("Broken"));
            m.Find(2).IfNone(() => failwith<int>("Broken"));
            m.Find(3).IfNone(() => failwith<int>("Broken"));
            m.Find(4).IfNone(() => failwith<int>("Broken"));
            m.Find(5).IfNone(() => failwith<int>("Broken"));

            m = Set(1, 3, 5, 2, 4);
            m.Find(1).IfNone(() => failwith<int>("Broken"));
            m.Find(2).IfNone(() => failwith<int>("Broken"));
            m.Find(3).IfNone(() => failwith<int>("Broken"));
            m.Find(4).IfNone(() => failwith<int>("Broken"));
            m.Find(5).IfNone(() => failwith<int>("Broken"));
        }


        [Fact]
        public void SetRemoveTest()
        {
            var m = Set(1, 3, 5, 2, 4);

            m.Find(1).IfNone(() => failwith<int>("Broken 1"));
            m.Find(2).IfNone(() => failwith<int>("Broken 2"));
            m.Find(3).IfNone(() => failwith<int>("Broken 3"));
            m.Find(4).IfNone(() => failwith<int>("Broken 4"));
            m.Find(5).IfNone(() => failwith<int>("Broken 5"));

            Assert.Equal(5, m.Count);

            m = m.Remove(4);
            Assert.Equal(4, m.Count);
            Assert.True(m.Find(4).IsNone);
            m.Find(1).IfNone(() => failwith<int>("Broken 1"));
            m.Find(2).IfNone(() => failwith<int>("Broken 2"));
            m.Find(3).IfNone(() => failwith<int>("Broken 3"));
            m.Find(5).IfNone(() => failwith<int>("Broken 5"));

            m = m.Remove(1);
            Assert.Equal(3, m.Count);
            Assert.True(m.Find(1).IsNone);
            m.Find(2).IfNone(() => failwith<int>("Broken 2"));
            m.Find(3).IfNone(() => failwith<int>("Broken 3"));
            m.Find(5).IfNone(() => failwith<int>("Broken 5"));

            m = m.Remove(2);
            Assert.Equal(2, m.Count);
            Assert.True(m.Find(2).IsNone);
            m.Find(3).IfNone(() => failwith<int>("Broken 3"));
            m.Find(5).IfNone(() => failwith<int>("Broken 5"));

            m = m.Remove(3);
            Assert.Single(m);
            Assert.True(m.Find(3).IsNone);
            m.Find(5).IfNone(() => failwith<int>("Broken 5"));

            m = m.Remove(5);
            Assert.Empty(m);
            Assert.True(m.Find(5).IsNone);
        }

        [Fact]
        public void MassAddRemoveTest()
        {
            int max = 100000;

            var items = IterableExtensions.AsIterable(Range(1, max)).Map(_ => Guid.NewGuid()).ToLst();

            var m = toSet(items);
            Assert.True(m.Count == max);
            foreach (var item in items)
            {
                Assert.True(m.Contains(item));
                m = m.Remove(item);
                Assert.False(m.Contains(item));
                max--;
                Assert.True(m.Count == max);
            }
        }
        
        [Fact]
        public void SetOrdSumTest()
        {
            using (withOrderComparer(StringComparer.OrdinalIgnoreCase))
            {
                var m1 = Set("one", "two");
                var m2 = Set("three");

                var sum = m1 + m2;
                
                Assert.Equal(sum, m1.AddRange(m2));
                Assert.Equal(m2, sum.Clear() + m2);
            }
        }

        [Fact]
        public void SetUnionTest1()
        {
            var x = Set((1, 1), (2, 2), (3, 3));
            var y = Set((1, 1), (2, 2), (3, 3));

            var z = Set.union(x, y);

            Assert.True(z == Set((1, 1), (2, 2), (3, 3)));
        }

        [Fact]
        public void SetUnionTest2()
        {
            var x = Set((1, 1), (2, 2), (3, 3));
            var y = Set((4, 4), (5, 5), (6, 6));

            var z = Set.union(x, y);

            Assert.True(z == Set((1, 1), (2, 2), (3, 3), (4, 4), (5, 5), (6, 6)));
        }

        [Fact]
        public void SetIntesectTest1()
        {
            var x = Set((2, 2), (3, 3));
            var y = Set((1, 1), (2, 2));

            var z = Set.intersect(x, y);

            Assert.True(z == Set((2, 2)));
        }

        [Fact]
        public void SetExceptTest()
        {
            var x = Set((1, 1), (2, 2), (3, 3));
            var y = Set((1, 1));

            var z = Set.except(x, y);

            Assert.True(z == Set((2, 2), (3, 3)));
        }

        [Fact]
        public void SetSymmetricExceptTest()
        {
            var x = Set((1, 1), (2, 2), (3, 3));
            var y = Set((1, 1), (3, 3));

            var z = Set.symmetricExcept(x, y);

            Assert.True(z == Set((2, 2)));
        }


        [Fact]
        public void SliceTest()
        {
            var m = Set(1, 2, 3, 4, 5);

            var x = m.Slice(1, 2);

            Assert.Equal(2, x.Count);
            Assert.True(x.Contains(1));
            Assert.True(x.Contains(2));

            var y = m.Slice(2, 4);

            Assert.Equal(3, y.Count);
            Assert.True(y.Contains(2));
            Assert.True(y.Contains(3));
            Assert.True(y.Contains(4));

            var z = m.Slice(4, 5);

            Assert.Equal(2, z.Count);
            Assert.True(z.Contains(4));
            Assert.True(z.Contains(5));
        }

        [Fact]
        public void MinMaxTest()
        {
            var m = Set(1, 2, 3, 4, 5);

            Assert.Equal(1, m.Min);
            Assert.Equal(5, m.Max);

            var me = Set<int>();

            Assert.True(me.Min == None);
            Assert.True(me.Max == None);
        }

        [Fact]
        public void FindPredecessorWhenKeyExistsTest()
        {
            var m = Set(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);

            Assert.True(m.FindPredecessor(1) == None);
            Assert.Equal(1, m.FindPredecessor(2));
            Assert.Equal(2, m.FindPredecessor(3));
            Assert.Equal(3, m.FindPredecessor(4));
            Assert.Equal(4, m.FindPredecessor(5));
            Assert.Equal(5, m.FindPredecessor(6));
            Assert.Equal(6, m.FindPredecessor(7));
            Assert.Equal(7, m.FindPredecessor(8));
            Assert.Equal(8, m.FindPredecessor(9));
            Assert.Equal(9, m.FindPredecessor(10));
            Assert.Equal(10, m.FindPredecessor(11));
            Assert.Equal(11, m.FindPredecessor(12));
            Assert.Equal(12, m.FindPredecessor(13));
            Assert.Equal(13, m.FindPredecessor(14));
            Assert.Equal(14, m.FindPredecessor(15));
        }

        [Fact]
        public void FindPredecessorWhenKeyNotExistsTest()
        {
            var m = Set(1, 3, 5, 7, 9, 11, 13, 15);

            Assert.True(m.FindPredecessor(1) == None);
            Assert.Equal(1, m.FindPredecessor(2));
            Assert.Equal(1, m.FindPredecessor(3));
            Assert.Equal(3, m.FindPredecessor(4));
            Assert.Equal(3, m.FindPredecessor(5));
            Assert.Equal(5, m.FindPredecessor(6));
            Assert.Equal(5, m.FindPredecessor(7));
            Assert.Equal(7, m.FindPredecessor(8));
            Assert.Equal(7, m.FindPredecessor(9));
            Assert.Equal(9, m.FindPredecessor(10));
            Assert.Equal(9, m.FindPredecessor(11));
            Assert.Equal(11, m.FindPredecessor(12));
            Assert.Equal(11, m.FindPredecessor(13));
            Assert.Equal(13, m.FindPredecessor(14));
            Assert.Equal(13, m.FindPredecessor(15));
        }

        [Fact]
        public void FindExactOrPredecessorWhenKeyExistsTest()
        {
            var m = Set(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);

            Assert.Equal(1, m.FindExactOrPredecessor(1));
            Assert.Equal(2, m.FindExactOrPredecessor(2));
            Assert.Equal(3, m.FindExactOrPredecessor(3));
            Assert.Equal(4, m.FindExactOrPredecessor(4));
            Assert.Equal(5, m.FindExactOrPredecessor(5));
            Assert.Equal(6, m.FindExactOrPredecessor(6));
            Assert.Equal(7, m.FindExactOrPredecessor(7));
            Assert.Equal(8, m.FindExactOrPredecessor(8));
            Assert.Equal(9, m.FindExactOrPredecessor(9));
            Assert.Equal(10, m.FindExactOrPredecessor(10));
            Assert.Equal(11, m.FindExactOrPredecessor(11));
            Assert.Equal(12, m.FindExactOrPredecessor(12));
            Assert.Equal(13, m.FindExactOrPredecessor(13));
            Assert.Equal(14, m.FindExactOrPredecessor(14));
            Assert.Equal(15, m.FindExactOrPredecessor(15));
        }

        [Fact]
        public void FindExactOrPredecessorWhenKeySometimesExistsTest()
        {
            var m = Set(1, 3, 5, 7, 9, 11, 13, 15);

            Assert.Equal(1, m.FindExactOrPredecessor(1));
            Assert.Equal(1, m.FindExactOrPredecessor(2));
            Assert.Equal(3, m.FindExactOrPredecessor(3));
            Assert.Equal(3, m.FindExactOrPredecessor(4));
            Assert.Equal(5, m.FindExactOrPredecessor(5));
            Assert.Equal(5, m.FindExactOrPredecessor(6));
            Assert.Equal(7, m.FindExactOrPredecessor(7));
            Assert.Equal(7, m.FindExactOrPredecessor(8));
            Assert.Equal(9, m.FindExactOrPredecessor(9));
            Assert.Equal(9, m.FindExactOrPredecessor(10));
            Assert.Equal(11, m.FindExactOrPredecessor(11));
            Assert.Equal(11, m.FindExactOrPredecessor(12));
            Assert.Equal(13, m.FindExactOrPredecessor(13));
            Assert.Equal(13, m.FindExactOrPredecessor(14));
            Assert.Equal(15, m.FindExactOrPredecessor(15));
        }

        [Fact]
        public void FindSuccessorWhenKeyExistsTest()
        {
            var m = Set(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);

            Assert.Equal(2, m.FindSuccessor(1));
            Assert.Equal(3, m.FindSuccessor(2));
            Assert.Equal(4, m.FindSuccessor(3));
            Assert.Equal(5, m.FindSuccessor(4));
            Assert.Equal(6, m.FindSuccessor(5));
            Assert.Equal(7, m.FindSuccessor(6));
            Assert.Equal(8, m.FindSuccessor(7));
            Assert.Equal(9, m.FindSuccessor(8));
            Assert.Equal(10, m.FindSuccessor(9));
            Assert.Equal(11, m.FindSuccessor(10));
            Assert.Equal(12, m.FindSuccessor(11));
            Assert.Equal(13, m.FindSuccessor(12));
            Assert.Equal(14, m.FindSuccessor(13));
            Assert.Equal(15, m.FindSuccessor(14));
            Assert.True(m.FindSuccessor(15) == None);
        }

        [Fact]
        public void FindSuccessorWhenKeyNotExistsTest()
        {
            var m = Set(1, 3, 5, 7, 9, 11, 13, 15);

            Assert.Equal(3, m.FindSuccessor(1));
            Assert.Equal(3, m.FindSuccessor(2));
            Assert.Equal(5, m.FindSuccessor(3));
            Assert.Equal(5, m.FindSuccessor(4));
            Assert.Equal(7, m.FindSuccessor(5));
            Assert.Equal(7, m.FindSuccessor(6));
            Assert.Equal(9, m.FindSuccessor(7));
            Assert.Equal(9, m.FindSuccessor(8));
            Assert.Equal(11, m.FindSuccessor(9));
            Assert.Equal(11, m.FindSuccessor(10));
            Assert.Equal(13, m.FindSuccessor(11));
            Assert.Equal(13, m.FindSuccessor(12));
            Assert.Equal(15, m.FindSuccessor(13));
            Assert.Equal(15, m.FindSuccessor(14));
            Assert.True(m.FindSuccessor(15) == None);
        }

        [Fact]
        public void FindExactOrSuccessorWhenKeyExistsTest()
        {
            var m = Set(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);

            Assert.Equal(1, m.FindExactOrSuccessor(1));
            Assert.Equal(2, m.FindExactOrSuccessor(2));
            Assert.Equal(3, m.FindExactOrSuccessor(3));
            Assert.Equal(4, m.FindExactOrSuccessor(4));
            Assert.Equal(5, m.FindExactOrSuccessor(5));
            Assert.Equal(6, m.FindExactOrSuccessor(6));
            Assert.Equal(7, m.FindExactOrSuccessor(7));
            Assert.Equal(8, m.FindExactOrSuccessor(8));
            Assert.Equal(9, m.FindExactOrSuccessor(9));
            Assert.Equal(10, m.FindExactOrSuccessor(10));
            Assert.Equal(11, m.FindExactOrSuccessor(11));
            Assert.Equal(12, m.FindExactOrSuccessor(12));
            Assert.Equal(13, m.FindExactOrSuccessor(13));
            Assert.Equal(14, m.FindExactOrSuccessor(14));
            Assert.Equal(15, m.FindExactOrSuccessor(15));
        }

        [Fact]
        public void FindExactOrSuccessorWhenKeySometimesExistsTest()
        {
            var m = Set(1, 3, 5, 7, 9, 11, 13, 15);

            Assert.Equal(1, m.FindExactOrSuccessor(1));
            Assert.Equal(3, m.FindExactOrSuccessor(2));
            Assert.Equal(3, m.FindExactOrSuccessor(3));
            Assert.Equal(5, m.FindExactOrSuccessor(4));
            Assert.Equal(5, m.FindExactOrSuccessor(5));
            Assert.Equal(7, m.FindExactOrSuccessor(6));
            Assert.Equal(7, m.FindExactOrSuccessor(7));
            Assert.Equal(9, m.FindExactOrSuccessor(8));
            Assert.Equal(9, m.FindExactOrSuccessor(9));
            Assert.Equal(11, m.FindExactOrSuccessor(10));
            Assert.Equal(11, m.FindExactOrSuccessor(11));
            Assert.Equal(13, m.FindExactOrSuccessor(12));
            Assert.Equal(13, m.FindExactOrSuccessor(13));
            Assert.Equal(15, m.FindExactOrSuccessor(14));
            Assert.Equal(15, m.FindExactOrSuccessor(15));
        }

        [Fact]
        public void CaseTest()
        {
            // seq1 tests here just for reference
            { Assert.True(Seq<int>().Case is not var (_, _) and not {} ); }

            { Assert.True(Set<int>().Case is not var (_, _) and not {} ); }
            { Assert.True(Set<int>(1).Case is not var (_, _) and 1); }

           
        }
    }
}
