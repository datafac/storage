using Xunit;
using Shouldly;
using DataFac.Storage;
using System.Linq;
using System.Diagnostics;
using System;

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
    public void Name01_FirstPut_WritesNewName(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        BlobData data = default;
        BlobIdV1 id = data.GetId();
        bool missing = dataStore.PutName("name1", id);
        missing.ShouldBeTrue();
        var counters = dataStore.GetCounters();
        counters.NameDelta.ShouldBe(1);
        dataStore.GetNames().Length.ShouldBe(1);
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public void Name02_PutAgain_Overwrites(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        BlobData data = default;
        BlobIdV1 id = data.GetId();
        bool missing = dataStore.PutName("name1", id);
        missing.ShouldBeTrue();
        var counters1 = dataStore.GetCounters();
        counters1.NameDelta.ShouldBe(1);
        dataStore.GetNames().Length.ShouldBe(1);

        missing = dataStore.PutName("name1", id);
        missing.ShouldBeFalse();
        var counters2 = dataStore.GetCounters();
        counters2.NameDelta.ShouldBe(1);
        dataStore.GetNames().Length.ShouldBe(1);
    }

    [Theory]
    [InlineData(StoreKind.Testing)]
#if NET8_0_OR_GREATER
    [InlineData(StoreKind.RocksDb)]
#endif
    public void Name03_GetAndRemoveNames(StoreKind storeKind)
    {
        string testpath = $"{testroot}{Guid.NewGuid():N}";
        using IDataStore dataStore = TestHelpers.CreateDataStore(storeKind, testpath);

        var names0 = dataStore.GetNames();
        names0.Length.ShouldBe(0);

        BlobData data = default;
        BlobIdV1 id = data.GetId();
        dataStore.PutName("name1", id);
        dataStore.PutName("name2", id);
        dataStore.PutName("name2", id);

        var names1 = dataStore.GetNames().OrderBy(x => x.Key).Select(x => x.Key).ToArray();
        names1.Length.ShouldBe(2);
        names1[0].ShouldBe("name1");
        names1[1].ShouldBe("name2");

        dataStore.RemoveNames(names1);

        var names2 = dataStore.GetNames();
        names2.Length.ShouldBe(0);

    }
}
