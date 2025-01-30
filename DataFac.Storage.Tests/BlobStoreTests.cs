using System.Threading.Tasks;
using System;
using Xunit;
using FluentAssertions;
using System.Linq;
using DataFac.Storage;

#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task

namespace DataFac.Storage.Tests;

public class BlobStoreTests
{
    private const string testroot = @"C:\temp\unittest\RocksDB\";

    [Theory]
    [InlineData(StoreKind.Testing)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public void Store01Create(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        var counters = dataStore.GetCounters();
        counters.BlobPutCount.Should().Be(0);
        counters.BlobPutWrits.Should().Be(0);
        counters.BlobPutSkips.Should().Be(0);
        counters.ByteDelta.Should().Be(0);
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Store02GetEmptyIdFails(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        var ex = await Assert.ThrowsAnyAsync<ArgumentException>(async () =>
        {
            var result = await dataStore.GetBlob(default);
        });
        ex.Message.Should().StartWith("Must not be empty");
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Store03GetInvalidId(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        BlobData data = new BlobData(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobIdV1 id = data.GetId();
        var result = await dataStore.GetBlob(id);
        result.Should().BeNull();
        var counters = dataStore.GetCounters();
        counters.BlobGetCount.Should().Be(1);
        counters.BlobGetReads.Should().Be(1);
        counters.BlobGetCache.Should().Be(0);
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Store05PutEmpty(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        BlobData data = default;
        await dataStore.PutBlob(data, true);

        var counters = dataStore.GetCounters();
        counters.BlobPutCount.Should().Be(1);
        counters.BlobPutWrits.Should().Be(1);
        counters.BlobPutSkips.Should().Be(0);
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Store06PutNonEmpty(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        BlobData data = new BlobData(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        await dataStore.PutBlob(data, true);

        var counters = dataStore.GetCounters();
        counters.BlobPutCount.Should().Be(1);
        counters.BlobPutWrits.Should().Be(1);
        counters.BlobPutSkips.Should().Be(0);
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Store08Get(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        BlobData data = new BlobData(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobIdV1 id = data.GetId();
        await dataStore.PutBlob(data, true);

        var data2 = await dataStore.GetBlob(id);
        data2.Should().NotBeNull();
        data2.Value.Should().Be(data);

        var counters = dataStore.GetCounters();
        counters.BlobPutCount.Should().Be(1);
        counters.BlobPutWrits.Should().Be(1);
        counters.BlobPutSkips.Should().Be(0);
        counters.BlobGetCount.Should().Be(1);
        counters.BlobGetCache.Should().Be(1);
        counters.BlobGetReads.Should().Be(0);
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Store09PutAgain(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        BlobData data = new BlobData(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());

        // put first
        BlobIdV1 id0 = data.GetId();
        await dataStore.PutBlob(data, true);
        var counters1 = dataStore.GetCounters();
        counters1.BlobPutCount.Should().Be(1);
        counters1.BlobPutWrits.Should().Be(1);
        counters1.BlobPutSkips.Should().Be(0);
        counters1.ByteDelta.Should().Be(256);

        // put again
        BlobIdV1 id1 = data.GetId();
        id1.Equals(id0).Should().BeTrue();
        await dataStore.PutBlob(data, true);
        var counters2 = dataStore.GetCounters();
        counters2.BlobPutCount.Should().Be(2);
        counters2.BlobPutWrits.Should().Be(1);
        counters2.BlobPutSkips.Should().Be(1);
        counters2.ByteDelta.Should().Be(256);
    }
}
