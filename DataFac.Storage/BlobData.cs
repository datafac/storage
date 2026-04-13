using System;

namespace DataFac.Storage;

public readonly struct BlobData : IEquatable<BlobData>
{
    private readonly static BlobData _notFound = new BlobData(false, ReadOnlyMemory<byte>.Empty);
    public static BlobData NotFound() => _notFound;
    public static BlobData From(ReadOnlyMemory<byte> bytes) => new BlobData(true, bytes);

    public readonly bool HasValue;
    public readonly ReadOnlyMemory<byte> Bytes;

    private BlobData(bool hasValue, ReadOnlyMemory<byte> bytes)
    {
        HasValue = hasValue;
        Bytes = bytes;
    }

    public bool Equals(BlobData other) => HasValue == other.HasValue && Bytes.Span.SequenceEqual(other.Bytes.Span);
    public override bool Equals(object? obj) => obj is BlobData other && Equals(other);
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
    public static bool operator ==(BlobData left, BlobData right) => left.Equals(right);
    public static bool operator !=(BlobData left, BlobData right) => !left.Equals(right);
}
