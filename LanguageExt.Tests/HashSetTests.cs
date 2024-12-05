using Xunit;
using System;
using Newtonsoft.Json;
using static LanguageExt.HashSet;

namespace LanguageExt.Tests;

public class HashSetTests
{
    [Fact]
    public void HashSetHasItemsTest() => 
        Assert.False(HashSet(1, 2, 3).IsEmpty);

    [Fact]
    public void HashSetGeneratorTest()
    {
        var m1 = HashSet<string>();
        m1 = add(m1, "hello");
        Assert.True(m1.Count == 1 && contains(m1, "hello"));
    }

    [Fact]
    public void HashSetGeneratorAndMatchTest()
    {
        var m2 = HashSet("a", "b", "c");

        m2 = add(m2, "world");

        var res = find(m2, "world").Match(
            v  => v,
            () => "failed"
        );

        Assert.Equal("world", res);
    }

    [Fact]
    public void HashSetAddInOrderTest()
    {
        var m = HashSet(1);
        m.Find(1).IfNone(() => failwith<int>("Broken"));

        m = HashSet(1, 2);
        m.Find(1).IfNone(() => failwith<int>("Broken"));
        m.Find(2).IfNone(() => failwith<int>("Broken"));

        m = HashSet(1, 2, 3);
        m.Find(1).IfNone(() => failwith<int>("Broken"));
        m.Find(2).IfNone(() => failwith<int>("Broken"));
        m.Find(3).IfNone(() => failwith<int>("Broken"));

        m = HashSet(1, 2, 3, 4);
        m.Find(1).IfNone(() => failwith<int>("Broken"));
        m.Find(2).IfNone(() => failwith<int>("Broken"));
        m.Find(3).IfNone(() => failwith<int>("Broken"));
        m.Find(4).IfNone(() => failwith<int>("Broken"));

        m = HashSet(1, 2, 3, 4, 5);
        m.Find(1).IfNone(() => failwith<int>("Broken"));
        m.Find(2).IfNone(() => failwith<int>("Broken"));
        m.Find(3).IfNone(() => failwith<int>("Broken"));
        m.Find(4).IfNone(() => failwith<int>("Broken"));
        m.Find(5).IfNone(() => failwith<int>("Broken"));
    }

    [Fact]
    public void HashSetAddInReverseOrderTest()
    {
        var m = HashSet(1);
        m.Find(1).IfNone(() => failwith<int>("Broken"));

        m = HashSet(2, 1);
        m.Find(1).IfNone(() => failwith<int>("Broken"));
        m.Find(2).IfNone(() => failwith<int>("Broken"));

        m = HashSet(3, 2, 1);
        m.Find(1).IfNone(() => failwith<int>("Broken"));
        m.Find(2).IfNone(() => failwith<int>("Broken"));
        m.Find(3).IfNone(() => failwith<int>("Broken"));

        m = HashSet(4, 3, 2, 1);
        m.Find(1).IfNone(() => failwith<int>("Broken"));
        m.Find(2).IfNone(() => failwith<int>("Broken"));
        m.Find(3).IfNone(() => failwith<int>("Broken"));
        m.Find(4).IfNone(() => failwith<int>("Broken"));

        m = HashSet(5, 4, 3, 2, 1);
        m.Find(1).IfNone(() => failwith<int>("Broken"));
        m.Find(2).IfNone(() => failwith<int>("Broken"));
        m.Find(3).IfNone(() => failwith<int>("Broken"));
        m.Find(4).IfNone(() => failwith<int>("Broken"));
        m.Find(5).IfNone(() => failwith<int>("Broken"));
    }

    [Fact]
    public void HashSetAddInMixedOrderTest()
    {
        var m = HashSet(5, 1, 3, 2, 4);
        m.Find(1).IfNone(() => failwith<int>("Broken"));
        m.Find(2).IfNone(() => failwith<int>("Broken"));
        m.Find(3).IfNone(() => failwith<int>("Broken"));
        m.Find(4).IfNone(() => failwith<int>("Broken"));
        m.Find(5).IfNone(() => failwith<int>("Broken"));

        m = HashSet(1, 3, 5, 2, 4);
        m.Find(1).IfNone(() => failwith<int>("Broken"));
        m.Find(2).IfNone(() => failwith<int>("Broken"));
        m.Find(3).IfNone(() => failwith<int>("Broken"));
        m.Find(4).IfNone(() => failwith<int>("Broken"));
        m.Find(5).IfNone(() => failwith<int>("Broken"));
    }

