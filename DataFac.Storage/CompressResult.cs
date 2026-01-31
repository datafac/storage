using System.Buffers;

namespace DataFac.Storage;

public readonly struct CompressResult
{
    public readonly BlobIdV1 BlobId;
    public readonly ReadOnlySequence<byte> CompressedData;
    public CompressResult(BlobIdV1 blobId, ReadOnlySequence<byte> compressedData) : this()
    {
        BlobId = blobId;
        CompressedData = compressedData;
    }
}
