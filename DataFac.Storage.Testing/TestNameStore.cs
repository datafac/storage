using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DataFac.Storage.Testing;

/// <summary>
/// Implements an in-memory name store. Useful for unit testing.
/// </summary>
public sealed class TestNameStore : INameStore
{
    private readonly ConcurrentDictionary<string, BlobKey> _nameStore = new ConcurrentDictionary<string, BlobKey>();

    public TestNameStore()
    {
    }

    public void Dispose()
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowMustNotBeEmpty(string name)
    {
        throw new ArgumentException("Must not be empty", name);
    }

    public IEnumerable<KeyValuePair<string, BlobKey>> GetNames() => _nameStore;

    public BlobKey GetName(string name)
    {
        if (string.IsNullOrEmpty(name)) ThrowMustNotBeEmpty(nameof(name));

        return _nameStore.TryGetValue(name, out var id) ? id : BlobKey.NotFound();
    }

    public void RemoveName(string name)
    {
        _nameStore.TryRemove(name, out var _);
    }

    public void RemoveNames(IEnumerable<string> names)
    {
        if (names is null) throw new ArgumentNullException(nameof(names));

        foreach (var name in names)
        {
            _nameStore.TryRemove(name, out var _);
        }
    }

    public bool PutName(string name, in BlobKey key)
    {
        if (string.IsNullOrEmpty(name)) ThrowMustNotBeEmpty(nameof(name));
        bool added = _nameStore.TryAdd(name, key);
        return added;
    }

}
