using System;
using System.Buffers;

namespace DataFac.Storage;

public interface IBlobHasher
{
#if NET7_0_OR_GREATER
    static abstract void ComputeHash(ReadOnlySequence<byte> data, Span<byte> hashOutput);
#endif
}