    [Fact]
    public void HashSetRemoveTest()
    {
        var m = HashSet("a", "b", "c", "d", "e");

        m.Find("a").IfNone(() => failwith<string>("Broken 1"));
        m.Find("b").IfNone(() => failwith<string>("Broken 2"));
        m.Find("c").IfNone(() => failwith<string>("Broken 3"));
        m.Find("d").IfNone(() => failwith<string>("Broken 4"));
        m.Find("e").IfNone(() => failwith<string>("Broken 5"));

        Assert.Equal(5, m.Count);

        m = remove(m, "d");
        Assert.Equal(4, m.Count);
        Assert.True(m.Find("d").IsNone);
        m.Find("a").IfNone(() => failwith<string>("Broken 1"));
        m.Find("b").IfNone(() => failwith<string>("Broken 2"));
        m.Find("c").IfNone(() => failwith<string>("Broken 3"));
        m.Find("e").IfNone(() => failwith<string>("Broken 5"));

        m = remove(m, "a");
        Assert.Equal(3, m.Count);
        Assert.True(m.Find("a").IsNone);
        m.Find("b").IfNone(() => failwith<string>("Broken 2"));
        m.Find("c").IfNone(() => failwith<string>("Broken 3"));
        m.Find("e").IfNone(() => failwith<string>("Broken 5"));

        m = remove(m, "b");
        Assert.Equal(2, m.Count);
        Assert.True(m.Find("b").IsNone);
        m.Find("c").IfNone(() => failwith<string>("Broken 3"));
        m.Find("e").IfNone(() => failwith<string>("Broken 5"));

        m = remove(m, "c");
        Assert.Single(m);
        Assert.True(m.Find("c").IsNone);
        m.Find("e").IfNone(() => failwith<string>("Broken 5"));

        m = remove(m, "e");
        Assert.Empty(m);
        Assert.True(m.Find("e").IsNone);
    }

    [Fact]
    public void HashSetKeyTypeTests()
    {
        var set = new HashSet<string>(["one", "two", "three"], equalityComparer: StringComparer.OrdinalIgnoreCase);

        Assert.True(set.Contains("one"));
        Assert.True(set.Contains("ONE"));

        Assert.True(set.Contains("two"));
        Assert.True(set.Contains("Two"));

        Assert.True(set.Contains("three"));
        Assert.True(set.Contains("thREE"));
    }

    [Fact]
    public void HashSetSetTest()
    {
        var set  = HashSet(StringComparer.OrdinalIgnoreCase, "one", "two", "three");
        var set2 = set.SetItem("One");
        Assert.Equal(3, set2.Count);
        Assert.False(set2.ToSeq().Contains("one"));
        Assert.True(set2.ToSeq().Contains("One"));

        Assert.Throws<ArgumentException>(() => set.SetItem("four"));
    }

    [Fact]
    public void EqualsTest()
    {
        Assert.False(HashSet(1, 2, 3).Equals(HashSet<int>()));
        Assert.False(HashSet<int>().Equals(HashSet(1, 2, 3)));
        Assert.True(HashSet<int>().Equals(HashSet<int>()));
        Assert.True(HashSet(1).Equals(HashSet(1)));
        Assert.True(HashSet(1, 2).Equals(HashSet(1, 2)));
        Assert.False(HashSet(1, 2).Equals(HashSet(1, 2, 3)));
        Assert.False(HashSet(1, 2, 3).Equals(HashSet(1, 2)));
    }
        
    [Fact]
    public void HashSet_WithDefaultSettings_SerializationTest()
    {
        var source = HashSet(123, 456);
        var json   = JsonConvert.SerializeObject(source);
        var result = JsonConvert.DeserializeObject<HashSet<int>>(json);

        Assert.Equal(source, result);
    }

    [Fact]
    public void HashSet_WithEchoCustomSettings_SerializationTest()
    {
        var settings = new JsonSerializerSettings
                       {
                           TypeNameHandling               = TypeNameHandling.All,
                           TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                           MissingMemberHandling          = MissingMemberHandling.Ignore
                       };

        var source = HashSet(123, 456);
        var json   = JsonConvert.SerializeObject(source, settings);
        var result = JsonConvert.DeserializeObject<HashSet<int>>(json, settings);

        Assert.Equal(source, result);
    }
}
