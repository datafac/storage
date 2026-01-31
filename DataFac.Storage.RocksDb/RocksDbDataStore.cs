using RocksDbSharp;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DataFac.Storage.RocksDbStore;

public sealed class RocksDbDataStore : IDataStore
{
#pragma warning disable CA2213 // Disposable fields should be disposed
    private readonly RocksDb _rocksBlobDb;
    private readonly RocksDb _rocksNameDb;
#pragma warning restore CA2213 // Disposable fields should be disposed

    private readonly ConcurrentDictionary<BlobIdV1, ReadOnlySequence<byte>> _blobCache = new ConcurrentDictionary<BlobIdV1, ReadOnlySequence<byte>>();
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
        if (_disposed) return;
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

    public KeyValuePair<string, BlobIdV1>[] GetNames()
    {
        var list = new List<KeyValuePair<string, BlobIdV1>>();
        using var iter = _rocksNameDb.NewIterator();
        var iter2 = iter.SeekToFirst();
        while (iter2.Valid())
        {
#if NET8_0_OR_GREATER
            string key = Encoding.UTF8.GetString(iter2.GetKeySpan());
            BlobIdV1 value = BlobIdV1.UnsafeWrap(iter2.Value());
#else
            string key = Encoding.UTF8.GetString(iter2.Key());
            BlobIdV1 value = BlobIdV1.UnsafeWrap(iter2.Value());
#endif
            list.Add(new KeyValuePair<string, BlobIdV1>(key, value));
            iter2 = iter2.Next();
        }
        return list.ToArray();
    }

