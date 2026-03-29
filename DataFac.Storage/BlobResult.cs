using System;

namespace DataFac.Storage;

public readonly struct BlobResult
{
    public readonly bool Complete;
    public readonly bool HasData;
    public readonly ReadOnlyMemory<byte> Data;

    private BlobResult(bool complete, bool hasData, ReadOnlyMemory<byte> data)
    {
        Complete = complete;
        HasData = hasData;
        Data = data;
    }

    private readonly static BlobResult _pending = new BlobResult(false, false, ReadOnlyMemory<byte>.Empty);
    private readonly static BlobResult _notFound = new BlobResult(true, false, ReadOnlyMemory<byte>.Empty);

    public static BlobResult Pending() => _pending;
    public static BlobResult NotFound() => _notFound;
    public static BlobResult WithData(ReadOnlyMemory<byte> data) => new BlobResult(true, true, data);
}
