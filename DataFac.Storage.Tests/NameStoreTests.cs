using DataFac.MemBlox2;
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
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        BlobData data = BlobData.From(ReadOnlyMemory<byte>.Empty);
        Memory<byte> idMemory = new byte[BlobIdV1.Size];
        BlobHelpers.CompressData(data.Bytes, idMemory.Span);
        BlobKey key = BlobKey.From(idMemory);

        await dataStore.PutBlob(key, data, true);
        bool missing = dataStore.PutName("name1", key);
        missing.ShouldBeTrue();
        var counters = dataStore.GetCounters();
        counters.NameDelta.ShouldBe(1);
        dataStore.GetNames().Count().ShouldBe(1);
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Name02_PutAgain_Overwrites(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        BlobData data = BlobData.From(ReadOnlyMemory<byte>.Empty);
        Memory<byte> idMemory = new byte[BlobIdV1.Size];
        BlobHelpers.CompressData(data.Bytes, idMemory.Span);
        BlobKey key = BlobKey.From(idMemory);

        await dataStore.PutBlob(key, data, true);
        bool missing = dataStore.PutName("name1", key);
        missing.ShouldBeTrue();
        var counters1 = dataStore.GetCounters();
        counters1.NameDelta.ShouldBe(1);
        dataStore.GetNames().Count().ShouldBe(1);

        missing = dataStore.PutName("name1", key);
        missing.ShouldBeFalse();
        var counters2 = dataStore.GetCounters();
        counters2.NameDelta.ShouldBe(1);
        dataStore.GetNames().Count().ShouldBe(1);
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public async Task Name03_GetAndRemoveNames(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        var names0 = dataStore.GetNames();
        names0.Count().ShouldBe(0);

        BlobData data = BlobData.From(ReadOnlyMemory<byte>.Empty);
        Memory<byte> idMemory = new byte[BlobIdV1.Size];
        BlobHelpers.CompressData(data.Bytes, idMemory.Span);
        BlobKey key = BlobKey.From(idMemory);

        await dataStore.PutBlob(key, data, true);
        dataStore.PutName("name1", key);
        dataStore.PutName("name2", key);
        dataStore.PutName("name2", key);

        var names1 = dataStore.GetNames().OrderBy(x => x.Key).Select(x => x.Key).ToArray();
        names1.Length.ShouldBe(2);
        names1[0].ShouldBe("name1");
        names1[1].ShouldBe("name2");

        dataStore.RemoveName("name1");
        dataStore.RemoveName("name2");

        var names2 = dataStore.GetNames();
        names2.Count().ShouldBe(0);

    }
}
