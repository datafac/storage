using System;

namespace DataFac.Storage;

/// <summary>
/// todo drop this type in favour of Octets
/// </summary>
public readonly struct BlobData : IEquatable<BlobData>
{
    public static BlobData UnsafeWrap(ReadOnlyMemory<byte> data) => new BlobData(data);

    private readonly ReadOnlyMemory<byte> _data;
    private BlobData(ReadOnlyMemory<byte> data) => _data = data;
    public BlobData() => _data = ReadOnlyMemory<byte>.Empty;
    public BlobData(ReadOnlySpan<byte> data) => _data = data.ToArray();
    public BlobData(BlobData other) => _data = other._data;

    public bool IsEmpty => _data.IsEmpty;
    public ReadOnlyMemory<byte> Memory => _data;

    public bool Equals(BlobData other) => other._data.Span.SequenceEqual(_data.Span);
    public override bool Equals(object? obj) => obj is BlobData other && Equals(other);
    public override int GetHashCode()
    {
        // todo? compute once and cache
        var hash = new HashCode();
        hash.Add(_data.Length);
#if NET8_0_OR_GREATER
        hash.AddBytes(_data.Span);
#else
        var byteSpan = _data.Span;
        for (int i = 0; i < byteSpan.Length; i++)
        {
            hash.Add(byteSpan[i]);
        }
#endif
        return hash.ToHashCode();
    }

    public static bool operator ==(BlobData left, BlobData right) => left.Equals(right);
    public static bool operator !=(BlobData left, BlobData right) => !left.Equals(right);
}
