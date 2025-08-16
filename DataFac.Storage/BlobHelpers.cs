using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace DataFac.Storage;

public static class BlobHelpers
{
    public static BlobIdV1 GetBlobId(this ReadOnlySpan<byte> blob)
    {
        // embed small blobs directly into id
        if (blob.Length <= (BlobIdV1.Size - 2))
        {
            return new BlobIdV1(BlobCompAlgo.None, blob);
        }

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
        return new BlobIdV1(blob.Length, BlobCompAlgo.None, 0, BlobHashAlgo.Sha256, hashSpan);
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void CheckIdMatchesData(in BlobIdV1 id, in ReadOnlyMemory<byte> data)
    {
        BlobIdV1 checkId = GetBlobId(data.Span);
        if (!id.Equals(checkId)) throw new InvalidDataException($"Id does not match Data");
    }
}
