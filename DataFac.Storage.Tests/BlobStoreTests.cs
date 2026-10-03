using DataFac.Compression;
using DataFac.Hashing;
using Shouldly;
using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task

namespace DataFac.Storage.Tests;

public class BlobStoreTests
{
#if NET8_0_OR_GREATER
    private static string testroot = Directory.CreateTempSubdirectory().FullName + "\\";
#else
    private const string testroot = @"C:\temp\unittest\RocksDB\";
#endif

    [Theory]
    [InlineData(StoreKind.Testing)]
    [InlineData(StoreKind.LocalFS)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public void Store01Create(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using INameStore nameStore = TestHelpers.CreateNameStore(storeKind, testpath);
        using IBlobStore blobStore = TestHelpers.CreateBlobStore(storeKind, testpath);
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
    [InlineData(StoreKind.LocalFS)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Store02aGetInvalidKeyReturnsNotFound(StoreKind storeKind)
    {
        var ct = TestContext.Current.CancellationToken;
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using INameStore nameStore = TestHelpers.CreateNameStore(storeKind, testpath);
        var key = await nameStore.GetName("missing", ct);
        key.HasValue.ShouldBeFalse();
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
    [InlineData(StoreKind.LocalFS)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Store02bGetEmptyIdReturnsNull(StoreKind storeKind)
    {
        var ct = TestContext.Current.CancellationToken;
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IBlobStore blobStore = TestHelpers.CreateBlobStore(storeKind, testpath);
        var result = await blobStore.GetBlob(default, ct);
        result.HasValue.ShouldBeFalse();
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
    [InlineData(StoreKind.LocalFS)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Store03GetInvalidId(StoreKind storeKind)
    {
        var ct = TestContext.Current.CancellationToken;
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IBlobStore blobStore = TestHelpers.CreateBlobStore(storeKind, testpath);
        BlobData data = BlobData.From(Enumerable.Range(0, 64).Select(i => (byte)i).ToArray());
        Memory<byte> idMemory = new byte[BlobIdV1.Size];
        BlobHelpers.CompressData(data.Bytes, idMemory.Span);
        BlobKey key = BlobKey.From(idMemory);
        var result = await blobStore.GetBlob(key, ct);
        result.HasValue.ShouldBeFalse();
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
    [InlineData(StoreKind.LocalFS)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Store04PutNonEmptyBlob(StoreKind storeKind)
    {
        var ct = TestContext.Current.CancellationToken;
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IBlobStore blobStore = TestHelpers.CreateBlobStore(storeKind, testpath);
        BlobData data = BlobData.From(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        Memory<byte> idMemory = new byte[BlobIdV1.Size];
        (bool embedded, var compressed) = BlobHelpers.CompressData(data.Bytes, idMemory.Span);
        embedded.ShouldBeFalse();
        ReadOnlySpan<byte> idSpan = idMemory.Span;

        (_, _, var compAlgo, var hashAlgo, _) = BlobIdV1.ReadNonEmbedded(idSpan);
        hashAlgo.ShouldBe(BlobHashAlgo.Sha256);
        compAlgo.ShouldBe(BlobCompAlgo.UnComp);
        BlobIdV1.ToDisplayString(idSpan).ShouldBe("V1.0:256:U:S:QK/y6dLYki5Hr9RkjmlnSXFYeF+9Hahw5xECZr+USIA=");

        BlobKey key = BlobKey.From(idMemory);
        await blobStore.PutBlob(key, data, ct);
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
    [InlineData(StoreKind.LocalFS)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Store05GetCompressed(StoreKind storeKind)
    {
        var ct = TestContext.Current.CancellationToken;
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IBlobStore blobStore = TestHelpers.CreateBlobStore(storeKind, testpath);

        var text =
            "The rain in Spain falls mainly on the plain. " +
            "Please explain my pain and disdain or I will go insain [sic]. " +
            "Plain Jain is a brain in a train in Spain. " +
            "Maine is the main domain to obtain the brain drain.";

        BlobKey key;
        {
            // sender
            Memory<byte> idMemory = new byte[BlobIdV1.Size];
            var idSpan = idMemory.Span;
            (var _, var compressed) = BlobHelpers.CompressText(text, idSpan);
            key = BlobKey.From(idMemory);
            BlobData data = BlobData.From(compressed);

            (_, _, var compAlgo, var hashAlgo, _) = BlobIdV1.ReadNonEmbedded(idSpan);
            hashAlgo.ShouldBe(BlobHashAlgo.Sha256);
            compAlgo.ShouldBe(BlobCompAlgo.Snappy);
            BlobIdV1.ToDisplayString(idSpan).ShouldBe("V1.0:201:S:S:f+8O2Wm1is/9ut73eja0VCML3qUOWA9rgBZg4INPL34=");

            await blobStore.PutBlob(key, data, ct);
        }

        {
            // recver
            var recd = await blobStore.GetBlob(key, ct);
            recd.HasValue.ShouldBeTrue();

            //(bool embedded, var data) = BlobHelpers.TryGetEmbedded(key.Bytes);
            //data.HasValue.ShouldBeFalse();
            //embedded.ShouldBeFalse();

            var copy = BlobHelpers.DecompressData(key.Bytes.Span, recd.Bytes);
            string text2 = Encoding.UTF8.GetString(copy.ToArray());
            text2.ShouldBe(text);
        }
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
    [InlineData(StoreKind.LocalFS)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Store06GetUncompressed(StoreKind storeKind)
    {
        var ct = TestContext.Current.CancellationToken;
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IBlobStore blobStore = TestHelpers.CreateBlobStore(storeKind, testpath);

        BlobData data = BlobData.From(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobKey key;
        {
            // writer
            Memory<byte> idMemory = new byte[BlobIdV1.Size];
            BlobHelpers.CompressData(data.Bytes, idMemory.Span);
            key = BlobKey.From(idMemory);

            await blobStore.PutBlob(key, data, ct);
        }

        {
            // reader
            (_, _, var compAlgo, var hashAlgo, _) = BlobIdV1.ReadNonEmbedded(key.Bytes.Span);
            compAlgo.ShouldBe(BlobCompAlgo.UnComp);

            var copy = await blobStore.GetBlob(key, ct);
            copy.HasValue.ShouldBeTrue();
            copy.Bytes.Length.ShouldBe(data.Bytes.Length);
            copy.Bytes.Span.SequenceEqual(data.Bytes.Span).ShouldBeTrue();
        }
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
    [InlineData(StoreKind.LocalFS)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Store07PutAgain(StoreKind storeKind)
    {
        var ct = TestContext.Current.CancellationToken;
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IBlobStore blobStore = TestHelpers.CreateBlobStore(storeKind, testpath);

        BlobData data = BlobData.From(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        Memory<byte> idMemory = new byte[BlobIdV1.Size];
        BlobHelpers.CompressData(data.Bytes, idMemory.Span);
        BlobKey key = BlobKey.From(idMemory);

        // put first
        await blobStore.PutBlob(key, data, ct);

        // put again
        await blobStore.PutBlob(key, data, ct);
    }
}
