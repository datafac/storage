using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

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

    public IEnumerable<string> GetNames() => _nameStore.Keys;

    public async ValueTask<BlobKey> GetName(string name, CancellationToken cancellation)
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

    public ValueTask PutName(string name, BlobKey key, CancellationToken cancellation)
    {
        if (string.IsNullOrEmpty(name)) ThrowMustNotBeEmpty(nameof(name));
        _nameStore[name] = key;
        return default;
    }

}
