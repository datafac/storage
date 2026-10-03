using DataFac.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Storage.LocalFS;

public sealed class LocalFSNameStore : INameStore
{
    private readonly string _namePath;

    public LocalFSNameStore(string rootpath)
    {
        _namePath = rootpath + "\\names";
        Directory.CreateDirectory(_namePath);
    }

    private volatile bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        //_rocksBlobDb.Dispose();
    }

    public async ValueTask<BlobKey> GetName(string name, CancellationToken cancellation)
    {
        string filepath = Path.Combine(_namePath, name);
        if (!File.Exists(filepath)) return BlobKey.NotFound();

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
        return BlobKey.From(builder.Build());
    }

    public IEnumerable<string> GetNames()
    {
        return new DirectoryInfo(_namePath).EnumerateFiles().Select(f => f.Name);
    }

    public async ValueTask PutName(string name, BlobKey key, CancellationToken cancellation)
    {
        string filepath = Path.Combine(_namePath, name);
        using var fileStream = new FileStream(filepath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
#if NET8_0_OR_GREATER
        await fileStream.WriteAsync(key.Bytes, cancellation);
#else
        await fileStream.WriteAsync(key.Bytes.ToArray(), 0, key.Bytes.Length, cancellation); // todo alloc!
#endif
    }

    public void RemoveName(string name)
    {
        string filepath = Path.Combine(_namePath, name);
        File.Delete(filepath);
    }
}
