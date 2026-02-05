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

        ReadOnlySequence<byte> dataToReturn;
        int compSize;
        BlobCompAlgo compAlgo;
        if (compressedData.Length < uncompressedData.Length)
        {
            // compressed is smaller - use compressed data
            dataToReturn = compressedData;
            compAlgo = BlobCompAlgo.Snappy;
            compSize = (int)compressedData.Length;
        }
        else
        {
            // compressed is larger - use uncompressed data
            dataToReturn = uncompressedData;
            compAlgo = BlobCompAlgo.UnComp;
            compSize = 0;
        }

        // note: we always hash the original/uncompressed data
        Span<byte> hashSpan = stackalloc byte[32];
#if NET8_0_OR_GREATER
        {
            // incremental hasher for SHA-256
            using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            foreach (var segment in uncompressedData)
            {
                if (!segment.IsEmpty)
                {
                    hasher.AppendData(segment.Span);
                }
            }
            if (!hasher.TryGetHashAndReset(hashSpan, out int bytesWritten))
            {
                throw new InvalidOperationException("Destination too small");
            }
        }
#else
        {
            using var sha256 = SHA256.Create();
            byte[] blobBytes = uncompressedData.ToArray();
            byte[] hashBytes = sha256.ComputeHash(blobBytes);
            hashBytes.CopyTo(hashSpan);
        }
#endif
        BlobIdV1 blobId = new BlobIdV1(blobSize, compAlgo, compSize, BlobHashAlgo.Sha256, hashSpan);
        return new CompressResult(blobId, dataToReturn);
    }
}
