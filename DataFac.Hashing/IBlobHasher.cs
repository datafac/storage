using System;

namespace DataFac.Hashing;

public interface IBlobHasher
{
#if NET7_0_OR_GREATER
    static abstract void ComputeHash(ReadOnlySpan<byte> data, Span<byte> hashOutput);
#endif
}

public enum BlobHashAlgo : byte
{
    None = 0,
    Sha256 = 1,
}
