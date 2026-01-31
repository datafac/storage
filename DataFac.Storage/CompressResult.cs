using System.Buffers;

namespace DataFac.Storage;

public readonly struct CompressResult
{
    public readonly BlobIdV1 BlobId;
    public readonly BlobCompAlgo CompAlgo;
    public readonly ReadOnlySequence<byte> CompressedData;
    public CompressResult(BlobIdV1 blobId, BlobCompAlgo compAlgo, ReadOnlySequence<byte> compressedData) : this()
    {
        BlobId = blobId;
        CompAlgo = compAlgo;
        CompressedData = compressedData;
    }
}
