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
    private readonly ConcurrentDictionary<string, BlobIdV1> _nameStore = new ConcurrentDictionary<string, BlobIdV1>();
    private readonly ConcurrentDictionary<BlobIdV1, ReadOnlyMemory<byte>> _blobStore = new ConcurrentDictionary<BlobIdV1, ReadOnlyMemory<byte>>();

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

    public KeyValuePair<string, BlobIdV1>[] GetNames() => _nameStore.ToArray();

    public BlobIdV1? GetName(string key)
    {
        if (string.IsNullOrEmpty(key)) ThrowMustNotBeEmpty(nameof(key));

        return _nameStore.TryGetValue(key, out var id) ? id : null;
    }

    public void RemoveName(string key)
    {
        _nameStore.TryRemove(key, out var _);
    }

    public void RemoveNames(IEnumerable<string> keys)
    {
        if (keys is null) throw new ArgumentNullException(nameof(keys));

        foreach (var key in keys)
        {
            _nameStore.TryRemove(key, out var _);
        }
    }

    public bool PutName(string key, in BlobIdV1 id)
    {
        if (string.IsNullOrEmpty(key)) ThrowMustNotBeEmpty(nameof(key));

        bool added = _nameStore.TryAdd(key, id);

        if (added)
        {
            Interlocked.Increment(ref _counters.NameDelta);
        }

        return added;
    }

    public KeyValuePair<BlobIdV1, ReadOnlyMemory<byte>>[] GetCachedBlobs() => _blobStore.ToArray();

    public KeyValuePair<BlobIdV1, ReadOnlyMemory<byte>>[] GetStoredBlobs() => _blobStore.ToArray();

    public async ValueTask<BlobResult> GetBlob(BlobIdV1 id)
    {
        if (id.IsDefault)
            return BlobResult.NotFound();

        if (id.TryGetEmbeddedBlob(out var embeddedBlob))
            return BlobResult.WithData(embeddedBlob);

        Interlocked.Increment(ref _counters.BlobGetCount);
        if (_blobStore.TryGetValue(id, out var data))
        {
            Interlocked.Increment(ref _counters.BlobGetCache);
            return BlobResult.WithData(BlobHelpers.TryDecompressBlob(id, data));
        }
        else
        {
            Interlocked.Increment(ref _counters.BlobGetReads);
            return BlobResult.NotFound();
        }
    }

    public async ValueTask<BlobResult> RemoveBlob(BlobIdV1 id, bool withSync)
    {
        return _blobStore.TryRemove(id, out var data)
            ? BlobResult.WithData(data)
            : BlobResult.NotFound();
    }

    public ValueTask RemoveBlobs(IEnumerable<BlobIdV1> ids, bool withSync)
    {
        if (ids is null) throw new ArgumentNullException(nameof(ids));

        foreach (var id in ids)
        {
            _blobStore.TryRemove(id, out var _);
        }

        return default;
    }

    public ValueTask PutBlob(ReadOnlyMemory<byte> uncompressed, Memory<byte> idMemory, bool withSync)
    {
        if (idMemory.Length != BlobIdV1.Size) throw new ArgumentException($"Length must be {BlobIdV1.Size}.", nameof(idMemory));

        // Snappier compression and hashing
        // todo inline this and optimise
        var compressResult1 = SnappyCompressor.CompressData(uncompressed, idMemory.Slice(32, 32).Span);

        // embed compressed if small engough
        if (compressResult1.Output.Length <= BlobIdV1.MaxEmbeddedSize)
        {
            BlobIdV1.WriteEmbedded(idMemory.Span, compressResult1.CompAlgo, compressResult1.Output);
            return new ValueTask();
        }

        BlobIdV1.WriteSansHash(idMemory.Span, compressResult1.InputSize, compressResult1.CompAlgo, BlobHashAlgo.Sha256);

        Interlocked.Increment(ref _counters.BlobPutCount);
        // todo skip this conversion
        var blobId = BlobIdV1.FromSpan(idMemory.Span);
        if (_blobStore.TryAdd(blobId, compressResult1.Output))
        {
            Interlocked.Increment(ref _counters.BlobPutWrits);
            Interlocked.Add(ref _counters.ByteDelta, compressResult1.Output.Length);
        }
        else
        {
            Interlocked.Increment(ref _counters.BlobPutSkips);
        }

        return new ValueTask();
    }

    public ValueTask PutBlob(string text, Memory<byte> idMemory, bool withSync = false)
    {
        if (idMemory.Length != BlobIdV1.Size) throw new ArgumentException($"Length must be {BlobIdV1.Size}.", nameof(idMemory));

        // Snappier compression and hashing
        // todo inline this and optimise
        var compressResult1 = SnappyCompressor.CompressText(text, idMemory.Slice(32, 32).Span);

        // embed compressed if small engough
        if (compressResult1.Output.Length <= BlobIdV1.MaxEmbeddedSize)
        {
            BlobIdV1.WriteEmbedded(idMemory.Span, compressResult1.CompAlgo, compressResult1.Output);
            return new ValueTask();
        }

        BlobIdV1.WriteSansHash(idMemory.Span, compressResult1.InputSize, compressResult1.CompAlgo, BlobHashAlgo.Sha256);

        Interlocked.Increment(ref _counters.BlobPutCount);
        // todo skip this conversion
        var blobId = BlobIdV1.FromSpan(idMemory.Span);
        if (_blobStore.TryAdd(blobId, compressResult1.Output))
        {
            Interlocked.Increment(ref _counters.BlobPutWrits);
            Interlocked.Add(ref _counters.ByteDelta, compressResult1.Output.Length);
        }
        else
        {
            Interlocked.Increment(ref _counters.BlobPutSkips);
        }

        return new ValueTask();
    }

    public ValueTask Sync() => default;

    public int ClearCache() => 0;
}
