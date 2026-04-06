using System;
using System.Buffers;

namespace DataFac.Storage;

public readonly struct CompressResult2
{
    public readonly BlobCompAlgo CompAlgo;
    public readonly BlobHashAlgo HashAlgo;
    public readonly ReadOnlyMemory<byte> Output;
    public CompressResult2(BlobCompAlgo compAlgo, BlobHashAlgo hashAlgo, ReadOnlyMemory<byte> output) : this()
    {
        CompAlgo = compAlgo;
        HashAlgo = hashAlgo;
        Output = output;
    }
}
public interface IBlobCompressor
{
#if NET7_0_OR_GREATER
    static abstract CompressResult2 CompressData(ReadOnlyMemory<byte> data, int maxEmbeddedSize = BlobIdV1.MaxEmbeddedSize);
    static abstract CompressResult2 CompressText(string text, Span<byte> hashSpan, int maxEmbeddedSize = BlobIdV1.MaxEmbeddedSize);
    static abstract ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> data);
#endif
}
