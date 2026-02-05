using System;
using System.Buffers;
using System.Security.Cryptography;

namespace DataFac.Storage;

public sealed class SHA256Hasher : IBlobHasher
{
    public static void ComputeHash(ReadOnlySequence<byte> data, Span<byte> hashOutput)
    {
        // incremental hasher for SHA-256
        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
#if NET8_0_OR_GREATER
        foreach (var segment in data)
        {
            if (!segment.IsEmpty)
            {
                hasher.AppendData(segment.Span);
            }
        }
        if (!hasher.TryGetHashAndReset(hashOutput, out int bytesWritten))
        {
            throw new InvalidOperationException("Destination too small");
        }
#else
        foreach (var segment in data)
        {
            if (!segment.IsEmpty)
            {
                hasher.AppendData(segment.ToArray());
            }
        }
        byte[] hashBytes = hasher.GetHashAndReset();
        hashBytes.CopyTo(hashOutput);
#endif
    }
}
