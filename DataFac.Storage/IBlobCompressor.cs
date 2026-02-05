using System.Buffers;

namespace DataFac.Storage;

public interface IBlobCompressor
{
#if NET7_0_OR_GREATER
    static abstract ReadOnlySequence<byte> Compress(ReadOnlySequence<byte> data);
    static abstract ReadOnlySequence<byte> Decompress(ReadOnlySequence<byte> data);
#endif
}
