using System;
using System.Diagnostics.CodeAnalysis;

namespace DataFac.Storage.Tests;

internal sealed class TestRecord : IEquatable<TestRecord>
{
    public readonly string Key; public readonly long Value;
    public TestRecord(string key, long value) { Key = key; Value = value; }
    public bool Equals(TestRecord? other) => other is null ? false : other.Key == Key && other.Value == Value;
    public override bool Equals(object? obj) => obj is TestRecord other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Key, Value);
    public override string ToString() => $"Key={Key};Value={Value}";
}

