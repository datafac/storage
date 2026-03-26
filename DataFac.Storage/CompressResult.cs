using System;
using System.Buffers;

namespace DataFac.Storage;

public readonly struct CompressResult
{
    public readonly BlobIdV1 BlobId;
    public readonly ReadOnlyMemory<byte> CompressedData;
    public CompressResult(BlobIdV1 blobId, ReadOnlyMemory<byte> compressedData) : this()
    {
        BlobId = blobId;
        CompressedData = compressedData;
    }
}
