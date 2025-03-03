using System.Collections.Generic;
using Xunit;
using Shouldly;

#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task

namespace DataFac.Storage.Tests;

public class SnapshotTests
{
    [Fact]
    public void Snap01_CompareRecords_True()
    {
        var a1 = new TestRecord("a", 1L);
        var a2 = new TestRecord("a", 1L);

        // check equality
        a2.Equals(a1).ShouldBeTrue();
        a2.IsEqualTo(a1).ShouldBeNull();
    }

    [Fact]
    public void Snap02_CompareRecords_FalseA()
    {
        var initial = new TestRecord("a", 1L);
        var current = new TestRecord("a", 2L);

        // check equality
        current.Equals(initial).ShouldBeFalse();
        current.IsEqualTo(initial).ShouldBe("left (Key=a;Value=2) != right (Key=a;Value=1)");
    }

    [Fact]
    public void Snap02_CompareRecords_FalseB()
    {
        TestRecord initial = new TestRecord("a", 1L);
        TestRecord? current = null;

        // check equality
        current.IsEqualTo(initial).ShouldBe("left is null");
    }

    [Fact]
    public void Snap03_CompareSnapshots_True()
    {
        var snapshot1 = new Dictionary<string, TestRecord>()
        {
            ["a"] = new TestRecord("a", 1L),
            ["b"] = new TestRecord("b", 2L),
        };
        var snapshot2 = new Dictionary<string, TestRecord>()
        {
            ["a"] = new TestRecord("a", 1L),
            ["b"] = new TestRecord("b", 2L),
        };

        // check equality
        snapshot2.IsEqualTo(snapshot1).ShouldBeNull();

        // check differences
        var differences = snapshot1.DifferentTo(snapshot2);
        differences.Length.ShouldBe(0);
    }

    [Fact]
    public void Snap04_CompareSnapshots_FalseA_ValueChanged()
    {
        var snapshot1 = new Dictionary<string, TestRecord>()
        {
            ["a"] = new TestRecord("a", 1L),
            ["b"] = new TestRecord("b", 2L),
        };
        var snapshot2 = new Dictionary<string, TestRecord>()
        {
            ["a"] = new TestRecord("a", 1L),
            ["b"] = new TestRecord("b", 3L),
        };

        // check equality
        snapshot2.IsEqualTo(snapshot1).ShouldBe("left[b] (Key=b;Value=3) != right[b] (Key=b;Value=2)");

        // check differences
        var differences = snapshot1.DifferentTo(snapshot2);
        differences.Length.ShouldBe(1);
        differences[0].Kind.ShouldBe(DifferenceKind.Changed);
        differences[0].Initial.ShouldBe(new TestRecord("b", 2L));
        differences[0].Current.ShouldBe(new TestRecord("b", 3L));
    }

    [Fact]
    public void Snap04_CompareSnapshots_FalseB_ValueAdded()
    {
        var snapshot1 = new Dictionary<string, TestRecord>()
        {
            ["a"] = new TestRecord("a", 1L),
            ["b"] = new TestRecord("b", 2L),
        };
        var snapshot2 = new Dictionary<string, TestRecord>()
        {
            ["a"] = new TestRecord("a", 1L),
            ["b"] = new TestRecord("b", 2L),
            ["c"] = new TestRecord("c", 3L),
        };

        // check equality
        snapshot2.IsEqualTo(snapshot1).ShouldBe("left.Count (3) != right.Count (2)");

        // check differences
        var differences = snapshot1.DifferentTo(snapshot2);
        differences.Length.ShouldBe(1);
        differences[0].Kind.ShouldBe(DifferenceKind.Added);
        differences[0].Current.ShouldBe(new TestRecord("c", 3L));
    }

    [Fact]
    public void Snap04_CompareSnapshots_FalseC_ValueRemoved()
    {
        var snapshot1 = new Dictionary<string, TestRecord>()
        {
            ["a"] = new TestRecord("a", 1L),
            ["b"] = new TestRecord("b", 2L),
        };
        var snapshot2 = new Dictionary<string, TestRecord>()
        {
            ["b"] = new TestRecord("b", 2L),
        };

        // check equality
        snapshot2.IsEqualTo(snapshot1).ShouldBe("left.Count (1) != right.Count (2)");

        // check differences
        var differences = snapshot1.DifferentTo(snapshot2);
        differences.Length.ShouldBe(1);
        differences[0].Kind.ShouldBe(DifferenceKind.Removed);
        differences[0].Initial.ShouldBe(new TestRecord("a", 1L));
    }

    [Fact]
    public void Snap04_CompareSnapshots_FalseD_ValueAddedAndRemoved()
    {
        var snapshot1 = new Dictionary<string, TestRecord>()
        {
            ["a"] = new TestRecord("a", 1L),
            ["b"] = new TestRecord("b", 2L),
        };
        var snapshot2 = new Dictionary<string, TestRecord>()
        {
            ["b"] = new TestRecord("b", 2L),
            ["c"] = new TestRecord("c", 3L),
        };

        // check equality
        snapshot2.IsEqualTo(snapshot1).ShouldBe("right[c] is missing");
        snapshot1.IsEqualTo(snapshot2).ShouldBe("right[a] is missing");

        // check differences
        var differences = snapshot1.DifferentTo(snapshot2);
        differences.Length.ShouldBe(2);
        differences[0].Kind.ShouldBe(DifferenceKind.Removed);
        differences[0].Initial.ShouldBe(new TestRecord("a", 1L));
        differences[1].Kind.ShouldBe(DifferenceKind.Added);
        differences[1].Current.ShouldBe(new TestRecord("c", 3L));
    }

    private const string databaseName = "InventoryData";
    private const string testroot = @"C:\temp\unittest\RocksDB\";

}
