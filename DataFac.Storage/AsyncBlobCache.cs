using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DataFac.Storage;

public sealed class AsyncBlobCache : IBlobCache
{
    private readonly IBlobStore _blobStore;
    private readonly bool _chainDispose;

    private readonly ConcurrentDictionary<BlobKey, BlobData> _blobCache = new ConcurrentDictionary<BlobKey, BlobData>();
    private readonly ChannelWriter<AsyncOp> _writer;
    private readonly ChannelReader<AsyncOp> _reader;

    public AsyncBlobCache(IBlobStore blobStore, bool chainDispose = false)
    {
        _blobStore = blobStore;
        _chainDispose = chainDispose;

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
        if (_chainDispose)
        {
            _blobStore.Dispose();
        }
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
                            var result = await _blobStore.GetBlob(item.Key, CancellationToken.None).ConfigureAwait(false);
                            item.Completion?.TrySetResult(result);
                            break;
                        }
                    case AsyncOpKind.Put:
                        {
                            // async put
                            await _blobStore.PutBlob(item.Key, item.Data, CancellationToken.None).ConfigureAwait(false);
                            item.Completion?.TrySetResult(item.Data);
                            break;
                        }
                    case AsyncOpKind.Del:
                        {
                            // async del
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
        if (_chainDispose)
        {
            _blobStore.Dispose();
        }
    }
}
