using Shouldly;
using System;
using System.Buffers;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task

namespace DataFac.Storage.Tests;

public class NameStoreTests
{
    private const string testroot = @"C:\temp\unittest\RocksDB\";

    [Theory]
    [InlineData(StoreKind.Testing)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Name01_FirstPut_WritesNewName(StoreKind storeKind)
    {
        var ct = TestContext.Current.CancellationToken;
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using INameStore nameStore = TestHelpers.CreateNameStore(storeKind, testpath);
        using IBlobStore blobStore = TestHelpers.CreateBlobStore(storeKind, testpath);

        BlobData data = BlobData.From(ReadOnlyMemory<byte>.Empty);
        Memory<byte> idMemory = new byte[BlobIdV1.Size];
        BlobHelpers.CompressData(data.Bytes, idMemory.Span);
        BlobKey key = BlobKey.From(idMemory);

        await blobStore.PutBlob(key, data);
        bool missing = nameStore.PutName("name1", key);
        missing.ShouldBeTrue();
        nameStore.GetNames().Count().ShouldBe(1);
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Name02_PutAgain_Overwrites(StoreKind storeKind)
    {
        var ct = TestContext.Current.CancellationToken;
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using INameStore nameStore = TestHelpers.CreateNameStore(storeKind, testpath);
        using IBlobStore blobStore = TestHelpers.CreateBlobStore(storeKind, testpath);

        BlobData data = BlobData.From(ReadOnlyMemory<byte>.Empty);
        Memory<byte> idMemory = new byte[BlobIdV1.Size];
        BlobHelpers.CompressData(data.Bytes, idMemory.Span);
        BlobKey key = BlobKey.From(idMemory);

        await blobStore.PutBlob(key, data);
        bool missing = nameStore.PutName("name1", key);
        missing.ShouldBeTrue();
        nameStore.GetNames().Count().ShouldBe(1);

        missing = nameStore.PutName("name1", key);
        missing.ShouldBeFalse();
        nameStore.GetNames().Count().ShouldBe(1);
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Name03_GetAndRemoveNames(StoreKind storeKind)
    {
        var ct = TestContext.Current.CancellationToken;
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using INameStore nameStore = TestHelpers.CreateNameStore(storeKind, testpath);
        using IBlobStore blobStore = TestHelpers.CreateBlobStore(storeKind, testpath);

        var names0 = nameStore.GetNames();
        names0.Count().ShouldBe(0);

        BlobData data = BlobData.From(ReadOnlyMemory<byte>.Empty);
        Memory<byte> idMemory = new byte[BlobIdV1.Size];
        BlobHelpers.CompressData(data.Bytes, idMemory.Span);
        BlobKey key = BlobKey.From(idMemory);

        await blobStore.PutBlob(key, data);
        nameStore.PutName("name1", key);
        nameStore.PutName("name2", key);
        nameStore.PutName("name2", key);

        var names1 = nameStore.GetNames().OrderBy(x => x.Key).Select(x => x.Key).ToArray();
        names1.Length.ShouldBe(2);
        names1[0].ShouldBe("name1");
        names1[1].ShouldBe("name2");

        nameStore.RemoveName("name1");
        nameStore.RemoveName("name2");

        var names2 = nameStore.GetNames();
        names2.Count().ShouldBe(0);

    }
}
