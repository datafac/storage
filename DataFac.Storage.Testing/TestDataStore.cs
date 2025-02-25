using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

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

    public ValueTask<ReadOnlyMemory<byte>?> GetBlob(BlobIdV1 id)
    {
        if (id.IsEmpty) ThrowMustNotBeEmpty(nameof(id));
        if (id.IsEmbedded)
        {
            return new ValueTask<ReadOnlyMemory<byte>?>(id.GetEmbeddedBlob());
        }
        Interlocked.Increment(ref _counters.BlobGetCount);
        if (_blobStore.TryGetValue(id, out var data))
        {
            Interlocked.Increment(ref _counters.BlobGetCache);
            return new ValueTask<ReadOnlyMemory<byte>?>(data);
        }
        else
        {
            Interlocked.Increment(ref _counters.BlobGetReads);
            return new ValueTask<ReadOnlyMemory<byte>?>((ReadOnlyMemory<byte>?)null);
        }
    }

    public ValueTask<ReadOnlyMemory<byte>?> RemoveBlob(BlobIdV1 id, bool withSync)
    {
        if (_blobStore.TryRemove(id, out var data))
        {
            return new ValueTask<ReadOnlyMemory<byte>?>(data);
        }
        else
        {
            return new ValueTask<ReadOnlyMemory<byte>?>((ReadOnlyMemory<byte>?)null);
        }
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

    public ValueTask<BlobIdV1> PutBlob(ReadOnlyMemory<byte> data, bool withSync)
    {
        var id = data.Span.GetBlobIdqqq();
        if (id.IsEmbedded) return new ValueTask<BlobIdV1>(id);
        Interlocked.Increment(ref _counters.BlobPutCount);
        if (_blobStore.TryAdd(id, data))
        {
            Interlocked.Increment(ref _counters.BlobPutWrits);
            Interlocked.Add(ref _counters.ByteDelta, data.Length);
        }
        else
        {
            Interlocked.Increment(ref _counters.BlobPutSkips);
        }

        return new ValueTask<BlobIdV1>(id);
    }

    public ValueTask Sync() => default;

    public int ClearCache() => 0;
}
