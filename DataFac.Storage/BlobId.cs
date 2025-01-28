using System.Globalization;
using System.Runtime.CompilerServices;
using System;
using System.Xml.Linq;

namespace Inventory.Store;

public readonly struct BlobId : IEquatable<BlobId> //, ISpanFormattable
{
    private const int V1Size = 32;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowArgumentOutOfRangeException(string name, object? value)
    {
        throw new ArgumentOutOfRangeException(name, value, null);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowInvalidLengthException(string name, int expectedSize)
    {
        throw new ArgumentException($"Expected {name}.Length to be {expectedSize}", name);
    }

    public static BlobId UnsafeWrap(ReadOnlyMemory<byte> id)
    {
        return new BlobId(id, true);
    }

    private readonly ReadOnlyMemory<byte> _id;
    private readonly int _hashCode;

    private BlobId(ReadOnlyMemory<byte> id, bool checkArgs)
    {
        if (checkArgs)
        {
            if (id.Length != V1Size) ThrowInvalidLengthException(nameof(id), V1Size);
        }
        _id = id;

        // pre-compute value for GetHashCode()
        var hashCode = new HashCode();
        hashCode.Add(_id.Length);
#if NET8_0_OR_GREATER
        hashCode.AddBytes(_id.Span);
#else
        var byteSpan = _id.Span;
        for (int i = 0; i < byteSpan.Length; i++)
        {
            hashCode.Add(byteSpan[i]);
        }
#endif
        _hashCode = hashCode.ToHashCode();
    }

    public BlobId() : this(ReadOnlyMemory<byte>.Empty, false) { }

    public BlobId(ReadOnlySpan<byte> id) : this(id.ToArray(), true) { }

    public BlobId(BlobId other)
    {
        _id = other._id;
        _hashCode = other._hashCode;
    }

    public bool IsEmpty => _id.IsEmpty;
    public ReadOnlyMemory<byte> Id => _id;

    public bool Equals(BlobId other) => other._hashCode == _hashCode && other._id.Span.SequenceEqual(_id.Span);
    public override bool Equals(object? obj) => obj is BlobId other && Equals(other);
    public override int GetHashCode() => _hashCode;

    /// <summary>
    /// Destination buffer size should be at least 72 chars.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="charsWritten"></param>
    private void WriteToSpan(Span<char> destination, out int charsWritten, IFormatProvider? provider)
    {
        int start = 0;
        "V1/".AsSpan().CopyTo(destination.Slice(start));
        start += 3;
        var hashSpan = _id.Span;
        for (int i = 0; i < hashSpan.Length; i++)
        {
            hashSpan[i].ToString("X2", provider).AsSpan().CopyTo(destination.Slice(start));
            start += 2;
        }
        charsWritten = start;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        WriteToSpan(destination, out charsWritten, provider);
        return true;
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        Span<char> buffer = stackalloc char[80];
        WriteToSpan(buffer, out int charsWritten, formatProvider);
#if NET8_0_OR_GREATER
        return new string(buffer.Slice(0, charsWritten));
#else
        return new string(buffer.Slice(0, charsWritten).ToArray());
#endif
    }

    public override string ToString()
    {
        return ToString(null, CultureInfo.InvariantCulture);
    }

    public static bool operator ==(BlobId left, BlobId right) => left.Equals(right);
    public static bool operator !=(BlobId left, BlobId right) => !left.Equals(right);
}
