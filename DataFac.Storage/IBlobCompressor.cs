using System;
using System.Buffers;

namespace DataFac.Storage;

public readonly struct CompressResult2
{
    public readonly int InputSize;
    public readonly ReadOnlyMemory<byte> InputHash;
    public readonly BlobCompAlgo CompAlgo;
    public readonly BlobHashAlgo HashAlgo;
    public readonly ReadOnlyMemory<byte> Output;
    public CompressResult2(int inputSize, BlobHashAlgo hashAlgo, ReadOnlyMemory<byte> inputHash, BlobCompAlgo compAlgo, ReadOnlyMemory<byte> output) : this()
    {
        InputSize = inputSize;
        HashAlgo = hashAlgo;
        InputHash = inputHash;
        CompAlgo = compAlgo;
        Output = output;
    }
}
public interface IBlobCompressor
{
#if NET7_0_OR_GREATER
    static abstract CompressResult2 CompressData(ReadOnlyMemory<byte> data, int maxEmbeddedSize = BlobIdV1.MaxEmbeddedSize);
    static abstract CompressResult2 CompressText(string text, int maxEmbeddedSize = BlobIdV1.MaxEmbeddedSize);
    static abstract ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> data);
#endif
}
