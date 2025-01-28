using System;

namespace Inventory.Tests;

internal readonly struct Difference<T> where T : class, IEquatable<T>
{
    public readonly DifferenceKind Kind;
    public readonly string Key;
    public readonly T? Initial;
    public readonly T? Current;

    public Difference(DifferenceKind kind, string key, T? initial, T? current)
    {
        Kind = kind;
        Key = key;
        Initial = initial;
        Current = current;
    }
}

