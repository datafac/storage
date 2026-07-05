using RocksDbSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DataFac.Storage.RocksDbStore;

public sealed class BlobCache : IBlobCache
{
    private readonly IBlobStore _blobStore;
    private readonly bool _disposeStore = false;

    private readonly ConcurrentDictionary<BlobKey, BlobData> _blobCache = new ConcurrentDictionary<BlobKey, BlobData>();
    private readonly ChannelWriter<AsyncOp> _writer;
    private readonly ChannelReader<AsyncOp> _reader;

    public BlobCache(IBlobStore blobStore, bool disposeStore)
    {
        _blobStore = blobStore;
        _disposeStore = disposeStore;

        // async get/put queue
        var putQueue = Channel.CreateUnbounded<AsyncOp>(new UnboundedChannelOptions() { SingleReader = true });
        _writer = putQueue.Writer;
        _reader = putQueue.Reader;
#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
        _ = Task.Factory.StartNew(DequeueLoop);
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler
    }

    private volatile bool _disposed;
    ///<inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _writer.TryComplete();
        if (_disposeStore)
            _blobStore.Dispose();
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

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowMustNotBeEmpty(string paramName)
    {
        throw new ArgumentException("Must not be empty", paramName);
    }

    ///<inheritdoc/>
    public int Clear()
    {
        int count = _blobCache.Count;
        _blobCache.Clear();
        return count;
    }

    public ValueTask Sync()
    {
        ThrowIfDisposed();
        // enqueue sync
        var complete = new TaskCompletionSource<BlobData>();
        _writer.TryWrite(AsyncOp.Sync(complete));
        return new ValueTask(complete.Task);
    }

    public async ValueTask<BlobData> GetBlob(BlobKey key)
    {
        ThrowIfDisposed();
        if (!key.HasValue) return BlobData.NotFound();
        //Interlocked.Increment(ref _counters.BlobGetCount);
        if (_blobCache.TryGetValue(key, out var data))
        {
            //Interlocked.Increment(ref _counters.BlobGetCache);
            return data;
        }
        else
        {
            //Interlocked.Increment(ref _counters.BlobGetReads);
        }

        // enqueue get
        var complete = new TaskCompletionSource<BlobData>();
        _writer.TryWrite(AsyncOp.Get(key, complete));
        var result = await complete.Task.ConfigureAwait(false);
        _blobCache.TryAdd(key, result);
        return result;
    }

    public async ValueTask PutBlob(BlobKey key, BlobData data, bool withSync)
    {
        ThrowIfDisposed();

        if (!key.HasValue) ThrowMustNotBeEmpty(nameof(key));
        if (!data.HasValue) ThrowMustNotBeEmpty(nameof(data));

        //Interlocked.Increment(ref _counters.BlobPutCount);
        if (!_blobCache.TryAdd(key, data))
        {
            // already in cache - skip put
            //Interlocked.Increment(ref _counters.BlobPutSkips);
            return;
        }

        // added to cache - enqueue put
        if (withSync)
        {
            var complete = new TaskCompletionSource<BlobData>();
            _writer.TryWrite(AsyncOp.Put(key, data, complete));
            await complete.Task.ConfigureAwait(false);
        }
        else
        {
            _writer.TryWrite(AsyncOp.Put(key, data, null));
        }
    }

    public async ValueTask<BlobData> RemoveBlob(BlobKey key, bool withSync)
    {
        ThrowIfDisposed();

        if (!key.HasValue) ThrowMustNotBeEmpty(nameof(key));

        _blobCache.TryRemove(key, out var _);

        // enqueue remove
        if (withSync)
        {
            var complete = new TaskCompletionSource<BlobData>();
            _writer.TryWrite(AsyncOp.Del(key, complete));
            return await complete.Task.ConfigureAwait(false);
        }
        else
        {
            _writer.TryWrite(AsyncOp.Del(key, null));
            return BlobData.NotFound();
        }
    }

