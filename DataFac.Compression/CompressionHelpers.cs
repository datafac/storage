namespace DataFac.Compression;

public static class CompressionHelpers
{
    public static byte ToCharCode(this BlobCompAlgo algo)
    {
        return algo switch
        {
            BlobCompAlgo.Brotli => (byte)'B',
            BlobCompAlgo.Snappy => (byte)'S',
            _ => (byte)'U'
        };
    }

    public static BlobCompAlgo ToCompAlgo(this byte charCode)
    {
        return charCode switch
        {
            (byte)'B' => BlobCompAlgo.Brotli,
            (byte)'S' => BlobCompAlgo.Snappy,
            _ => BlobCompAlgo.UnComp,
        };
    }

}
