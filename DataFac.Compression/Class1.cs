using DataFac.Memory;
using DataFac.UnsafeHelpers;
using Snappier;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DataFac.Compression;

public enum BlobCompAlgo : byte
{
    UnComp = 0, // 'U'
    Brotli = 1, // 'B'
    Snappy = 2, // 'S'
}

public enum BlobHashAlgo : byte
{
    None = 0,
    Sha256 = 1,
}

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


public readonly struct BlobIdV1 : IEquatable<BlobIdV1>
{
    public const int Size = 64;
    public const int MaxEmbeddedSize = Size - 2; // 62 bytes for embedded data, 2 bytes for marker and length

    private readonly BlockB064 _block;
    public BlockB064 Block => _block;

    // map
    //  offset  path        len fieldname
    //  00      A.A.A.A.A.A 1   Marker00 '|' for non-embedded or compAlgo char code if embedded
    //  01      A.A.A.A.A.B 1   Marker01 '_' for non-embedded or data length if embedded
    //  02      A.A.A.A.B.A 1   MajorVer
    //  03      A.A.A.A.B.B 1   MinorVer
    //  04      A.A.A.B.A.A 1   CompAlgo
    //  05      A.A.A.B.A.B 1   HashAlgo
    //  06-07   A.A.A.B.B   2   -unused-
    //  08-0B   A.A.B.A     4   BlobSize
    //  0C-0F   A.A.B.B     4   -unused-
    //  10-1F   A.B         16  -unused-
    //  20-3F   B           32  HashData
    public byte Marker00 => _block.A.A.A.A.A.A.ByteValue; // _memory.Span[0];
    public byte Marker01 => _block.A.A.A.A.A.B.ByteValue; // _memory.Span[1];
    public byte MajorVer => _block.A.A.A.A.B.A.ByteValue; // _memory.Span[2];
    public byte MinorVer => _block.A.A.A.A.B.B.ByteValue; // _memory.Span[3];
    public BlobCompAlgo CompAlgo
    {
        get
        {
            return IsEmbedded
                ? _block.A.A.A.A.A.A.ByteValue.ToCompAlgo()     // _memory.Span[0];
                : (BlobCompAlgo)_block.A.A.A.B.A.A.ByteValue;   // _memory.Span[4];
        }
    }

    public BlobHashAlgo HashAlgo
    {
        get
        {
            return IsEmbedded
                ? BlobHashAlgo.None // embedded blobs do not have hash algo, as they are small enough to be stored directly
                : (BlobHashAlgo)_block.A.A.A.B.A.B.ByteValue;   // _memory.Span[5];
        }
    }

    public int BlobSize => _block.A.A.B.A.Int32ValueLE; // Codec_Int32_LE.ReadFromSpan(_memory.Span.Slice(8, 4));
    public BlockB032 HashData => _block.B; // _memory.Slice(32, 32);

    public bool IsDefault => _block.IsEmpty;

    private BlobIdV1(ReadOnlySpan<byte> source)
    {
        if (source.Length != BlobIdV1.Size) throw new ArgumentException($"Length must be {BlobIdV1.Size}.", nameof(source));
        _block.TryRead(source);
    }

    private BlobIdV1(BlockB064 source)
    {
        _block = source;
    }

    public static BlobIdV1 FromSpan(ReadOnlySpan<byte> source) => new BlobIdV1(source);

    public static BlobIdV1 FromBlock(BlockB064 source) => new BlobIdV1(source);

    /// <summary>
    /// Used to directly embed blob data which is small enough into the id.
    /// </summary>
    /// <param name="compAlgo"></param>
    /// <param name="data"></param>
    /// <exception cref="ArgumentException"></exception>
    public BlobIdV1(BlobCompAlgo compAlgo, ReadOnlyMemory<byte> data)
    {
        if (data.Length > MaxEmbeddedSize) throw new ArgumentException("Length must be <= 62", nameof(data));
        _block.A.A.A.A.A.A.ByteValue = compAlgo.ToCharCode();
        _block.A.A.A.A.A.B.ByteValue = (byte)(data.Length + (byte)'A');
        data.Span.CopyTo(BlockHelper.AsWritableSpan(ref _block).Slice(2));
    }

    public static void WriteEmbedded(Span<byte> target, BlobCompAlgo compAlgo, ReadOnlyMemory<byte> data)
    {
        if (target.Length != Size) throw new ArgumentException($"Length must be {Size}.", nameof(target));
        if (data.Length > MaxEmbeddedSize) throw new ArgumentException("Length must be <= 62", nameof(data));
        target.Clear();
        target[0] = compAlgo.ToCharCode();
        target[1] = (byte)(data.Length + (byte)'A');
        data.Span.CopyTo(target.Slice(2));
    }

    /// <summary>
    /// Used to directly embed blob data which is small enough into the id.
    /// </summary>
    /// <param name="compAlgo"></param>
    /// <param name="data"></param>
    /// <exception cref="ArgumentException"></exception>
    public BlobIdV1(BlobCompAlgo compAlgo, ReadOnlySpan<byte> data)
    {
        if (data.Length > MaxEmbeddedSize) throw new ArgumentException("Length must be <= 62", nameof(data));
        _block.A.A.A.A.A.A.ByteValue = compAlgo.ToCharCode();
        _block.A.A.A.A.A.B.ByteValue = (byte)(data.Length + (byte)'A');
        data.CopyTo(BlockHelper.AsWritableSpan(ref _block).Slice(2));
    }

    private BlobIdV1(byte majorVer, byte minorVer, int blobSize, BlobCompAlgo compAlgo, BlobHashAlgo hashAlgo, ReadOnlySpan<byte> hashData)
    {
        if (hashData.Length != 32) throw new ArgumentException("Length must be == 32", nameof(hashData));
        _block.A.A.A.A.A.A.ByteValue = (byte)'|';   // Marker00
        _block.A.A.A.A.A.B.ByteValue = (byte)'_';   // Marker01
        _block.A.A.A.A.B.A.ByteValue = majorVer;
        _block.A.A.A.A.B.B.ByteValue = minorVer;
        _block.A.A.A.B.A.A.ByteValue = (byte)compAlgo;
        _block.A.A.A.B.A.B.ByteValue = (byte)hashAlgo;
        _block.A.A.B.A.Int32ValueLE = blobSize;
        hashData.CopyTo(BlockHelper.AsWritableSpan(ref _block).Slice(32, 32));
    }

    public BlobIdV1(int blobSize, BlobCompAlgo compAlgo, BlobHashAlgo hashAlgo, ReadOnlySpan<byte> hashData)
        : this(1, 0, blobSize, compAlgo, hashAlgo, hashData) { }

    private static void WriteHeadPart(Span<byte> target, int blobSize, BlobCompAlgo compAlgo, BlobHashAlgo hashAlgo)
    {
        target[0] = (byte)'|';   // Marker00
        target[1] = (byte)'_';   // Marker01
        target[2] = (byte)1;
        target[3] = (byte)0;
        target[4] = (byte)compAlgo;
        target[5] = (byte)hashAlgo;
        BinaryPrimitives.WriteInt32LittleEndian(target.Slice(8, 4), blobSize);
    }
    public static void Write(Span<byte> target, int blobSize, BlobCompAlgo compAlgo, BlobHashAlgo hashAlgo, ReadOnlySpan<byte> hashData)
    {
        if (target.Length != Size) throw new ArgumentException($"Length must be {Size}.", nameof(target));
        if (hashData.Length != 32) throw new ArgumentException("Length must be == 32", nameof(hashData));
        target.Clear();
        WriteHeadPart(target, blobSize, compAlgo, hashAlgo);
        hashData.CopyTo(target.Slice(32, 32));
    }
    public static void WriteSansHash(Span<byte> target, int blobSize, BlobCompAlgo compAlgo, BlobHashAlgo hashAlgo)
    {
        if (target.Length != Size) throw new ArgumentException($"Length must be {Size}.", nameof(target));
        var header = target.Slice(0, 32); // header is the first 32 bytes, hash data is the last 32 bytes
        header.Clear();
        WriteHeadPart(header, blobSize, compAlgo, hashAlgo);
    }
    public void WriteTo(Span<byte> target) => _block.TryWrite(target);

    public byte[] ToByteArray() => _block.ToByteArray();

    public bool IsEmbedded => (Marker00 != 0) && Marker00 != (byte)'|';

    public bool TryGetEmbeddedBlob(out ReadOnlyMemory<byte> embedded)
    {
        embedded = ReadOnlyMemory<byte>.Empty;
        if (!IsEmbedded) return false;
        switch (Marker00.ToCompAlgo())
        {
            case BlobCompAlgo.UnComp:
                int dataSize0 = Marker01 - (byte)'A';
                embedded = new ReadOnlyMemory<byte>(_block.ToByteArray(2, dataSize0));
                return true;
            case BlobCompAlgo.Brotli:
                throw new NotImplementedException("Brotli embedded blobs are not implemented yet.");
            case BlobCompAlgo.Snappy:
                int dataSize2 = Marker01 - (byte)'A';
                var compressedData = new ReadOnlyMemory<byte>(_block.ToByteArray(2, dataSize2));
                embedded = SnappyCompressor.Decompress(compressedData);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Formats the blob id as a round-trip string.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (IsDefault) return string.Empty;
        StringBuilder result = new StringBuilder();
        if (IsEmbedded)
        {
            char marker = (char)Marker00;
            int dataSize = Marker01 - (byte)'A';
            result.Append(marker);
            result.Append(':');
            result.Append(dataSize);
            result.Append(':');
            result.Append(_block.ToBase64String(2, dataSize));
            return result.ToString();
        }

        result.Append($"V{MajorVer}.{MinorVer}:");
        result.Append(BlobSize);
        result.Append(':');
        result.Append((char)CompAlgo.ToCharCode());
        result.Append(':');
        result.Append((int)HashAlgo);
        result.Append(':');
        if (HashAlgo != BlobHashAlgo.None)
        {
            result.Append(_block.ToBase64String(32, 32));
        }
        return result.ToString();
    }

    /// <summary>
    /// Parses a formatted string to a blob id.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    public static BlobIdV1 FromString(string source)
    {
        if (string.IsNullOrEmpty(source)) return default;
        var sourceSpan = source.AsSpan();
        int partId = 0;
        int blobSize = default;
        int compAlgoInt = default;
        var compAlgo = BlobCompAlgo.UnComp;
        int hashAlgoInt = default;
        var hashAlgo = BlobHashAlgo.None;
        Span<byte> hashSpan = stackalloc byte[32];
        while (sourceSpan.Length > 0)
        {
            var sourceIndex = sourceSpan.IndexOf(':');
            ReadOnlySpan<char> partSpan = sourceIndex >= 0 ? sourceSpan.Slice(0, sourceIndex) : sourceSpan;
            string part = partSpan.ToString();
            switch (partId)
            {
                case 0: // header
                    if (part != "V1.0") throw new InvalidDataException($"Invalid version: '{part}'.");
                    break;
                case 1:
                    if (!int.TryParse(part, out blobSize)) throw new InvalidDataException($"Invalid blobSize: '{part}'.");
                    break;
                case 2:
                    if (!int.TryParse(part, out compAlgoInt)) throw new InvalidDataException($"Invalid compAlgo: '{part}'.");
                    compAlgo = (BlobCompAlgo)compAlgoInt;
                    break;
                case 3:
                    if (!int.TryParse(part, out hashAlgoInt)) throw new InvalidDataException($"Invalid hashAlgo: '{part}'.");
                    hashAlgo = (BlobHashAlgo)hashAlgoInt;
                    break;
                case 4:
#if NET8_0_OR_GREATER
                        if (!Convert.TryFromBase64Chars(partSpan, hashSpan, out int bytesDecoded) || bytesDecoded != 32) throw new InvalidDataException($"Invalid hashData: '{part}'.");
#else
                    byte[] hashBytes = Convert.FromBase64String(part);
                    hashBytes.CopyTo(hashSpan);
#endif
                    break;
                default:
                    throw new InvalidDataException($"Unexpected format: '{source}'");
            }
            // next
            partId++;
            sourceSpan = sourceIndex >= 0 ? sourceSpan.Slice(sourceIndex + 1) : default;
        }
        return new BlobIdV1(blobSize, compAlgo, hashAlgo, hashSpan);
    }

    public bool Equals(BlobIdV1 that) => _block.Equals(that._block);
    public override bool Equals(object? obj) => obj is BlobIdV1 other && Equals(other);
    public override int GetHashCode() => _block.GetHashCode();

    public static bool operator ==(BlobIdV1 left, BlobIdV1 right) => left.Equals(right);
    public static bool operator !=(BlobIdV1 left, BlobIdV1 right) => !left.Equals(right);
}

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
public interface IBlobCompressor
{
#if NET7_0_OR_GREATER
    static abstract CompressResult2 CompressData(ReadOnlyMemory<byte> data, Span<byte> hashSpan, int maxEmbeddedSize = BlobIdV1.MaxEmbeddedSize);
    static abstract CompressResult2 CompressText(string text, Span<byte> hashSpan, int maxEmbeddedSize = BlobIdV1.MaxEmbeddedSize);
    static abstract ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> data);
#endif
}

