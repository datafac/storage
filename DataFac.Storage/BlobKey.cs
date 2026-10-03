using System;
using System.Buffers;

namespace DataFac.Storage;

public readonly struct BlobKey : IEquatable<BlobKey>, IFormattable
#if NET8_0_OR_GREATER
, ISpanFormattable
#endif
{
    private readonly static BlobKey _notFound = new BlobKey(false, ReadOnlyMemory<byte>.Empty);
    public static BlobKey NotFound() => _notFound;
    public static BlobKey From(ReadOnlyMemory<byte> bytes) => new BlobKey(true, bytes);
    public static BlobKey From(ReadOnlySequence<byte> sequence)
    {
        if (sequence.IsEmpty)
        {
            return new BlobKey(true, ReadOnlyMemory<byte>.Empty);
        }
        else if (sequence.IsSingleSegment)
        {
            return new BlobKey(true, sequence.First);
        }
        else
        {
            Memory<byte> buffer = new byte[sequence.Length];
            sequence.CopyTo(buffer.Span);
            return new BlobKey(true, buffer);
        }
    }

    public readonly bool HasValue;
    public readonly ReadOnlyMemory<byte> Bytes;

    private BlobKey(bool hasValue, ReadOnlyMemory<byte> bytes)
    {
        HasValue = hasValue;
        Bytes = bytes;
    }

    public bool Equals(BlobKey other) => HasValue == other.HasValue && Bytes.Span.SequenceEqual(other.Bytes.Span);
    public override bool Equals(object? obj) => obj is BlobKey other && Equals(other);
    public override int GetHashCode()
    {
        var hasher = new HashCode();
        var span = Bytes.Span;
        hasher.Add(HasValue);
        hasher.Add(span.Length);
#if NET8_0_OR_GREATER
        hasher.AddBytes(span);
#else
        for (int i = 0; i < span.Length; i++) { hasher.Add(span[i]); }
#endif
        return hasher.ToHashCode();
    }

    public static bool operator ==(BlobKey left, BlobKey right) => left.Equals(right);
    public static bool operator !=(BlobKey left, BlobKey right) => !left.Equals(right);

    /// <summary>
    /// 64 chars that are file name compatible.
    /// </summary>
    private static readonly string _customMap = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-.";

    /// <summary>
    /// Encode bytes in the in a Base64-esque algorithm using a custom char map.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="charsWritten"></param>
    /// <returns></returns>
    private bool TryFormatCustomBase64(Span<char> destination, out int charsWritten)
    {
        ReadOnlySpan<char> customMap = _customMap.AsSpan();
        ReadOnlySpan<byte> source = this.Bytes.Span;
        Span<char> target = destination;

        int sourcePos = 0;
        charsWritten = 0;
        while (sourcePos + 3 <= source.Length && target.Length >= 4)
        {
            int value = (int)(source[sourcePos] << 16 | source[sourcePos + 1] << 8 | source[sourcePos + 2]);
            target[0] = customMap[(value >> 18) & 0x3F];
            target[1] = customMap[(value >> 12) & 0x3F];
            target[2] = customMap[(value >> 6) & 0x3F];
            target[3] = customMap[value & 0x3F];
            sourcePos += 3;
            charsWritten += 4;
            target = destination.Slice(charsWritten);
        }

        int remaining = source.Length - sourcePos;
        if (remaining == 2 && target.Length >= 4)
        {
            int value = source[sourcePos] << 16 | source[sourcePos + 1] << 8;
            target[0] = customMap[(value >> 18) & 0x3F];
            target[1] = customMap[(value >> 12) & 0x3F];
            target[2] = customMap[(value >> 6) & 0x3F];
            target[3] = '=';
            charsWritten += 4;
            return true;
        }
        else if (remaining == 1 && target.Length >= 4)
        {
            int value = source[sourcePos] << 16;
            target[0] = customMap[(value >> 18) & 0x3F];
            target[1] = customMap[(value >> 12) & 0x3F];
            target[2] = '=';
            target[3] = '=';
            charsWritten += 4;
            return true;
        }

        return remaining == 0;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return TryFormatCustomBase64(destination, out charsWritten);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        Span<char> buffer = stackalloc char[4 * ((Bytes.Length + 2) / 3)];
        if (!TryFormatCustomBase64(buffer, out int charsWritten))
        {
            throw new InvalidOperationException("Insufficient buffer space");
        }
        else
        {
#if NET8_0_OR_GREATER
            return new string(buffer.Slice(0, charsWritten));
#else
            return new string(buffer.Slice(0, charsWritten).ToArray());
#endif
        }
    }

    public override string ToString()
    {
        return ToString(null, null);
    }

}


#if NET8_0_OR_GREATER
#else
#endif
