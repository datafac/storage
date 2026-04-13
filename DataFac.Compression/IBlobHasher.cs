using System;

namespace DataFac.Compression;

public interface IBlobHasher
{
#if NET7_0_OR_GREATER
    static abstract void ComputeHash(ReadOnlySpan<byte> data, Span<byte> hashOutput);
#endif
}
