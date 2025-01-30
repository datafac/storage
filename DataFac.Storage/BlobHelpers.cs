using DataFac.Memory;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace DataFac.Storage;

public static class BlobHelpers
{
    public static BlobIdV0old GetBlobIdold(this BlobData data)
    {
#if NET8_0_OR_GREATER
        byte[] hash = SHA256.HashData(data.Memory.Span);
#else
        SHA256 sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(data.Memory.ToArray());
#endif

        return BlobIdV0old.UnsafeWrap(hash);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void CheckIdMatchesData(in BlobIdV0old id, in BlobData data)
    {
        BlobIdV0old checkId = data.GetBlobIdold();
        if (!id.Equals(checkId)) throw new InvalidDataException($"Id does not match Data");
    }

    public static BlobIdV1 GetBlobId(this ReadOnlySpan<byte> blob)
    {
        using var sha256 = SHA256.Create();
#if NET8_0_OR_GREATER
        Span<byte> hashSpan = stackalloc byte[32];
        if (!sha256.TryComputeHash(blob, hashSpan, out int bytesWritten) || bytesWritten != 32)
        {
            throw new InvalidOperationException("Destination too small");
        }
#else
        byte[] hashBytes = sha256.ComputeHash(blob.ToArray());
        Span<byte> hashSpan = hashBytes.AsSpan();
#endif
        // todo compression
        BlockB032 hashData = default;
        hashData.TryRead(hashSpan);
        return new BlobIdV1(blob.Length, BlobCompAlgo.None, 0, BlobHashAlgo.Sha256, hashData);
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void CheckIdMatchesData(in BlobIdV1 id, in BlobData data)
    {
        BlobIdV1 checkId = GetBlobId(data.Memory.Span);
        if (!id.Equals(checkId)) throw new InvalidDataException($"Id does not match Data");
    }

    public static BlobIdV1 GetId(this BlobData data)
    {
        return data.Memory.Span.GetBlobId();
    }

}
