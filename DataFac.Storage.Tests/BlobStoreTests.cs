using Shouldly;
using System;
using System.Buffers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using DataFac.MemBlox2;
using DataFac.Hashing;
using DataFac.Compression;

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
    public async Task Store02GetEmptyIdReturnsNull(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        var result = await dataStore.GetBlob(default);
        result.HasValue.ShouldBeFalse();
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

        BlobData data = BlobData.From(Enumerable.Range(0, 64).Select(i => (byte)i).ToArray());
        BlobKey key = BlobKey.From(data.Bytes.ToBlobId());
        var result = await dataStore.GetBlob(key);
        result.HasValue.ShouldBeFalse();
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
    public async Task Store06PutNonEmptyBlob(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        BlobData data = BlobData.From(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobKey key = BlobKey.From(data.Bytes.ToBlobId());
        await dataStore.PutBlob(key, data, true);
        var id = BlobIdV1.FromSpan(key.Bytes.Span);
        id.IsEmbedded.ShouldBeFalse();
        id.HashAlgo.ShouldBe(BlobHashAlgo.Sha256);
        id.CompAlgo.ShouldBe(BlobCompAlgo.UnComp); // not compressible
        id.ToString().ShouldBe("V1.0:256:U:1:QK/y6dLYki5Hr9RkjmlnSXFYeF+9Hahw5xECZr+USIA=");

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
    public async Task Store08GetCompressed(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        var text =
            "The rain in Spain falls mainly on the plain. " +
            "Please explain my pain and disdain or I will go insain [sic]. " +
            "Plain Jain is a brain in a train in Spain. " +
            "Maine is the main domain to obtain the brain drain.";

        BlobData data = BlobData.From(Encoding.UTF8.GetBytes(text));
        BlobKey key = BlobKey.From(data.Bytes.ToBlobId());
        await dataStore.PutBlob(key, data, true);
        var id = BlobIdV1.FromSpan(key.Bytes.Span);
        id.IsEmbedded.ShouldBeFalse();
        id.HashAlgo.ShouldBe(BlobHashAlgo.Sha256);
        id.CompAlgo.ShouldBe(BlobCompAlgo.Snappy);
        id.ToString().ShouldBe("V1.0:201:S:1:f+8O2Wm1is/9ut73eja0VCML3qUOWA9rgBZg4INPL34=");

        var copy = await dataStore.GetBlob(key);
        copy.HasValue.ShouldBeTrue();
        string text2 = Encoding.UTF8.GetString(copy.Bytes.ToArray());
        text2.ShouldBe(text);

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
    public async Task Store08GetUncompressed(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        BlobData data = BlobData.From(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobKey key = BlobKey.From(data.Bytes.ToBlobId());
        await dataStore.PutBlob(key, data, true);
        var id = BlobIdV1.FromSpan(key.Bytes.Span);
        id.CompAlgo.ShouldBe(BlobCompAlgo.UnComp);

        var copy = await dataStore.GetBlob(key);
        copy.HasValue.ShouldBeTrue();
        copy.Bytes.Span.SequenceEqual(data.Bytes.Span).ShouldBeTrue();

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

        BlobData data = BlobData.From(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobKey key = BlobKey.From(data.Bytes.ToBlobId());

        // put first
        await dataStore.PutBlob(key, data, true);
        var counters1 = dataStore.GetCounters();
        counters1.BlobPutCount.ShouldBe(1);
        counters1.BlobPutWrits.ShouldBe(1);
        counters1.BlobPutSkips.ShouldBe(0);
        counters1.ByteDelta.ShouldBe(256);

        // put again
        await dataStore.PutBlob(key, data, true);

        var counters2 = dataStore.GetCounters();
        counters2.BlobPutCount.ShouldBe(2);
        counters2.BlobPutWrits.ShouldBe(1);
        counters2.BlobPutSkips.ShouldBe(1);
        counters2.ByteDelta.ShouldBe(256);
    }
}
