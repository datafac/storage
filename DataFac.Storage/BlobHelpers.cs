using System;
using System.Buffers;
using System.Security.Cryptography;

namespace DataFac.Storage;

public static class BlobHelpers
{
    public static byte ToCharCode(this BlobCompAlgo algo)
    {
        return algo switch
        {
            BlobCompAlgo.Brotli => (byte)'B',
            BlobCompAlgo.Snappy => (byte)'S',
            _ => (byte)'U'
        };
    }

    public static BlobCompAlgo ToCompAlgo(this byte charCode)
    {
        return charCode switch
        {
            (byte)'B' => BlobCompAlgo.Brotli,
            (byte)'S' => BlobCompAlgo.Snappy,
            _ => BlobCompAlgo.UnComp,
        };
    }

    public static ReadOnlySequence<byte> TryDecompressBlob(BlobIdV1 id, ReadOnlySequence<byte> blobData)
    {
        switch (id.CompAlgo)
        {
            case BlobCompAlgo.UnComp:
                return blobData;
            case BlobCompAlgo.Brotli:
                throw new NotImplementedException("Brotli embedded blobs are not implemented yet.");
            case BlobCompAlgo.Snappy:
                var decompressedData = SnappyCompressor.Decompress(blobData);
                return decompressedData;
            default:
                throw new NotSupportedException($"Compression algorithm {id.CompAlgo} not supported.");
        }
    }

    public static CompressResult TryCompressBlob(this ReadOnlySequence<byte> uncompressedData)
    {
        const long maxBlobSize = 64 * 1024; // 64K todo const
        if (uncompressedData.Length > maxBlobSize) throw new ArgumentOutOfRangeException(nameof(uncompressedData), uncompressedData.Length, "Must not exceed 64K");

        int blobSize = (int)uncompressedData.Length;

        // embed small blobs directly into id
        if (uncompressedData.Length <= (BlobIdV1.Size - 2))
        {
            return new CompressResult(new BlobIdV1(BlobCompAlgo.UnComp, uncompressedData), ReadOnlySequence<byte>.Empty);
        }

        // Snappier compression
        var compressedData = SnappyCompressor.Compress(uncompressedData);

        // embed compressed if small engough
        if (compressedData.Length <= (BlobIdV1.Size - 2))
        {
            return new CompressResult(new BlobIdV1(BlobCompAlgo.Snappy, compressedData), ReadOnlySequence<byte>.Empty);
        }

        ReadOnlySequence<byte> dataToReturnqqq;
        BlobCompAlgo compAlgo;
        if (compressedData.Length < uncompressedData.Length)
        {
            // compressed is smaller - use compressed data
            dataToReturnqqq = compressedData;
            compAlgo = BlobCompAlgo.Snappy;
        }
        else
        {
            // compressed is larger - use uncompressed data
            dataToReturnqqq = uncompressedData;
            compAlgo = BlobCompAlgo.UnComp;
        }

        Span<byte> hashSpan = stackalloc byte[32];
        SHA256Hasher.ComputeHash(uncompressedData, hashSpan);
        BlobIdV1 blobId = new BlobIdV1(blobSize, compAlgo, BlobHashAlgo.Sha256, hashSpan);
        return new CompressResult(blobId, dataToReturnqqq);
    }
}
