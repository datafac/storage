namespace DataFac.Storage
{
    public enum BlobCompAlgo : byte
    {
        UnComp = 0, // 'U'
        Brotli = 1, // 'B'
        Snappy = 2, // 'S'
    }
}
