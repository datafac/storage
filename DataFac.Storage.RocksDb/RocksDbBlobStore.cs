using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Storage.RocksDbStore;

public sealed class RocksDbBlobStore : IBlobStore
{
    private readonly RocksDb _rocksBlobDb;

    public RocksDbBlobStore(string rootpath)
    {
        DbOptions dbOptions = new DbOptions().SetCreateIfMissing(true);

        string blobPath = $"{rootpath}\\blobs";
        Directory.CreateDirectory(blobPath);
        _rocksBlobDb = RocksDb.Open(dbOptions, blobPath);

    }

    private volatile bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _rocksBlobDb.Dispose();
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

    public async IAsyncEnumerable<KeyValuePair<BlobKey, BlobData>> GetBlobs([EnumeratorCancellation] CancellationToken cancellation)
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

    public async ValueTask<BlobData> GetBlob(BlobKey key, CancellationToken cancellation)
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

    public async ValueTask PutBlob(BlobKey key, BlobData data, CancellationToken cancellation)
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
