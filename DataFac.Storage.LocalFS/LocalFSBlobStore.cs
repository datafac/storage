using DataFac.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Storage.LocalFS;

public sealed class LocalFSBlobStore : IBlobStore
{
    private readonly string _blobPath;

    public LocalFSBlobStore(string rootpath)
    {
        _blobPath = rootpath + "\\blobs";
        Directory.CreateDirectory(_blobPath);
    }

    private volatile bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        //_rocksBlobDb.Dispose();
    }

    public async ValueTask<BlobData> GetBlob(BlobKey key, CancellationToken cancellation)
    {
        string filename = key.ToString();
        string filepath = Path.Combine(_blobPath, filename);
        if(!File.Exists(filepath)) return BlobData.NotFound();

        using var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var builder = new ReadOnlySequenceBuilder<byte>();
        int bytesRead = 0;
        do
        {
#if NET8_0_OR_GREATER
            Memory<byte> buffer = new byte[1024];
            bytesRead = await fileStream.ReadAsync(buffer, cancellation);
            if (bytesRead > 0) builder = builder.Append(buffer.Slice(0, bytesRead));
#else
            byte[] buffer = new byte[1024];
            bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellation);
            if (bytesRead > 0) builder = builder.Append(new ReadOnlyMemory<byte>(buffer, 0, bytesRead));
#endif
        } while (bytesRead > 0);
        return BlobData.From(builder.Build());
    }

    public IAsyncEnumerable<KeyValuePair<BlobKey, BlobData>> GetBlobs(CancellationToken cancellation)
    {
        throw new NotImplementedException();
    }

    public async ValueTask PutBlob(BlobKey key, BlobData data, CancellationToken cancellation)
    {
        string filename = key.ToString();
        string filepath = Path.Combine(_blobPath, filename);
        using var fileStream = new FileStream(filepath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
#if NET8_0_OR_GREATER
        await fileStream.WriteAsync(data.Bytes, cancellation);
#else
        await fileStream.WriteAsync(data.Bytes.ToArray(), 0, data.Bytes.Length, cancellation); // todo alloc!
#endif
    }

    public ValueTask<BlobData> RemoveBlob(BlobKey key)
    {
        throw new NotImplementedException();
    }
}
