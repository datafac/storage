using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace DataFac.Hashing;

public sealed class SHA256Hasher : IBlobHasher
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowBufferTooSmall()
    {
        throw new InvalidOperationException("Hash output buffer too small");
    }

#if NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ComputeHash(ReadOnlySpan<byte> data, Span<byte> hashOutput)
    {
        if (!SHA256.TryHashData(data, hashOutput, out int bytesWritten))
        {
            ThrowBufferTooSmall();
        }
    }
#else
    public static void ComputeHash(ReadOnlySpan<byte> data, Span<byte> hashOutput)
    {
        // incremental hasher for SHA-256
        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hasher.AppendData(data.ToArray());
        byte[] hashBytes = hasher.GetHashAndReset();
        hashBytes.CopyTo(hashOutput);
    }
#endif
}
