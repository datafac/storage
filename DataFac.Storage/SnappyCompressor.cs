using DataFac.Memory;
using Snappier;
using System.Buffers;

namespace DataFac.Storage;

public sealed class SnappyCompressor : IBlobCompressor
{
    public static ReadOnlySequence<byte> Compress(ReadOnlySequence<byte> uncompressedData)
    {
        var buffers = new ByteBufferWriter();
        Snappy.Compress(uncompressedData, buffers);
        var compressedData = buffers.GetWrittenSequence();
        return compressedData;
    }
    public static ReadOnlySequence<byte> Decompress(ReadOnlySequence<byte> compressedData)
    {
        var buffers = new ByteBufferWriter();
        Snappy.Decompress(compressedData, buffers);
        var decompressedData = buffers.GetWrittenSequence();
        return decompressedData;
    }
}
