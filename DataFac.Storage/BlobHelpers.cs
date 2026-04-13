using DataFac.Compression;
using System;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace DataFac.Storage;

public static class BlobHelpers
{
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

    //[Obsolete("Follow TestDataStore method instead.", true)]
    //public static CompressResult TryCompressBlob(this ReadOnlyMemory<byte> uncompressedData)
    //{
    //    // Snappier compression and hashing
    //    var compressResult = SnappyCompressor.CompressData(uncompressedData);

    //    // embed compressed if small engough
    //    if (compressResult.Output.Length <= BlobIdV1.MaxEmbeddedSize)
    //    {
    //        return new CompressResult(new BlobIdV1(compressResult.CompAlgo, compressResult.Output), ReadOnlyMemory<byte>.Empty);
    //    }

    //    BlobIdV1 blobId = new BlobIdV1(compressResult.InputSize, compressResult.CompAlgo, BlobHashAlgo.Sha256, compressResult.InputHash.Span);
    //    return new CompressResult(blobId, compressResult.Output);
    //}

    //[Obsolete("Follow TestDataStore method instead.", true)]
    //public static CompressResult TryCompressText(this string uncompressedText)
    //{
    //    // Snappier compression and hashing
    //    var compressResult = SnappyCompressor.CompressText(uncompressedText);

    //    // embed compressed if small engough
    //    if (compressResult.Output.Length <= BlobIdV1.MaxEmbeddedSize)
    //    {
    //        return new CompressResult(new BlobIdV1(compressResult.CompAlgo, compressResult.Output), ReadOnlyMemory<byte>.Empty);
    //    }

    //    BlobIdV1 blobId = new BlobIdV1(compressResult.InputSize, compressResult.CompAlgo, BlobHashAlgo.Sha256, compressResult.InputHash.Span);
    //    return new CompressResult(blobId, compressResult.Output);
    //}

}
