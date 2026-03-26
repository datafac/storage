using System;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;

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

    /// <summary>
    /// todo convert to rom buffers
    /// </summary>
    /// <param name="id"></param>
    /// <param name="blobData"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public static ReadOnlyMemory<byte> TryDecompressBlob(BlobIdV1 id, ReadOnlyMemory<byte> blobData)
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

    public static CompressResult TryCompressBlob(this ReadOnlyMemory<byte> uncompressedData)
    {
        const long maxBlobSize = 64 * 1024; // 64K todo const
        if (uncompressedData.Length > maxBlobSize) throw new ArgumentOutOfRangeException(nameof(uncompressedData), uncompressedData.Length, "Must not exceed 64K");

        int blobSize = (int)uncompressedData.Length;

        // embed small blobs directly into id
        if (uncompressedData.Length <= (BlobIdV1.Size - 2))
        {
            return new CompressResult(new BlobIdV1(BlobCompAlgo.UnComp, uncompressedData), ReadOnlyMemory<byte>.Empty);
        }

        // Snappier compression
        var compressedData = SnappyCompressor.Compress(uncompressedData);

        // embed compressed if small engough
        if (compressedData.Length <= (BlobIdV1.Size - 2))
        {
            return new CompressResult(new BlobIdV1(BlobCompAlgo.Snappy, compressedData), ReadOnlyMemory<byte>.Empty);
        }

        ReadOnlyMemory<byte> dataToReturn;
        BlobCompAlgo compAlgo;
        if (compressedData.Length < uncompressedData.Length)
        {
            // compressed is smaller - use compressed data
            dataToReturn = compressedData;
            compAlgo = BlobCompAlgo.Snappy;
        }
        else
        {
            // compressed is larger - use uncompressed data
            dataToReturn = uncompressedData;
            compAlgo = BlobCompAlgo.UnComp;
        }

        Span<byte> hashSpan = stackalloc byte[32];
        SHA256Hasher.ComputeHash(uncompressedData.Span, hashSpan);
        BlobIdV1 blobId = new BlobIdV1(blobSize, compAlgo, BlobHashAlgo.Sha256, hashSpan);
        return new CompressResult(blobId, dataToReturn);
    }

    public static CompressResult TryCompressText(this string uncompressedText)
    {
        // embed small blobs directly into id
#if NET8_0_OR_GREATER
        Span<byte> smallBuffer = stackalloc byte[BlobIdV1.Size - 2];
        if (Encoding.UTF8.TryGetBytes(uncompressedText, smallBuffer, out int bytesWritten))
        {
            return new CompressResult(new BlobIdV1(BlobCompAlgo.UnComp, smallBuffer.Slice(0, bytesWritten)), ReadOnlyMemory<byte>.Empty);
        }
        return TryCompressBlob(Encoding.UTF8.GetBytes(uncompressedText));
#else
        byte[] encodedText = Encoding.UTF8.GetBytes(uncompressedText);
        if (encodedText.Length <= (BlobIdV1.Size - 2))
        {
            return new CompressResult(new BlobIdV1(BlobCompAlgo.UnComp, encodedText), ReadOnlyMemory<byte>.Empty);
        }
        return TryCompressBlob(new ReadOnlyMemory<byte>(encodedText));
#endif
    }

}
