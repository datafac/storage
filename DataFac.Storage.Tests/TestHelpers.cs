using System.Collections.Generic;
using System;
using System.Linq;
using DataFac.Storage.Testing;
using DataFac.Storage.RocksDbStore;

#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace DataFac.Storage.Tests;

internal static class TestHelpers
{
    public static IDataStore CreateDataStore(StoreKind storeKind, string rocksDbRoot = "")
    {
        if (storeKind != StoreKind.Testing && string.IsNullOrEmpty(rocksDbRoot)) throw new System.ArgumentException($"'{nameof(rocksDbRoot)}' cannot be null or empty.", nameof(rocksDbRoot));
#pragma warning disable CA2000 // Dispose objects before losing scope
        return storeKind switch
        {
            StoreKind.Testing => new TestDataStore(),
            StoreKind.RocksDb => new RocksDbDataStore(rocksDbRoot),
            _ => throw new System.ArgumentOutOfRangeException(nameof(storeKind), storeKind, null),
        };
#pragma warning restore CA2000 // Dispose objects before losing scope
    }

    public static string? IsEqualTo<T>(this T? left, T? right) where T : class, IEquatable<T>
    {
        if (ReferenceEquals(left, right)) return null;
        if (left is null) return "left is null";
        if (right is null) return "right is null";
        if (!left.Equals(right)) return $"left ({left}) != right ({right})";
        return null;
    }

    public static string? IsEqualTo<T>(this IReadOnlyDictionary<string, T> left, IReadOnlyDictionary<string, T> right) where T : class, IEquatable<T>
    {
        if (ReferenceEquals(left, right)) return null;
        if (left.Count != right.Count) return $"left.Count ({left.Count}) != right.Count ({right.Count})";
        foreach (var aKey in left.Keys)
        {
            if (!right.TryGetValue(aKey, out T? bValue)) return $"right[{aKey}] is missing";
            T? aValue = left[aKey];
            if (!ReferenceEquals(aValue, bValue))
            {
                if (aValue is null) return $"left[{aKey}] is null";
                if (bValue is null) return $"right[{aKey}] is null";
                if (!aValue.Equals(bValue)) return $"left[{aKey}] ({aValue}) != right[{aKey}] ({bValue})";
            }
        }
        return null;
    }

    public static Difference<T>[] DifferentTo<T>(this IReadOnlyDictionary<string, T> previous, IReadOnlyDictionary<string, T> current) where T : class, IEquatable<T>
    {
        if (ReferenceEquals(previous, current)) return Array.Empty<Difference<T>>();
        List<Difference<T>> differences = new List<Difference<T>>();

        // look for removed or changed records
        foreach (var kvp in previous.OrderBy(r => r.Key))
        {
            if (current.TryGetValue(kvp.Key, out var currentValue))
            {
                // check for changes
                // todo return lambdas of actual changes
                if (!currentValue.Equals(kvp.Value))
                {
                    // changed
                    differences.Add(new Difference<T>(DifferenceKind.Changed, kvp.Key, kvp.Value, currentValue));
                }
            }
            else
            {
                // removed
                differences.Add(new Difference<T>(DifferenceKind.Removed, kvp.Key, kvp.Value, null));
            }
        }

        // look for added records
        foreach (var kvp in current.OrderBy(r => r.Key))
        {
            if (!previous.ContainsKey(kvp.Key))
            {
                // added
                differences.Add(new Difference<T>(DifferenceKind.Added, kvp.Key, null, kvp.Value));
            }
        }

        return differences.Count == 0 ? Array.Empty<Difference<T>>() : differences.ToArray();
    }

    /// <summary>
    /// Returns differences between this (current) collection and an optional previous collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="current"></param>
    /// <param name="previous"></param>
    /// <returns></returns>
    public static Difference<T>[] DifferentFrom<T>(this IReadOnlyDictionary<string, T> current, IReadOnlyDictionary<string, T>? previous) where T : class, IEquatable<T>
    {
        if (ReferenceEquals(previous, current)) return Array.Empty<Difference<T>>();

        if (previous is null)
        {
            // add all records
            List<Difference<T>> differences = new List<Difference<T>>();
            foreach (var kvp in current.OrderBy(r => r.Key))
            {
                differences.Add(new Difference<T>(DifferenceKind.Added, kvp.Key, null, kvp.Value));
            }

            return differences.Count == 0 ? Array.Empty<Difference<T>>() : differences.ToArray();
        }

        return previous.DifferentTo(current);
    }

}

