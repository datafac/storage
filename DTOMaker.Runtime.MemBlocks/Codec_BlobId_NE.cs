using DataFac.Storage;
using System;

namespace DTOMaker.Runtime.MemBlocks
{
    public static class Codec_BlobId_NE
    {
        public static BlobIdV1 ReadFromSpan(ReadOnlySpan<byte> source) => new BlobIdV1(source);
        public static void WriteToSpan(Span<byte> target, BlobIdV1 value) => value.WriteTo(target);
    }
}
