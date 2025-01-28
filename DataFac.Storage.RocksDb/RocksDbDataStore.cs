using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using RocksDbSharp;

namespace Inventory.Store.RocksDbStore;

public sealed class RocksDbDataStore : IDataStore
{
#pragma warning disable CA2213 // Disposable fields should be disposed
    private readonly RocksDb _rocksBlobDb;
    private readonly RocksDb _rocksNameDb;
#pragma warning restore CA2213 // Disposable fields should be disposed

    private readonly ConcurrentDictionary<BlobId, BlobData> _blobCache = new ConcurrentDictionary<BlobId, BlobData>();
    private readonly ChannelWriter<AsyncOp> _writer;
    private readonly ChannelReader<AsyncOp> _reader;

    public RocksDbDataStore(string rootpath)
    {
        DbOptions dbOptions = new DbOptions().SetCreateIfMissing(true);

        string namePath = $"{rootpath}\\names";
        Directory.CreateDirectory(namePath);
        _rocksNameDb = RocksDb.Open(dbOptions, namePath);

        string blobPath = $"{rootpath}\\blobs";
        Directory.CreateDirectory(blobPath);
        _rocksBlobDb = RocksDb.Open(dbOptions, blobPath);

        // async get/put queue
        var putQueue = Channel.CreateUnbounded<AsyncOp>(new UnboundedChannelOptions() { SingleReader = true });
        _writer = putQueue.Writer;
        _reader = putQueue.Reader;
#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
        _ = Task.Factory.StartNew(DequeueLoop);
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler
    }

