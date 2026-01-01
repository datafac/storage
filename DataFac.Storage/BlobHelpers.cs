using DataFac.Memory;
using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace DataFac.Storage;

public static class BlobHelpers
{
    public static BlobIdV1 GetBlobId(this ReadOnlySequence<byte> blob)
    {
        // embed small blobs directly into id
        if (blob.Length <= (BlobIdV1.Size - 2))
        {
            return new BlobIdV1(BlobCompAlgo.None, blob);
        }

        using var sha256 = SHA256.Create();
#if NET8_0_OR_GREATER
        Span<byte> hashSpan = stackalloc byte[32];
        // todo how to avoid allocation here?
        ReadOnlySpan<byte> blobSpan = blob.Compact().Span;
        if (!SHA256.TryHashData(blobSpan, hashSpan, out int bytesWritten) || bytesWritten != 32)
        {
            throw new InvalidOperationException("Destination too small");
        }
        // todo compression
        return new BlobIdV1(blobSpan.Length, BlobCompAlgo.None, 0, BlobHashAlgo.Sha256, hashSpan);
#else
        byte[] blobBytes = blob.ToArray();
        byte[] hashBytes = sha256.ComputeHash(blobBytes);
        Span<byte> hashSpan = hashBytes.AsSpan();
        // todo compression
        return new BlobIdV1(blobBytes.Length, BlobCompAlgo.None, 0, BlobHashAlgo.Sha256, hashSpan);
#endif
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void CheckIdMatchesData(in BlobIdV1 id, in ReadOnlySequence<byte> data)
    {
        BlobIdV1 checkId = GetBlobId(data);
        if (!id.Equals(checkId)) throw new InvalidDataException($"Id does not match Data");
    }
}
