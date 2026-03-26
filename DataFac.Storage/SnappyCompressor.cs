using DataFac.Memory;
using Snappier;
using System;
using System.Buffers;

namespace DataFac.Storage;

public sealed class SnappyCompressor : IBlobCompressor
{
    public static ReadOnlyMemory<byte> Compress(ReadOnlyMemory<byte> uncompressedData)
    {
        ReadOnlySequence<byte> inputSequence = new ReadOnlySequence<byte>(uncompressedData);
        var buffers = new ByteBufferWriter();
        Snappy.Compress(inputSequence, buffers);
        var compressedData = buffers.GetWrittenSequence();
        return compressedData.Compact();
    }
    public static ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> compressedData)
    {
        ReadOnlySequence<byte> inputSequence = new ReadOnlySequence<byte>(compressedData);
        var buffers = new ByteBufferWriter();
        Snappy.Decompress(inputSequence, buffers);
        var decompressedData = buffers.GetWrittenSequence();
        return decompressedData.Compact();
    }
}
