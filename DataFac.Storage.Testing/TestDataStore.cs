using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Storage.Testing;

/// <summary>
/// Implements an in-memory name store. Useful for unit testing.
/// </summary>
public sealed class TestNameStore : INameStore
{
    private readonly ConcurrentDictionary<string, BlobKey> _nameStore = new ConcurrentDictionary<string, BlobKey>();

    public TestNameStore()
    {
    }

    public void Dispose()
    {
    }

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
        return added;
    }

}

/// <summary>
/// Implements an in-memory blob store. Useful for unit testing.
/// </summary>
public sealed class TestBlobStore : IBlobStore
{
    private readonly ConcurrentDictionary<BlobKey, BlobData> _blobStore = new ConcurrentDictionary<BlobKey, BlobData>();

    public TestBlobStore()
    {
    }

    public void Dispose()
    {
    }

    public IEnumerable<KeyValuePair<BlobKey, BlobData>> GetCachedBlobs() => _blobStore;

    public async IAsyncEnumerable<KeyValuePair<BlobKey, BlobData>> GetBlobs(CancellationToken cancellation)
    {
        foreach (var kvp in _blobStore)
        {
            if (cancellation.IsCancellationRequested)
                yield break;

            yield return kvp;
        }
    }

    public async ValueTask<BlobData> GetBlob(BlobKey key)
    {
        if (!key.HasValue) return BlobData.NotFound();

        if (_blobStore.TryGetValue(key, out var data))
        {
            return data;
        }
        else
        {
            return BlobData.NotFound();
        }
    }

    public async ValueTask<BlobData> RemoveBlob(BlobKey key)
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

    public ValueTask PutBlob(BlobKey key, BlobData data)
    {
        _blobStore.TryAdd(key, data);
        return new ValueTask();
    }
}
