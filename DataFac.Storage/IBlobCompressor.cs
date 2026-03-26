using System;
using System.Buffers;

namespace DataFac.Storage;

public interface IBlobCompressor
{
#if NET7_0_OR_GREATER
    static abstract ReadOnlyMemory<byte> Compress(ReadOnlyMemory<byte> data);
    static abstract ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> data);
#endif
}
