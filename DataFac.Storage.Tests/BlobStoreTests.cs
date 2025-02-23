using System.Threading.Tasks;
using System;
using Xunit;
using Shouldly;
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
        counters.BlobPutCount.ShouldBe(0);
        counters.BlobPutWrits.ShouldBe(0);
        counters.BlobPutSkips.ShouldBe(0);
        counters.ByteDelta.ShouldBe(0);
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
        ex.Message.ShouldStartWith("Must not be empty");
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

        ReadOnlyMemory<byte> data = new ReadOnlyMemory<byte>(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobIdV1 id = data.Span.GetBlobId();
        var result = await dataStore.GetBlob(id);
        result.ShouldBeNull();
        var counters = dataStore.GetCounters();
        counters.BlobGetCount.ShouldBe(1);
        counters.BlobGetReads.ShouldBe(1);
        counters.BlobGetCache.ShouldBe(0);
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

        ReadOnlyMemory<byte> data = default;
        await dataStore.PutBlob(data, true);

        var counters = dataStore.GetCounters();
        counters.BlobPutCount.ShouldBe(1);
        counters.BlobPutWrits.ShouldBe(1);
        counters.BlobPutSkips.ShouldBe(0);
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

        ReadOnlyMemory<byte> data = new ReadOnlyMemory<byte>(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        await dataStore.PutBlob(data, true);

        var counters = dataStore.GetCounters();
        counters.BlobPutCount.ShouldBe(1);
        counters.BlobPutWrits.ShouldBe(1);
        counters.BlobPutSkips.ShouldBe(0);
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

        ReadOnlyMemory<byte> data = new ReadOnlyMemory<byte>(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobIdV1 id = data.Span.GetBlobId();
        await dataStore.PutBlob(data, true);

        var data2 = await dataStore.GetBlob(id);
        data2.ShouldNotBeNull();
        data2.Value.ShouldBe(data);

        var counters = dataStore.GetCounters();
        counters.BlobPutCount.ShouldBe(1);
        counters.BlobPutWrits.ShouldBe(1);
        counters.BlobPutSkips.ShouldBe(0);
        counters.BlobGetCount.ShouldBe(1);
        counters.BlobGetCache.ShouldBe(1);
        counters.BlobGetReads.ShouldBe(0);
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

        ReadOnlyMemory<byte> data = new ReadOnlyMemory<byte>(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());

        // put first
        BlobIdV1 id0 = data.Span.GetBlobId();
        await dataStore.PutBlob(data, true);
        var counters1 = dataStore.GetCounters();
        counters1.BlobPutCount.ShouldBe(1);
        counters1.BlobPutWrits.ShouldBe(1);
        counters1.BlobPutSkips.ShouldBe(0);
        counters1.ByteDelta.ShouldBe(256);

        // put again
        BlobIdV1 id1 = data.Span.GetBlobId();
        id1.Equals(id0).ShouldBeTrue();
        await dataStore.PutBlob(data, true);
        var counters2 = dataStore.GetCounters();
        counters2.BlobPutCount.ShouldBe(2);
        counters2.BlobPutWrits.ShouldBe(1);
        counters2.BlobPutSkips.ShouldBe(1);
        counters2.ByteDelta.ShouldBe(256);
    }
}
