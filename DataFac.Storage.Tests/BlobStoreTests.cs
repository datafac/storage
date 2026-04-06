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
        result.HasData.ShouldBeFalse();
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
        result.HasData.ShouldBeFalse();
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
        id.HashAlgo.ShouldBe(BlobHashAlgo.None);
        id.CompAlgo.ShouldBe(BlobCompAlgo.UnComp);
        id.ToString().ShouldBe("U:0:");

        var counters = dataStore.GetCounters();
        counters.BlobPutCount.ShouldBe(0);
        counters.BlobPutWrits.ShouldBe(0);
        counters.BlobPutSkips.ShouldBe(0);

        var copy = await dataStore.GetBlob(id);
        copy.HasData.ShouldBeTrue();
        copy.Data.Length.ShouldBe(0);
        var origSpan = orig.Span;
        var copySpan = copy.Data.Span;
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
        id.HashAlgo.ShouldBe(BlobHashAlgo.None);
        id.CompAlgo.ShouldBe(BlobCompAlgo.UnComp);
        id.ToString().ShouldBe("U:0:");

        var counters = dataStore.GetCounters();
        counters.BlobPutCount.ShouldBe(0);
        counters.BlobPutWrits.ShouldBe(0);
        counters.BlobPutSkips.ShouldBe(0);

        var recd = await dataStore.GetBlob(id);
        recd.HasData.ShouldBeTrue();
        recd.Data.Length.ShouldBe(0);
        ReadOnlySpan<char> origSpan = orig;
#if NET8_0_OR_GREATER
        Span<char> copySpan = stackalloc char[62];
        bool decoded = Encoding.UTF8.TryGetChars(recd.Data.Span, copySpan, out int charsWritten);
        decoded.ShouldBeTrue();
        copySpan = copySpan.Slice(0, charsWritten);
#else
        string copy = Encoding.UTF8.GetString(recd.Data.ToArray());
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
        var id = await dataStore.PutBlob(blob, true);
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
    public async Task Store06PutNonEmptyText(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        string text = new string('Z', 100);
        var id = await dataStore.PutBlob(text, true);
        id.IsEmbedded.ShouldBeTrue();
        id.ToString().ShouldBe("S:9:ZABa/gEAigEA");

        var counters = dataStore.GetCounters();
        counters.BlobPutCount.ShouldBe(0);
        counters.BlobPutWrits.ShouldBe(0);
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

        BlobIdV1 id = await dataStore.PutBlob(text, true);
        id.IsEmbedded.ShouldBeFalse();
        id.HashAlgo.ShouldBe(BlobHashAlgo.Sha256);
        id.CompAlgo.ShouldBe(BlobCompAlgo.Snappy);
        id.ToString().ShouldBe("V1.0:201:S:1:f+8O2Wm1is/9ut73eja0VCML3qUOWA9rgBZg4INPL34=");

        var copy = await dataStore.GetBlob(id);
        copy.HasData.ShouldBeTrue();
        string text2 = Encoding.UTF8.GetString(copy.Data.ToArray());
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
        copy.HasData.ShouldBeTrue();
        copy.Data.ToArray().SequenceEqual(orig.ToArray()).ShouldBeTrue();

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
