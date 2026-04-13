using System;

namespace DataFac.Compression;

public interface IBlobCompressor
{
#if NET7_0_OR_GREATER
    static abstract CompressResult2 CompressData(ReadOnlyMemory<byte> data, Span<byte> hashSpan, int maxEmbeddedSize = BlobIdV1.MaxEmbeddedSize);
    static abstract CompressResult2 CompressText(string text, Span<byte> hashSpan, int maxEmbeddedSize = BlobIdV1.MaxEmbeddedSize);
    static abstract ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> data);
#endif
}
