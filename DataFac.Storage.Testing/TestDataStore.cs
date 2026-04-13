using DataFac.Compression;
using DataFac.Memory;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DataFac.Storage.Testing;

/// <summary>
/// Implements an in-memory data store. Useful for unit testing.
/// </summary>
public sealed class TestDataStore : IDataStore
{
    private readonly ConcurrentDictionary<string, BlobKey> _nameStore = new ConcurrentDictionary<string, BlobKey>();
    private readonly ConcurrentDictionary<BlobKey, BlobData> _blobStore = new ConcurrentDictionary<BlobKey, BlobData>();

    public TestDataStore()
    {
    }

    public void Dispose()
    {
    }

    private Counters _counters;
    public Counters GetCounters() => _counters;
    public void ResetCounters() => _counters = default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowMustNotBeEmpty(string name)
    {
        throw new ArgumentException("Must not be empty", name);
    }

    public IEnumerable<KeyValuePair<string, BlobKey>> GetNames() => _nameStore;

    public BlobKey GetName(string name)
    {
        if (string.IsNullOrEmpty(name)) ThrowMustNotBeEmpty(nameof(name));

        return _nameStore.TryGetValue(name, out var id) ? id : BlobKey.NotFound();
    }

    public void RemoveName(string name)
    {
        _nameStore.TryRemove(name, out var _);
    }

    public void RemoveNames(IEnumerable<string> names)
    {
        if (names is null) throw new ArgumentNullException(nameof(names));

        foreach (var name in names)
        {
            _nameStore.TryRemove(name, out var _);
        }
    }

    public bool PutName(string name, in BlobKey key)
    {
        if (string.IsNullOrEmpty(name)) ThrowMustNotBeEmpty(nameof(name));

        bool added = _nameStore.TryAdd(name, key);

        if (added)
        {
            Interlocked.Increment(ref _counters.NameDelta);
        }

        return added;
    }

    public IEnumerable<KeyValuePair<BlobKey, BlobData>> GetCachedBlobs() => _blobStore;

    public IEnumerable<KeyValuePair<BlobKey, BlobData>> GetStoredBlobs() => _blobStore;

    public async ValueTask<BlobData> GetBlob(BlobKey key)
    {
        if (!key.HasValue) return BlobData.NotFound();

        //if (id.TryGetEmbeddedBlob(out var embeddedBlob))
        //    return BlobData.WithData(embeddedBlob);

        Interlocked.Increment(ref _counters.BlobGetCount);
        if (_blobStore.TryGetValue(key, out var data))
        {
            Interlocked.Increment(ref _counters.BlobGetCache);
            return data;
        }
        else
        {
            Interlocked.Increment(ref _counters.BlobGetReads);
            return BlobData.NotFound();
        }
    }

    public async ValueTask<BlobData> RemoveBlob(BlobKey key, bool withSync)
    {
        return _blobStore.TryRemove(key, out var data) ? data : BlobData.NotFound();
    }

    public ValueTask RemoveBlobs(IEnumerable<BlobKey> keys, bool withSync)
    {
        if (keys is null) throw new ArgumentNullException(nameof(keys));

        foreach (var key in keys)
        {
            _blobStore.TryRemove(key, out var _);
        }

        return default;
    }

    public ValueTask PutBlob(BlobKey key, BlobData data, bool withSync)
    {
        //if (idMemory.Length != BlobKey.Size) throw new ArgumentException($"Length must be {BlobKey.Size}.", nameof(idMemory));

        //// Snappier compression and hashing
        //// todo inline this and optimise
        //var compressResult1 = SnappyCompressor.CompressData(uncompressed, idMemory.Slice(32, 32).Span);

        //// embed compressed if small engough
        //if (compressResult1.Output.Length <= BlobKey.MaxEmbeddedSize)
        //{
        //    BlobKey.WriteEmbedded(idMemory.Span, compressResult1.CompAlgo, compressResult1.Output);
        //    return new ValueTask();
        //}

        //BlobKey.WriteSansHash(idMemory.Span, compressResult1.InputSize, compressResult1.CompAlgo, BlobHashAlgo.Sha256);

        Interlocked.Increment(ref _counters.BlobPutCount);
        // todo skip this conversion
        //var blobId = BlobKey.FromSpan(idMemory.Span);
        if (_blobStore.TryAdd(key, data))
        {
            Interlocked.Increment(ref _counters.BlobPutWrits);
            Interlocked.Add(ref _counters.ByteDelta, data.Bytes.Length);
        }
        else
        {
            Interlocked.Increment(ref _counters.BlobPutSkips);
        }

        return new ValueTask();
    }

    //public ValueTask PutBlob(string text, Memory<byte> idMemory, bool withSync = false)
    //{
    //    if (idMemory.Length != BlobKey.Size) throw new ArgumentException($"Length must be {BlobKey.Size}.", nameof(idMemory));

    //    // Snappier compression and hashing
    //    // todo inline this and optimise
    //    var compressResult1 = SnappyCompressor.CompressText(text, idMemory.Slice(32, 32).Span);

    //    // embed compressed if small engough
    //    if (compressResult1.Output.Length <= BlobKey.MaxEmbeddedSize)
    //    {
    //        BlobKey.WriteEmbedded(idMemory.Span, compressResult1.CompAlgo, compressResult1.Output);
    //        return new ValueTask();
    //    }

    //    BlobKey.WriteSansHash(idMemory.Span, compressResult1.InputSize, compressResult1.CompAlgo, BlobHashAlgo.Sha256);

    //    Interlocked.Increment(ref _counters.BlobPutCount);
    //    // todo skip this conversion
    //    var blobId = BlobKey.FromSpan(idMemory.Span);
    //    if (_blobStore.TryAdd(blobId, compressResult1.Output))
    //    {
    //        Interlocked.Increment(ref _counters.BlobPutWrits);
    //        Interlocked.Add(ref _counters.ByteDelta, compressResult1.Output.Length);
    //    }
    //    else
    //    {
    //        Interlocked.Increment(ref _counters.BlobPutSkips);
    //    }

    //    return new ValueTask();
    //}

    public ValueTask Sync() => default;

    public int ClearCache() => 0;
}