    private volatile bool _disposed;
    public void Dispose()
    {
        if(_disposed) return;
        _disposed = true;
        _writer.TryComplete();
        _rocksNameDb.Dispose();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowDisposedException(string? memberName)
    {
        throw new ObjectDisposedException(null, $"Cannot call '{memberName}' when disposed");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed([CallerMemberName] string? memberName = null)
    {
        if (_disposed) ThrowDisposedException(memberName);
    }

    private Counters _counters;
    public Counters GetCounters() => _counters;
    public void ResetCounters() => _counters = default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowMustNotBeEmpty(string name)
    {
        throw new ArgumentException("Must not be empty", name);
    }

    public KeyValuePair<string, BlobId>[] GetNames()
    {
        var list = new List<KeyValuePair<string, BlobId>>();
        using var iter = _rocksNameDb.NewIterator();
        var iter2 = iter.SeekToFirst();
        while (iter2.Valid())
        {
#if NET8_0_OR_GREATER
            string key = Encoding.UTF8.GetString(iter2.GetKeySpan());
            BlobId value = new BlobId(iter2.GetValueSpan());
#else
            string key = Encoding.UTF8.GetString(iter2.Key());
            BlobId value = new BlobId(iter2.Value().AsSpan());
#endif
            list.Add(new KeyValuePair<string, BlobId>(key, value));
            iter2 = iter2.Next();
        }
        return list.ToArray();
    }

    public BlobId? GetName(string key)
    {
        if (string.IsNullOrEmpty(key)) ThrowMustNotBeEmpty(nameof(key));
        var keyBytes = Encoding.UTF8.GetBytes(key);
#if NET8_0_OR_GREATER
        ReadOnlySpan<byte> keySpan = keyBytes.AsSpan();
        var currentBytes = _rocksNameDb.Get(keySpan);
#else
        var currentBytes = _rocksNameDb.Get(keyBytes);
#endif
        return currentBytes is null ? null : BlobId.UnsafeWrap(currentBytes);
    }

    public void RemoveName(string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
#if NET8_0_OR_GREATER
        ReadOnlySpan<byte> keySpan = keyBytes.AsSpan();
        _rocksNameDb.Remove(keySpan);
#else
        _rocksNameDb.Remove(keyBytes);
#endif
    }

    public void RemoveNames(IEnumerable<string> keys)
    {
        if (keys is null) throw new ArgumentNullException(nameof(keys));

        foreach (var key in keys)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
#if NET8_0_OR_GREATER
            ReadOnlySpan<byte> keySpan = keyBytes.AsSpan();
            _rocksNameDb.Remove(keySpan);
#else
            _rocksNameDb.Remove(keyBytes);
#endif
        }
    }

    public bool PutName(string key, in BlobId id)
    {
        if (string.IsNullOrEmpty(key)) ThrowMustNotBeEmpty(nameof(key));
        // todo? optimistic locking revision check
        // todo? lock on key to ensure below is atomic
#if NET8_0_OR_GREATER
        var keyBytes = Encoding.UTF8.GetBytes(key);
        ReadOnlySpan<byte> keySpan = keyBytes.AsSpan();
        bool added = _rocksNameDb.Get(keySpan) is null;
        if (added)
        {
            _rocksNameDb.Put(keySpan, id.Id.Span);
        }
#else
        var keyBytes = Encoding.UTF8.GetBytes(key);
        bool added = _rocksNameDb.Get(keyBytes) is null;
        if (added)
        {
            _rocksNameDb.Put(keyBytes, id.Id.ToArray());
        }
#endif
        if (added)
        {
            Interlocked.Increment(ref _counters.NameDelta);
        }
        return added;
    }

    public KeyValuePair<BlobId, BlobData>[] GetCachedBlobs() => _blobCache.ToArray();

    public KeyValuePair<BlobId, BlobData>[] GetStoredBlobs()
    {
        var list = new List<KeyValuePair<BlobId, BlobData>>();
        using var iter = _rocksBlobDb.NewIterator();
        var iter2 = iter.SeekToFirst();
        while (iter2.Valid())
        {
#if NET8_0_OR_GREATER
            BlobId key = new BlobId(iter2.GetKeySpan());
            BlobData value = new BlobData(iter2.GetValueSpan());
#else
            BlobId key = new BlobId(iter2.Key());
            BlobData value = new BlobData(iter2.Value());
#endif
            list.Add(new KeyValuePair<BlobId, BlobData>(key, value));
            iter2 = iter2.Next();
        }
        return list.ToArray();
    }

    public async ValueTask<BlobData?> GetBlob(BlobId id)
    {
        ThrowIfDisposed();
        if (id.IsEmpty) ThrowMustNotBeEmpty(nameof(id));
        Interlocked.Increment(ref _counters.BlobGetCount);

        if (_blobCache.TryGetValue(id, out var cachedBlob))
        {
            Interlocked.Increment(ref _counters.BlobGetCache);
            return cachedBlob;
        }
        else
        {
            Interlocked.Increment(ref _counters.BlobGetReads);
        }

        // enqueue get
        var complete = new TaskCompletionSource<BlobData?>();
        _writer.TryWrite(new AsyncOp(id, complete));
        var data = await complete.Task.ConfigureAwait(false);
        if (data is not null)
        {
            _blobCache.TryAdd(id, data.Value);
        }
        return data;
    }

    public async ValueTask<BlobData?> RemoveBlob(BlobId id, bool withSync)
    {
        ThrowIfDisposed();

        _blobCache.TryRemove(id, out var _);

        // enqueue remove
        if (withSync)
        {
            var complete = new TaskCompletionSource<BlobData?>();
            _writer.TryWrite(new AsyncOp(AsyncOpKind.Del, id, default, null));
            var data = await complete.Task.ConfigureAwait(false);
            return data;
        }
        else
        {
            _writer.TryWrite(new AsyncOp(AsyncOpKind.Del, id, default, null));
            return null;
        }
    }

    public ValueTask RemoveBlobs(IEnumerable<BlobId> ids, bool withSync)
    {
        if (ids is null) throw new ArgumentNullException(nameof(ids));

        ThrowIfDisposed();

        foreach (var id in ids)
        {
            _blobCache.TryRemove(id, out var _);
            _writer.TryWrite(new AsyncOp(AsyncOpKind.Del, id, default, null));
        }

        if (withSync)
        {
            var complete = new TaskCompletionSource<BlobData?>();
            _writer.TryWrite(new AsyncOp(complete));
            return new ValueTask(complete.Task);
        }
        else
        {
            return default;
        }
    }

    public async ValueTask<BlobId> PutBlob(BlobData data, bool withSync)
    {
        ThrowIfDisposed();
        Interlocked.Increment(ref _counters.BlobPutCount);
        var id = data.GetBlobId();
        if (!_blobCache.TryAdd(id, data))
        {
            Interlocked.Increment(ref _counters.BlobPutSkips);
            return id;
        }
        else
        {

        }

        // added to cache - enqueue put
        if (withSync)
        {
            TaskCompletionSource<BlobData?>? complete = new TaskCompletionSource<BlobData?>();
            _writer.TryWrite(new AsyncOp(id, data, complete));
            await complete.Task.ConfigureAwait(false);
            return id;
        }
        else
        {
            _writer.TryWrite(new AsyncOp(id, data, null));
            return id;
        }
    }

    public ValueTask Sync()
    {
        ThrowIfDisposed();
        // enqueue sync
        var complete = new TaskCompletionSource<BlobData?>();
        _writer.TryWrite(new AsyncOp(complete));
        return new ValueTask(complete.Task);
    }

    public int ClearCache()
    {
        int count = _blobCache.Count;
        _blobCache.Clear();
        return count;
    }

    private async void DequeueLoop()
    {
        await foreach (AsyncOp item in _reader.ReadAllAsync().ConfigureAwait(false))
        {
#pragma warning disable CA1031 // Do not catch all
            try
            {
                if (item.Kind == AsyncOpKind.Get)
                {
                    // async get
                    var data = await InternalGetBlob(item.Id).ConfigureAwait(false);
                    item.Completion?.SetResult(data);
                }
                else if (item.Kind == AsyncOpKind.Putqqq)
                {
                    // async put
                    await InternalPutBlob(item.Id, item.Data).ConfigureAwait(false);
                    item.Completion?.TrySetResult(null);
                }
                else if (item.Kind == AsyncOpKind.Del)
                {
                    // async del
                    var data = await InternalDelBlob(item.Id).ConfigureAwait(false);
                    item.Completion?.TrySetResult(data);
                }
                else
                {
                    // assume sync
                    item.Completion?.SetResult(null);
                }
            }
            catch (OperationCanceledException e)
            {
                item.Completion?.TrySetCanceled(e.CancellationToken);
            }
            catch (Exception e)
            {
                item.Completion?.TrySetException(e);
            }
#pragma warning restore CA1031
        }
        _rocksBlobDb.Dispose();
    }

    private ValueTask<BlobData?> InternalGetBlob(in BlobId id)
    {
#if NET8_0_OR_GREATER
        byte[] currentBytes = _rocksBlobDb.Get(id.Id.Span);
#else
        byte[] currentBytes = _rocksBlobDb.Get(id.Id.ToArray());
#endif
        return currentBytes is null
            ? new ValueTask<BlobData?>((BlobData?)null)
            : new ValueTask<BlobData?>(BlobData.UnsafeWrap(currentBytes));
    }

    private ValueTask InternalPutBlob(in BlobId id, in BlobData data)
    {
#if NET8_0_OR_GREATER
        var key = id.Id.Span;
        byte[] currentBytes = _rocksBlobDb.Get(key);
#else
        var key = id.Id.ToArray();
        byte[] currentBytes = _rocksBlobDb.Get(key);
#endif
        if (currentBytes is null)
        {
            // adding blob
#if NET8_0_OR_GREATER
            _rocksBlobDb.Put(key, data.Memory.Span);
#else
            _rocksBlobDb.Put(key, data.Memory.ToArray());
#endif
            Interlocked.Increment(ref _counters.BlobPutWrits);
            Interlocked.Add(ref _counters.ByteDelta, data.Memory.Length);
        }
        else
        {
            Interlocked.Increment(ref _counters.BlobPutSkips);
        }
        return default;
    }

    private ValueTask<BlobData?> InternalDelBlob(in BlobId id)
    {
#if NET8_0_OR_GREATER
        var key = id.Id.Span;
        byte[] currentBytes = _rocksBlobDb.Get(key);
#else
        var key = id.Id.ToArray();
        byte[] currentBytes = _rocksBlobDb.Get(key);
#endif
        if (currentBytes is null)
        {
            return new ValueTask<BlobData?>((BlobData?)null);
        }
        else
        {
            _rocksBlobDb.Remove(key);
            return new ValueTask<BlobData?>(BlobData.UnsafeWrap(currentBytes));
        }
    }
}
