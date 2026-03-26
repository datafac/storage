using DataFac.Memory;
using Shouldly;
using System;
using System.Buffers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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
        result.IsEmpty.ShouldBeTrue();
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

        var data = new ReadOnlyMemory<byte>(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobIdV1 id = data.TryCompressBlob().BlobId;
        var result = await dataStore.GetBlob(id);
        result.IsEmpty.ShouldBeTrue();
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
    public async Task Store05PutEmptyBlob(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        ReadOnlyMemory<byte> orig = default;
        var id = await dataStore.PutBlob(orig, true);
        id.IsEmbedded.ShouldBeTrue();

        id.ToString().ShouldBe("U:0:");

        var counters = dataStore.GetCounters();
        counters.BlobPutCount.ShouldBe(0);
        counters.BlobPutWrits.ShouldBe(0);
        counters.BlobPutSkips.ShouldBe(0);

        var copy = await dataStore.GetBlob(id);
        copy.IsEmpty.ShouldBeTrue();
        copy.Length.ShouldBe(0);
        var origSpan = orig.Span;
        var copySpan = copy.Span;
        copySpan.SequenceEqual(origSpan).ShouldBeTrue();
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Store05PutEmptyText(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        string orig = string.Empty;
        var id = await dataStore.PutBlob(orig, true);
        id.IsEmbedded.ShouldBeTrue();

        id.ToString().ShouldBe("U:0:");

        var counters = dataStore.GetCounters();
        counters.BlobPutCount.ShouldBe(0);
        counters.BlobPutWrits.ShouldBe(0);
        counters.BlobPutSkips.ShouldBe(0);

        var recd = await dataStore.GetBlob(id);
        recd.IsEmpty.ShouldBeTrue();
        recd.Length.ShouldBe(0);
        ReadOnlySpan<char> origSpan = orig;
#if NET8_0_OR_GREATER
        Span<char> copySpan = stackalloc char[62];
        bool decoded = Encoding.UTF8.TryGetChars(recd.Span, copySpan, out int charsWritten);
        decoded.ShouldBeTrue();
        copySpan = copySpan.Slice(0, charsWritten);
#else
        string copy = Encoding.UTF8.GetString(recd.ToArray());
        ReadOnlySpan<char> copySpan = copy;
#endif
        copySpan.SequenceEqual(origSpan).ShouldBeTrue();
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

        var blob = new ReadOnlyMemory<byte>(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        await dataStore.PutBlob(blob, true);

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
    public async Task Store06PutNonEmptyText(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        string text = string.Join("-", Enumerable.Range(0, 64).Select(i => $"{i:X2}"));
        await dataStore.PutBlob(text, true);

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
            """
            The rain in Spain falls mainly on the plain.
            Please explain my pain and disdain or I will go insain [sic].
            Plain Jain is a brain in a train in Spain.
            Maine is the main domain to obtain the brain drain.";
            """;
        BlobIdV1 id = await dataStore.PutBlob(text, true);
        id.CompAlgo.ShouldBe(BlobCompAlgo.Snappy);

        var copy = await dataStore.GetBlob(id);
        copy.IsEmpty.ShouldBeFalse();
        string text2 = Encoding.UTF8.GetString(copy.ToArray());
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

        var orig = new ReadOnlyMemory<byte>(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobIdV1 id = await dataStore.PutBlob(orig, true);
        id.CompAlgo.ShouldBe(BlobCompAlgo.UnComp);

        var copy = await dataStore.GetBlob(id);
        copy.IsEmpty.ShouldBeFalse();
        copy.ToArray().SequenceEqual(orig.ToArray()).ShouldBeTrue();

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

        var data = new ReadOnlyMemory<byte>(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());

        // put first
        BlobIdV1 id1 = await dataStore.PutBlob(data, true);
        var counters1 = dataStore.GetCounters();
        counters1.BlobPutCount.ShouldBe(1);
        counters1.BlobPutWrits.ShouldBe(1);
        counters1.BlobPutSkips.ShouldBe(0);
        counters1.ByteDelta.ShouldBe(256);

        // put again
        BlobIdV1 id2 = await dataStore.PutBlob(data, true);
        id2.Equals(id1).ShouldBeTrue();

        var counters2 = dataStore.GetCounters();
        counters2.BlobPutCount.ShouldBe(2);
        counters2.BlobPutWrits.ShouldBe(1);
        counters2.BlobPutSkips.ShouldBe(1);
        counters2.ByteDelta.ShouldBe(256);
    }
}
