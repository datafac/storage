using System;

namespace DataFac.Storage;

public readonly struct BlobKey : IEquatable<BlobKey>
{
    private readonly static BlobKey _notFound = new BlobKey(false, ReadOnlyMemory<byte>.Empty);
    public static BlobKey NotFound() => _notFound;
    public static BlobKey From(ReadOnlyMemory<byte> bytes) => new BlobKey(true, bytes);

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
}