public interface IBlobHasher
{
#if NET7_0_OR_GREATER
    static abstract void ComputeHash(ReadOnlySpan<byte> data, Span<byte> hashOutput);
#endif
}

public sealed class SHA256Hasher : IBlobHasher
{
    public static void ComputeHash(ReadOnlySpan<byte> data, Span<byte> hashOutput)
    {
        // incremental hasher for SHA-256
        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
#if NET8_0_OR_GREATER
        hasher.AppendData(data);
        if (!hasher.TryGetHashAndReset(hashOutput, out int bytesWritten))
        {
            throw new InvalidOperationException("Hash output buffer too small");
        }
#else
        hasher.AppendData(data.ToArray());
        byte[] hashBytes = hasher.GetHashAndReset();
        hashBytes.CopyTo(hashOutput);
#endif
    }
}

public sealed class SnappyCompressor : IBlobCompressor
{
    public static ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> compressedData)
    {
        ReadOnlySequence<byte> inputSequence = new ReadOnlySequence<byte>(compressedData);
        var buffers = new ByteBufferWriter();
        Snappy.Decompress(inputSequence, buffers);
        var decompressedData = buffers.GetWrittenSequence();
        return decompressedData.Compact();
    }

    public static CompressResult2 CompressData(ReadOnlyMemory<byte> data, Span<byte> hashSpan, int maxEmbeddedSize = BlobIdV1.MaxEmbeddedSize)
    {
        if (hashSpan.Length != 32) throw new ArgumentException("Length must be 32 bytes for SHA256", nameof(hashSpan));
        // compress using stack allocation only for small buffers, otherwise use heap allocation
        // if compressed is smaller, return compressed bytes; otherwise return original bytes
        if (data.Length <= maxEmbeddedSize)
        {
            return new CompressResult2(data.Length, BlobHashAlgo.None, BlobCompAlgo.UnComp, data);
        }
        SHA256Hasher.ComputeHash(data.Span, hashSpan);
        if (data.Length < 1024)
        {
            Span<byte> outputSpan = stackalloc byte[data.Span.Length + 128];
            if (Snappy.TryCompress(data.Span, outputSpan, out int bytesWritten))
            {
                return bytesWritten < data.Length
                    ? new CompressResult2(data.Length, BlobHashAlgo.Sha256, BlobCompAlgo.Snappy, outputSpan.Slice(0, bytesWritten).ToArray())
                    : new CompressResult2(data.Length, BlobHashAlgo.Sha256, BlobCompAlgo.UnComp, data);
            }
        }

        ReadOnlySequence<byte> inputSequence = new ReadOnlySequence<byte>(data);
        var buffers = new ByteBufferWriter();
        Snappy.Compress(inputSequence, buffers);
        var compressedData = buffers.GetWrittenSequence();

        return compressedData.Length < data.Length
            ? new CompressResult2(data.Length, BlobHashAlgo.Sha256, BlobCompAlgo.Snappy, compressedData.Compact())
            : new CompressResult2(data.Length, BlobHashAlgo.Sha256, BlobCompAlgo.UnComp, data);
    }

    public static CompressResult2 CompressText(string text, Span<byte> hashSpan, int maxEmbeddedSize = BlobIdV1.MaxEmbeddedSize)
    {

        if (hashSpan.Length != 32) throw new ArgumentException("Length must be 32 bytes for SHA256", nameof(hashSpan));
        // compress using stack allocation only for small strings, otherwise use heap allocation
        //int estimatedSize = Encoding.UTF8.GetByteCount(text);
        if (text.Length < 1024)
        {
#if NET8_0_OR_GREATER
            Span<byte> localBuffer = stackalloc byte[1024];
            if (Encoding.UTF8.TryGetBytes(text, localBuffer, out int bytesEncoded))
            {
                var inputSpan = localBuffer.Slice(0, bytesEncoded);
                if (bytesEncoded <= maxEmbeddedSize)
                {
                    return new CompressResult2(bytesEncoded, BlobHashAlgo.None, BlobCompAlgo.UnComp, inputSpan.ToArray());
                }
                SHA256Hasher.ComputeHash(inputSpan, hashSpan);
                Span<byte> outputSpan = stackalloc byte[bytesEncoded];
                if (Snappy.TryCompress(inputSpan, outputSpan, out int bytesWritten))
                {
                    return bytesWritten < bytesEncoded
                        ? new CompressResult2(bytesEncoded, BlobHashAlgo.Sha256, BlobCompAlgo.Snappy, outputSpan.Slice(0, bytesWritten).ToArray())
                        : new CompressResult2(bytesEncoded, BlobHashAlgo.Sha256, BlobCompAlgo.UnComp, inputSpan.ToArray());
                }
            }
#else
            ReadOnlyMemory<byte> inputMemory1 = System.Text.Encoding.UTF8.GetBytes(text);
            if (inputMemory1.Length <= maxEmbeddedSize)
            {
                return new CompressResult2(inputMemory1.Length, BlobHashAlgo.None, BlobCompAlgo.UnComp, inputMemory1);
            }
            SHA256Hasher.ComputeHash(inputMemory1.Span, hashSpan);
            Span<byte> outputSpan = stackalloc byte[inputMemory1.Length * 2];
            if (Snappy.TryCompress(inputMemory1.Span, outputSpan, out int bytesWritten))
            {
                return bytesWritten < inputMemory1.Length
                    ? new CompressResult2(inputMemory1.Length, BlobHashAlgo.Sha256, BlobCompAlgo.Snappy, outputSpan.Slice(0, bytesWritten).ToArray())
                    : new CompressResult2(inputMemory1.Length, BlobHashAlgo.Sha256, BlobCompAlgo.UnComp, inputMemory1);
            }
#endif
        }

        // default to heap allocation for larger strings or if stack allocation fails
        ReadOnlyMemory<byte> inputMemory2 = Encoding.UTF8.GetBytes(text);
        if (inputMemory2.Length <= maxEmbeddedSize)
        {
            return new CompressResult2(inputMemory2.Length, BlobHashAlgo.None, BlobCompAlgo.UnComp, inputMemory2);
        }
        SHA256Hasher.ComputeHash(inputMemory2.Span, hashSpan);
        var inputSequence = new ReadOnlySequence<byte>(inputMemory2);
        var buffers = new ByteBufferWriter();
        Snappy.Compress(inputSequence, buffers);
        var compressedData = buffers.GetWrittenSequence().Compact();
        return compressedData.Length < inputMemory2.Length
            ? new CompressResult2(inputMemory2.Length, BlobHashAlgo.Sha256, BlobCompAlgo.Snappy, compressedData)
            : new CompressResult2(inputMemory2.Length, BlobHashAlgo.Sha256, BlobCompAlgo.UnComp, inputMemory2);
    }
}