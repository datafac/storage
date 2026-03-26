using System;
using System.Buffers;
using System.Security.Cryptography;

namespace DataFac.Storage;

public sealed class SHA256Hasher : IBlobHasher
{
    public static void ComputeHash(ReadOnlySpan<byte> data, Span<byte> hashOutput)
    {
        // incremental hasher for SHA-256
        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
#if NET8_0_OR_GREATER
        hasher.AppendData(data);
        if (!hasher.TryGetHashAndReset(hashOutput, out int bytesWritten))
        {
            throw new InvalidOperationException("Destination too small");
        }
#else
        hasher.AppendData(data.ToArray());
        byte[] hashBytes = hasher.GetHashAndReset();
        hashBytes.CopyTo(hashOutput);
#endif
    }
}
