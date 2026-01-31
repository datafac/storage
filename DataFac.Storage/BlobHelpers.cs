using DataFac.Memory;
using Snappier;
using System;
using System.Buffers;
using System.Linq;
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

    public static CompressResult TryCompressBlob(this ReadOnlySequence<byte> uncompressedData)
    {
        const long maxBlobSize = 64 * 1024; // 64K todo const
        if (uncompressedData.Length > maxBlobSize) throw new ArgumentOutOfRangeException(nameof(uncompressedData), uncompressedData.Length, "Must not exceed 64K");

        int blobSize = (int)uncompressedData.Length;

        // embed small blobs directly into id
        if (uncompressedData.Length <= (BlobIdV1.Size - 2))
        {
            BlobIdV1 blobId = new BlobIdV1(BlobCompAlgo.UnComp, uncompressedData);
            return new CompressResult(blobId, BlobCompAlgo.UnComp, ReadOnlySequence<byte>.Empty);
        }

        // try Snappier compression
        var compressedBuffer = new CompressionBuffers();
        Snappy.Compress(uncompressedData, compressedBuffer);
        var compressedData = compressedBuffer.GetWrittenSequence();

        // embed compressed if small engough
        if (compressedData.Length <= (BlobIdV1.Size - 2))
        {
            BlobIdV1 blobId = new BlobIdV1(BlobCompAlgo.Snappy, compressedData);
            return new CompressResult(blobId, BlobCompAlgo.Snappy, ReadOnlySequence<byte>.Empty);
        }

        ReadOnlySequence<byte> dataToHash;
        int compSize;
        BlobCompAlgo compAlgo;
        if (compressedData.Length < uncompressedData.Length)
        {
            // use compressed data
            dataToHash = compressedData;
            compAlgo = BlobCompAlgo.Snappy;
            compSize = (int)compressedData.Length;
        }
        else
        {
            // use uncompressed data
            dataToHash = uncompressedData;
            compAlgo = BlobCompAlgo.UnComp;
            compSize = 0;
        }


#if NET8_0_OR_GREATER
        {
            Span<byte> hashSpan = stackalloc byte[32];
            // todo how to avoid allocation here?
            ReadOnlySpan<byte> blobSpan = dataToHash.Compact().Span;
            if (!SHA256.TryHashData(blobSpan, hashSpan, out int bytesWritten) || bytesWritten != 32)
            {
                throw new InvalidOperationException("Destination too small");
            }
            BlobIdV1 blobId = new BlobIdV1(blobSize, compAlgo, compSize, BlobHashAlgo.Sha256, hashSpan);
            return new CompressResult(blobId, compAlgo, dataToHash);
        }
#else
        {
            using var sha256 = SHA256.Create();
            byte[] blobBytes = dataToHash.ToArray();
            byte[] hashBytes = sha256.ComputeHash(blobBytes);
            Span<byte> hashSpan = hashBytes.AsSpan();
            BlobIdV1 blobId = new BlobIdV1(blobSize, compAlgo, compSize, BlobHashAlgo.Sha256, hashSpan);
            return new CompressResult(blobId, compAlgo, dataToHash);
        }
#endif
    }

    public static BlobIdV1 GetBlobIdOldqqq(this ReadOnlySequence<byte> blob)
    {
        // embed small blobs directly into id
        if (blob.Length <= (BlobIdV1.Size - 2))
        {
            return new BlobIdV1(BlobCompAlgo.UnComp, blob);
        }

        using var sha256 = SHA256.Create();
#if NET8_0_OR_GREATER
        Span<byte> hashSpan = stackalloc byte[32];
        // todo how to avoid allocation here?
        ReadOnlySpan<byte> blobSpan = blob.Compact().Span;
        if (!SHA256.TryHashData(blobSpan, hashSpan, out int bytesWritten) || bytesWritten != 32)
        {
            throw new InvalidOperationException("Destination too small");
        }
        // todo compression
        return new BlobIdV1(blobSpan.Length, BlobCompAlgo.UnComp, 0, BlobHashAlgo.Sha256, hashSpan);
#else
        byte[] blobBytes = blob.ToArray();
        byte[] hashBytes = sha256.ComputeHash(blobBytes);
        Span<byte> hashSpan = hashBytes.AsSpan();
        // todo compression
        return new BlobIdV1(blobBytes.Length, BlobCompAlgo.UnComp, 0, BlobHashAlgo.Sha256, hashSpan);
#endif
    }
}
