using Xunit;
using FluentAssertions;
using Inventory.Store;
using System.Linq;
using System.Diagnostics;
using System;

#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task

namespace Inventory.Tests;

public class NameStoreTests
{
    private const string testroot = @"C:\temp\unittest\RocksDB\";

    [Theory]
    [InlineData(StoreKind.Testing)]
    [InlineData(StoreKind.RocksDb)]
    public void Name01_FirstPut_WritesNewName(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        BlobData data = default;
        BlobId id = data.GetBlobId();
        bool missing = dataStore.PutName("name1", id);
        missing.Should().BeTrue();
        var counters = dataStore.GetCounters();
        counters.NameDelta.Should().Be(1);
        dataStore.GetNames().Should().HaveCount(1);
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
    [InlineData(StoreKind.RocksDb)]
    public void Name02_PutAgain_Overwrites(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        BlobData data = default;
        BlobId id = data.GetBlobId();
        bool missing = dataStore.PutName("name1", id);
        missing.Should().BeTrue();
        var counters1 = dataStore.GetCounters();
        counters1.NameDelta.Should().Be(1);
        dataStore.GetNames().Should().HaveCount(1);

        missing = dataStore.PutName("name1", id);
        missing.Should().BeFalse();
        var counters2 = dataStore.GetCounters();
        counters2.NameDelta.Should().Be(1);
        dataStore.GetNames().Should().HaveCount(1);
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
    [InlineData(StoreKind.RocksDb)]
    public void Name03_GetAndRemoveNames(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        var names0 = dataStore.GetNames();
        names0.Length.Should().Be(0);

        BlobData data = default;
        BlobId id = data.GetBlobId();
        dataStore.PutName("name1", id);
        dataStore.PutName("name2", id);
        dataStore.PutName("name2", id);

        var names1 = dataStore.GetNames().OrderBy(x => x.Key).Select(x => x.Key).ToArray();
        names1.Length.Should().Be(2);
        names1[0].Should().Be("name1");
        names1[1].Should().Be("name2");

        dataStore.RemoveNames(names1);

        var names2 = dataStore.GetNames();
        names2.Length.Should().Be(0);

    }
}
