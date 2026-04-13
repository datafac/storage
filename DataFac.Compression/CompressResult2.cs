using System;

namespace DataFac.Compression;

public readonly struct CompressResult2
{
    public readonly int InputSize;
    public readonly BlobCompAlgo CompAlgo;
    public readonly BlobHashAlgo HashAlgo;
    public readonly ReadOnlyMemory<byte> Output;
    public CompressResult2(int inputSize, BlobHashAlgo hashAlgo, BlobCompAlgo compAlgo, ReadOnlyMemory<byte> output) : this()
    {
        InputSize = inputSize;
        HashAlgo = hashAlgo;
        CompAlgo = compAlgo;
        Output = output;
    }
}