    private async void DequeueLoop()
    {
        await foreach (AsyncOp item in _reader.ReadAllAsync().ConfigureAwait(false))
        {
#pragma warning disable CA1031 // Do not catch all
            try
            {
                switch (item.Kind)
                {
                    case AsyncOpKind.Get:
                        {
                            // async get
                            var result = await _blobStore.GetBlob(item.Key).ConfigureAwait(false);
                            //var result = InternalGetBlob(item.Key);
                            item.Completion?.TrySetResult(result);
                            break;
                        }
                    case AsyncOpKind.Put:
                        {
                            // async put
                            //InternalPutBlob(item.Key, item.Data);
                            await _blobStore.PutBlob(item.Key, item.Data).ConfigureAwait(false);
                            item.Completion?.TrySetResult(item.Data);
                            break;
                        }
                    case AsyncOpKind.Del:
                        {
                            // async del
                            //var data = InternalDelBlob(item.Key);
                            var data = await _blobStore.RemoveBlob(item.Key).ConfigureAwait(false);
                            item.Completion?.TrySetResult(data);
                            break;
                        }
                    default:
                        {
                            // assume sync
                            item.Completion?.TrySetResult(BlobData.NotFound());
                            break;
                        }
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
        if (_disposeStore)
        {
            _blobStore.Dispose();
        }
    }
}

public sealed class RocksDbDataStore : IDataStore
{
#pragma warning disable CA2213 // Disposable fields should be disposed
    private readonly RocksDb _rocksBlobDb;
    private readonly RocksDb _rocksNameDb;
#pragma warning restore CA2213 // Disposable fields should be disposed

    private const int MaxStackallocKeySize = 128; // todo tune size

    public RocksDbDataStore(string rootpath)
    {
        DbOptions dbOptions = new DbOptions().SetCreateIfMissing(true);

        string namePath = $"{rootpath}\\names";
        Directory.CreateDirectory(namePath);
        _rocksNameDb = RocksDb.Open(dbOptions, namePath);

        string blobPath = $"{rootpath}\\blobs";
        Directory.CreateDirectory(blobPath);
        _rocksBlobDb = RocksDb.Open(dbOptions, blobPath);

    }

    private volatile bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
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

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowMustNotBeEmpty(string paramName)
    {
        throw new ArgumentException("Must not be empty", paramName);
    }

#if NET8_0_OR_GREATER
    public IEnumerable<KeyValuePair<string, BlobKey>> GetNames()
    {
        using var iter = _rocksNameDb.NewIterator();
        var iter2 = iter.SeekToFirst();
        while (iter2.Valid())
        {
            string name = Encoding.UTF8.GetString(iter2.GetKeySpan());
            BlobKey key = BlobKey.From(iter2.Value());
            yield return new KeyValuePair<string, BlobKey>(name, key);
            iter2 = iter2.Next();
        }
    }
#else
    public IEnumerable<KeyValuePair<string, BlobKey>> GetNames()
    {
        using var iter = _rocksNameDb.NewIterator();
        var iter2 = iter.SeekToFirst();
        while (iter2.Valid())
        {
            string name = Encoding.UTF8.GetString(iter2.Key());
            BlobKey key = BlobKey.From(iter2.Value());
            yield return new KeyValuePair<string, BlobKey>(name, key);
            iter2 = iter2.Next();
        }
    }
#endif

#if NET8_0_OR_GREATER
    public BlobKey GetName(string name)
    {
        if (string.IsNullOrEmpty(name)) ThrowMustNotBeEmpty(nameof(name));
        Span<byte> buffer = stackalloc byte[MaxStackallocKeySize];
        if (Encoding.UTF8.TryGetBytes(name, buffer, out int bytesInKey))
        {
            var keySpan = buffer.Slice(0, bytesInKey);
            var bytes1 = _rocksNameDb.Get(keySpan);
            return bytes1 is null ? BlobKey.NotFound() : BlobKey.From(bytes1);
        }
        // fallback if key too large for stackalloc
        var keyBytes = Encoding.UTF8.GetBytes(name);
        var bytes2 = _rocksNameDb.Get(keyBytes);
        return bytes2 is null ? BlobKey.NotFound() : BlobKey.From(bytes2);
    }
#else
    public BlobKey GetName(string name)
    {
        var keyBytes = Encoding.UTF8.GetBytes(name);
        var bytes2 = _rocksNameDb.Get(keyBytes);
        return bytes2 is null ? BlobKey.NotFound() : BlobKey.From(bytes2);
    }
#endif

#if NET8_0_OR_GREATER
    public void RemoveName(string name)
    {
        if (string.IsNullOrEmpty(name)) ThrowMustNotBeEmpty(nameof(name));
        Span<byte> buffer = stackalloc byte[MaxStackallocKeySize];
        if (Encoding.UTF8.TryGetBytes(name, buffer, out int bytesInKey))
        {
            var keySpan = buffer.Slice(0, bytesInKey);
            _rocksNameDb.Remove(keySpan);
        }
        // fallback if key too large for stackalloc
        var keyBytes = Encoding.UTF8.GetBytes(name);
        _rocksNameDb.Remove(keyBytes);
    }
#else
    public void RemoveName(string name)
    {
        var keyBytes = Encoding.UTF8.GetBytes(name);
        _rocksNameDb.Remove(keyBytes);
    }
#endif

#if NET8_0_OR_GREATER
    public bool PutName(string name, in BlobKey key)
    {
        if (string.IsNullOrEmpty(name)) ThrowMustNotBeEmpty(nameof(name));
        if (!key.HasValue) ThrowMustNotBeEmpty(nameof(key));
        // todo? optimistic locking revision check
        // todo? lock on key to ensure below is atomic
        Span<byte> buffer = stackalloc byte[MaxStackallocKeySize];
        if (Encoding.UTF8.TryGetBytes(name, buffer, out int bytesInKey))
        {
            var keySpan = buffer.Slice(0, bytesInKey);
            bool added = _rocksNameDb.Get(keySpan) is null;
            if (added)
            {
                _rocksNameDb.Put(keySpan, key.Bytes.Span);
            }
            return added;
        }
        {
            var keyBytes = Encoding.UTF8.GetBytes(name);
            bool added = _rocksNameDb.Get(keyBytes) is null;
            if (added)
            {
                _rocksNameDb.Put(keyBytes, key.Bytes.ToArray());
            }
            return added;
        }
    }
#else
    public bool PutName(string name, in BlobKey key)
    {
        if (string.IsNullOrEmpty(name)) ThrowMustNotBeEmpty(nameof(name));
        if (!key.HasValue) ThrowMustNotBeEmpty(nameof(key));
        // todo? optimistic locking revision check
        // todo? lock on key to ensure below is atomic
        {
            var keyBytes = Encoding.UTF8.GetBytes(name);
            bool added = _rocksNameDb.Get(keyBytes) is null;
            if (added)
            {
                _rocksNameDb.Put(keyBytes, key.Bytes.ToArray());
            }
            return added;
        }
    }
#endif

    public async IAsyncEnumerable<KeyValuePair<BlobKey, BlobData>> GetBlobs(CancellationToken cancellation)
    {
        ThrowIfDisposed();
        using var iter = _rocksBlobDb.NewIterator();
        var iter2 = iter.SeekToFirst();
        while (iter2.Valid())
        {
            if (cancellation.IsCancellationRequested) yield break;
            BlobKey key = BlobKey.From(iter2.Key());
            BlobData data = BlobData.From(iter2.Value());
            yield return new KeyValuePair<BlobKey, BlobData>(key, data);
            iter2 = iter2.Next();
        }
    }

    public async ValueTask<BlobData> GetBlob(BlobKey key)
    {
        ThrowIfDisposed();
        if (!key.HasValue) return BlobData.NotFound();
#if NET8_0_OR_GREATER
        byte[] data = _rocksBlobDb.Get(key.Bytes.Span);
#else
        byte[] data = _rocksBlobDb.Get(key.Bytes.ToArray());
#endif
        return data is null ? BlobData.NotFound() : BlobData.From(data);
    }

    public async ValueTask<BlobData> RemoveBlob(BlobKey key)
    {
        ThrowIfDisposed();
        if (!key.HasValue) ThrowMustNotBeEmpty(nameof(key));
#if NET8_0_OR_GREATER
        ReadOnlySpan<byte> keyBytes = key.Bytes.Span;
        byte[] data = _rocksBlobDb.Get(keyBytes);
#else
        byte[] keyBytes = key.Bytes.ToArray();
        byte[] data = _rocksBlobDb.Get(keyBytes);
#endif
        if (data is not null) _rocksBlobDb.Remove(keyBytes);
        return data is null ? BlobData.NotFound() : BlobData.From(data);
    }

    public async ValueTask PutBlob(BlobKey key, BlobData data)
    {
        ThrowIfDisposed();
        if (!key.HasValue) ThrowMustNotBeEmpty(nameof(key));
        if (!data.HasValue) ThrowMustNotBeEmpty(nameof(data));
#if NET8_0_OR_GREATER
        _rocksBlobDb.Put(key.Bytes.Span, data.Bytes.Span);
#else
        _rocksBlobDb.Put(key.Bytes.ToArray(), data.Bytes.ToArray());
#endif
    }
}
