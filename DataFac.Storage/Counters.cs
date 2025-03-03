#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1815 // Override equals and operator equals on value types

namespace DataFac.Storage;

public struct Counters
{
    public long NameDelta;

    public long BlobGetCount;
    public long BlobGetCache;
    public long BlobGetReads;

    public long BlobPutCount;
    public long BlobPutWrits;
    public long BlobPutSkips;

    public long ByteDelta;
}