    public BlobIdV1? GetName(string key)
    {
        if (string.IsNullOrEmpty(key)) ThrowMustNotBeEmpty(nameof(key));
        var keyBytes = Encoding.UTF8.GetBytes(key);
#if NET8_0_OR_GREATER
        ReadOnlySpan<byte> keySpan = keyBytes.AsSpan();
        var currentBytes = _rocksNameDb.Get(keySpan);
#else
        var currentBytes = _rocksNameDb.Get(keyBytes);
#endif
        return currentBytes is null ? null : BlobIdV1.UnsafeWrap(currentBytes);
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

    public bool PutName(string key, in BlobIdV1 id)
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
            _rocksNameDb.Put(keySpan, id.Memory.Span);
        }
#else
        var keyBytes = Encoding.UTF8.GetBytes(key);
        bool added = _rocksNameDb.Get(keyBytes) is null;
        if (added)
        {
            byte[] valueBytes = id.Memory.ToArray();
            _rocksNameDb.Put(keyBytes, valueBytes);
        }
#endif
        if (added)
        {
            Interlocked.Increment(ref _counters.NameDelta);
        }
        return added;
    }

    public KeyValuePair<BlobIdV1, ReadOnlySequence<byte>>[] GetCachedBlobs() => _blobCache.ToArray();

    public KeyValuePair<BlobIdV1, ReadOnlySequence<byte>>[] GetStoredBlobs()
    {
        var list = new List<KeyValuePair<BlobIdV1, ReadOnlySequence<byte>>>();
        using var iter = _rocksBlobDb.NewIterator();
        var iter2 = iter.SeekToFirst();
        while (iter2.Valid())
        {
#if NET8_0_OR_GREATER
            BlobIdV1 key = BlobIdV1.UnsafeWrap(iter2.Key());
            ReadOnlySequence<byte> value = new ReadOnlySequence<byte>(iter2.Value());
#else
            BlobIdV1 key = BlobIdV1.UnsafeWrap(iter2.Key());
            ReadOnlySequence<byte> value = new ReadOnlySequence<byte>(iter2.Value());
#endif
            list.Add(new KeyValuePair<BlobIdV1, ReadOnlySequence<byte>>(key, value));
            iter2 = iter2.Next();
        }
        return list.ToArray();
    }

    public async ValueTask<ReadOnlySequence<byte>?> GetBlob(BlobIdV1 id)
    {
        ThrowIfDisposed();
        if (id.IsDefault) return null;
        if (id.TryGetEmbeddedBlob(out var embeddedBlob))
        {
            return embeddedBlob;
        }

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
        var complete = new TaskCompletionSource<ReadOnlySequence<byte>?>();
        _writer.TryWrite(AsyncOp.Get(id, complete));
        var data = await complete.Task.ConfigureAwait(false);
        if (data is not null)
        {
            _blobCache.TryAdd(id, data.Value);
        }
        return data;
    }

    public async ValueTask<ReadOnlySequence<byte>?> RemoveBlob(BlobIdV1 id, bool withSync)
    {
        ThrowIfDisposed();

        _blobCache.TryRemove(id, out var _);

        // enqueue remove
        if (withSync)
        {
            var complete = new TaskCompletionSource<ReadOnlySequence<byte>?>();
            _writer.TryWrite(AsyncOp.Del(id, complete));
            var data = await complete.Task.ConfigureAwait(false);
            return data;
        }
        else
        {
            _writer.TryWrite(AsyncOp.Del(id, null));
            return null;
        }
    }

    public ValueTask RemoveBlobs(IEnumerable<BlobIdV1> ids, bool withSync)
    {
        if (ids is null) throw new ArgumentNullException(nameof(ids));

        ThrowIfDisposed();

        foreach (var id in ids)
        {
            _blobCache.TryRemove(id, out var _);
            _writer.TryWrite(AsyncOp.Del(id, null));
        }

        if (withSync)
        {
            var complete = new TaskCompletionSource<ReadOnlySequence<byte>?>();
            _writer.TryWrite(AsyncOp.Sync(complete));
            return new ValueTask(complete.Task);
        }
        else
        {
            return default;
        }
    }

    public async ValueTask<BlobIdV1> PutBlob(ReadOnlySequence<byte> data, bool withSync)
    {
        ThrowIfDisposed();
        var compressResult = data.TryCompressBlob();
        var id = compressResult.BlobId;
        if (id.IsEmbedded) return id;

        Interlocked.Increment(ref _counters.BlobPutCount);
        if (!_blobCache.TryAdd(id, compressResult.CompressedData))
        {
            Interlocked.Increment(ref _counters.BlobPutSkips);
            return id;
        }

        // added to cache - enqueue put
        if (withSync)
        {
            var complete = new TaskCompletionSource<ReadOnlySequence<byte>?>();
            _writer.TryWrite(AsyncOp.Put(id, data, complete));
            await complete.Task.ConfigureAwait(false);
            return id;
        }
        else
        {
            _writer.TryWrite(AsyncOp.Put(id, data, null));
            return id;
        }
    }

    public ValueTask Sync()
    {
        ThrowIfDisposed();
        // enqueue sync
        var complete = new TaskCompletionSource<ReadOnlySequence<byte>?>();
        _writer.TryWrite(AsyncOp.Sync(complete));
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
                switch (item.Kind)
                {
                    case AsyncOpKind.Get:
                        {
                            // async get
                            var data = await InternalGetBlob(item.Id).ConfigureAwait(false);
                            item.Completion?.SetResult(data);
                            break;
                        }
                    case AsyncOpKind.Put:
                        {
                            // async put
                            await InternalPutBlob(item.Id, item.Data).ConfigureAwait(false);
                            item.Completion?.TrySetResult(null);
                            break;
                        }
                    case AsyncOpKind.Del:
                        {
                            // async del
                            var data = await InternalDelBlob(item.Id).ConfigureAwait(false);
                            item.Completion?.TrySetResult(data);
                            break;
                        }
                    default:
                        {
                            // assume sync
                            item.Completion?.SetResult(null);
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
        _rocksBlobDb.Dispose();
    }

    private ValueTask<ReadOnlySequence<byte>?> InternalGetBlob(in BlobIdV1 id)
    {
#if NET8_0_OR_GREATER
        byte[] currentBytes = _rocksBlobDb.Get(id.Memory.Span);
#else
        byte[] currentBytes = _rocksBlobDb.Get(id.Memory.ToArray());
#endif
        return currentBytes is null
            ? new ValueTask<ReadOnlySequence<byte>?>((ReadOnlySequence<byte>?)null)
            : new ValueTask<ReadOnlySequence<byte>?>(new ReadOnlySequence<byte>(currentBytes));
    }

    private ValueTask InternalPutBlob(in BlobIdV1 id, in ReadOnlySequence<byte> data)
    {
#if NET8_0_OR_GREATER
        var keySpan = id.Memory.Span;
        byte[] currentBytes = _rocksBlobDb.Get(keySpan);
#else
        var key = id.Memory.ToArray();
        byte[] currentBytes = _rocksBlobDb.Get(key);
#endif
        if (currentBytes is null)
        {
            // adding blob
#if NET8_0_OR_GREATER
            _rocksBlobDb.Put(keySpan, data.ToArray());
#else
            _rocksBlobDb.Put(key, data.ToArray());
#endif
            Interlocked.Increment(ref _counters.BlobPutWrits);
            Interlocked.Add(ref _counters.ByteDelta, data.Length);
        }
        else
        {
            Interlocked.Increment(ref _counters.BlobPutSkips);
        }
        return default;
    }

    private ValueTask<ReadOnlySequence<byte>?> InternalDelBlob(in BlobIdV1 id)
    {
#if NET8_0_OR_GREATER
        var keySpan = id.Memory.Span;
        byte[] currentBytes = _rocksBlobDb.Get(keySpan);
#else
        var key = id.Memory.ToArray();
        byte[] currentBytes = _rocksBlobDb.Get(key);
#endif
        if (currentBytes is null)
        {
            return new ValueTask<ReadOnlySequence<byte>?>((ReadOnlySequence<byte>?)null);
        }
        else
        {
#if NET8_0_OR_GREATER
            _rocksBlobDb.Remove(keySpan);
#else
            _rocksBlobDb.Remove(key);
#endif
            return new ValueTask<ReadOnlySequence<byte>?>(new ReadOnlySequence<byte>(currentBytes));
        }
    }
}
