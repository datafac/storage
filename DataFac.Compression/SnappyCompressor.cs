using DataFac.Hashing;
using DataFac.Memory;
using DataFac.UnsafeHelpers;
using Snappier;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DataFac.Compression;

public sealed class SnappyCompressor : IBlobCompressor
{
    public static ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> compressedData)
    {
        ReadOnlySequence<byte> inputSequence = new ReadOnlySequence<byte>(compressedData);
        var buffers = new ByteBufferWriter();
        Snappy.Decompress(inputSequence, buffers);
        var decompressedData = buffers.GetWrittenSequence();
        return decompressedData.Compact();
    }

    public static CompressResult2 CompressData(ReadOnlyMemory<byte> data, Span<byte> hashSpan, int maxEmbeddedSize)
    {
        if (hashSpan.Length != 32) throw new ArgumentException("Length must be 32 bytes for SHA256", nameof(hashSpan));
        // compress using stack allocation only for small buffers, otherwise use heap allocation
        // if compressed is smaller, return compressed bytes; otherwise return original bytes
        if (data.Length <= maxEmbeddedSize)
        {
            return new CompressResult2(data.Length, BlobHashAlgo.None, BlobCompAlgo.UnComp, data);
        }
        SHA256Hasher.ComputeHash(data.Span, hashSpan);
        if (data.Length < 1024)
        {
            Span<byte> outputSpan = stackalloc byte[data.Span.Length + 128];
            if (Snappy.TryCompress(data.Span, outputSpan, out int bytesWritten))
            {
                return bytesWritten < data.Length
                    ? new CompressResult2(data.Length, BlobHashAlgo.Sha256, BlobCompAlgo.Snappy, outputSpan.Slice(0, bytesWritten).ToArray())
                    : new CompressResult2(data.Length, BlobHashAlgo.Sha256, BlobCompAlgo.UnComp, data);
            }
        }

        ReadOnlySequence<byte> inputSequence = new ReadOnlySequence<byte>(data);
        var buffers = new ByteBufferWriter();
        Snappy.Compress(inputSequence, buffers);
        var compressedData = buffers.GetWrittenSequence();

        return compressedData.Length < data.Length
            ? new CompressResult2(data.Length, BlobHashAlgo.Sha256, BlobCompAlgo.Snappy, compressedData.Compact())
            : new CompressResult2(data.Length, BlobHashAlgo.Sha256, BlobCompAlgo.UnComp, data);
    }

    public static CompressResult2 CompressText(string text, Span<byte> hashSpan, int maxEmbeddedSize)
    {

        if (hashSpan.Length != 32) throw new ArgumentException("Length must be 32 bytes for SHA256", nameof(hashSpan));
        // compress using stack allocation only for small strings, otherwise use heap allocation
        //int estimatedSize = Encoding.UTF8.GetByteCount(text);
        if (text.Length < 1024)
        {
#if NET8_0_OR_GREATER
            Span<byte> localBuffer = stackalloc byte[1024];
            if (Encoding.UTF8.TryGetBytes(text, localBuffer, out int bytesEncoded))
            {
                var inputSpan = localBuffer.Slice(0, bytesEncoded);
                if (bytesEncoded <= maxEmbeddedSize)
                {
                    return new CompressResult2(bytesEncoded, BlobHashAlgo.None, BlobCompAlgo.UnComp, inputSpan.ToArray());
                }
                SHA256Hasher.ComputeHash(inputSpan, hashSpan);
                Span<byte> outputSpan = stackalloc byte[bytesEncoded];
                if (Snappy.TryCompress(inputSpan, outputSpan, out int bytesWritten))
                {
                    return bytesWritten < bytesEncoded
                        ? new CompressResult2(bytesEncoded, BlobHashAlgo.Sha256, BlobCompAlgo.Snappy, outputSpan.Slice(0, bytesWritten).ToArray())
                        : new CompressResult2(bytesEncoded, BlobHashAlgo.Sha256, BlobCompAlgo.UnComp, inputSpan.ToArray());
                }
            }
#else
            ReadOnlyMemory<byte> inputMemory1 = System.Text.Encoding.UTF8.GetBytes(text);
            if (inputMemory1.Length <= maxEmbeddedSize)
            {
                return new CompressResult2(inputMemory1.Length, BlobHashAlgo.None, BlobCompAlgo.UnComp, inputMemory1);
            }
            SHA256Hasher.ComputeHash(inputMemory1.Span, hashSpan);
            Span<byte> outputSpan = stackalloc byte[inputMemory1.Length * 2];
            if (Snappy.TryCompress(inputMemory1.Span, outputSpan, out int bytesWritten))
            {
                return bytesWritten < inputMemory1.Length
                    ? new CompressResult2(inputMemory1.Length, BlobHashAlgo.Sha256, BlobCompAlgo.Snappy, outputSpan.Slice(0, bytesWritten).ToArray())
                    : new CompressResult2(inputMemory1.Length, BlobHashAlgo.Sha256, BlobCompAlgo.UnComp, inputMemory1);
            }
#endif
        }

        // default to heap allocation for larger strings or if stack allocation fails
        ReadOnlyMemory<byte> inputMemory2 = Encoding.UTF8.GetBytes(text);
        if (inputMemory2.Length <= maxEmbeddedSize)
        {
            return new CompressResult2(inputMemory2.Length, BlobHashAlgo.None, BlobCompAlgo.UnComp, inputMemory2);
        }
        SHA256Hasher.ComputeHash(inputMemory2.Span, hashSpan);
        var inputSequence = new ReadOnlySequence<byte>(inputMemory2);
        var buffers = new ByteBufferWriter();
        Snappy.Compress(inputSequence, buffers);
        var compressedData = buffers.GetWrittenSequence().Compact();
        return compressedData.Length < inputMemory2.Length
            ? new CompressResult2(inputMemory2.Length, BlobHashAlgo.Sha256, BlobCompAlgo.Snappy, compressedData)
            : new CompressResult2(inputMemory2.Length, BlobHashAlgo.Sha256, BlobCompAlgo.UnComp, inputMemory2);
    }
}