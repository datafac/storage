using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Inventory.Store;

public static class BlobHelpers
{
    public static BlobId GetBlobId(this BlobData data)
    {
#if NET8_0_OR_GREATER
        byte[] hash = SHA256.HashData(data.Memory.Span);
#else
        SHA256 sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(data.Memory.ToArray());
#endif

        return BlobId.UnsafeWrap(hash);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void CheckIdMatchesData(in BlobId id, in BlobData data)
    {
        BlobId checkId = data.GetBlobId();
        if (!id.Equals(checkId)) throw new InvalidDataException($"Id does not match Data");
    }

}
