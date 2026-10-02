using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace DataFac.Storage.RocksDbStore;

public sealed class RocksDbNameStore : INameStore
{
#pragma warning disable CA2213 // Disposable fields should be disposed
    private readonly RocksDb _rocksNameDb;
#pragma warning restore CA2213 // Disposable fields should be disposed

    private const int MaxStackallocKeySize = 128; // todo tune size

    public RocksDbNameStore(string rootpath)
    {
        DbOptions dbOptions = new DbOptions().SetCreateIfMissing(true);

        string namePath = $"{rootpath}\\names";
        Directory.CreateDirectory(namePath);
        _rocksNameDb = RocksDb.Open(dbOptions, namePath);
    }

    private volatile bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _rocksNameDb.Dispose();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowMustNotBeEmpty(string paramName)
    {
        throw new ArgumentException("Must not be empty", paramName);
    }

#if NET8_0_OR_GREATER
    public IEnumerable<KeyValuePair<string, BlobKey>> GetNames()
    {
        using var iter = _rocksNameDb.NewIterator();
        var iter2 = iter.SeekToFirst();
        while (iter2.Valid())
        {
            string name = Encoding.UTF8.GetString(iter2.GetKeySpan());
            BlobKey key = BlobKey.From(iter2.Value());
            yield return new KeyValuePair<string, BlobKey>(name, key);
            iter2 = iter2.Next();
        }
    }
#else
    public IEnumerable<KeyValuePair<string, BlobKey>> GetNames()
    {
        using var iter = _rocksNameDb.NewIterator();
        var iter2 = iter.SeekToFirst();
        while (iter2.Valid())
        {
            string name = Encoding.UTF8.GetString(iter2.Key());
            BlobKey key = BlobKey.From(iter2.Value());
            yield return new KeyValuePair<string, BlobKey>(name, key);
            iter2 = iter2.Next();
        }
    }
#endif

#if NET8_0_OR_GREATER
    public BlobKey GetName(string name)
    {
        if (string.IsNullOrEmpty(name)) ThrowMustNotBeEmpty(nameof(name));
        Span<byte> buffer = stackalloc byte[MaxStackallocKeySize];
        if (Encoding.UTF8.TryGetBytes(name, buffer, out int bytesInKey))
        {
            var keySpan = buffer.Slice(0, bytesInKey);
            var bytes1 = _rocksNameDb.Get(keySpan);
            return bytes1 is null ? BlobKey.NotFound() : BlobKey.From(bytes1);
        }
        // fallback if key too large for stackalloc
        var keyBytes = Encoding.UTF8.GetBytes(name);
        var bytes2 = _rocksNameDb.Get(keyBytes);
        return bytes2 is null ? BlobKey.NotFound() : BlobKey.From(bytes2);
    }
#else
    public BlobKey GetName(string name)
    {
        var keyBytes = Encoding.UTF8.GetBytes(name);
        var bytes2 = _rocksNameDb.Get(keyBytes);
        return bytes2 is null ? BlobKey.NotFound() : BlobKey.From(bytes2);
    }
#endif

#if NET8_0_OR_GREATER
    public void RemoveName(string name)
    {
        if (string.IsNullOrEmpty(name)) ThrowMustNotBeEmpty(nameof(name));
        Span<byte> buffer = stackalloc byte[MaxStackallocKeySize];
        if (Encoding.UTF8.TryGetBytes(name, buffer, out int bytesInKey))
        {
            var keySpan = buffer.Slice(0, bytesInKey);
            _rocksNameDb.Remove(keySpan);
        }
        // fallback if key too large for stackalloc
        var keyBytes = Encoding.UTF8.GetBytes(name);
        _rocksNameDb.Remove(keyBytes);
    }
#else
    public void RemoveName(string name)
    {
        var keyBytes = Encoding.UTF8.GetBytes(name);
        _rocksNameDb.Remove(keyBytes);
    }
#endif

#if NET8_0_OR_GREATER
    public bool PutName(string name, in BlobKey key)
    {
        if (string.IsNullOrEmpty(name)) ThrowMustNotBeEmpty(nameof(name));
        if (!key.HasValue) ThrowMustNotBeEmpty(nameof(key));
        // todo? optimistic locking revision check
        // todo? lock on key to ensure below is atomic
        Span<byte> buffer = stackalloc byte[MaxStackallocKeySize];
        if (Encoding.UTF8.TryGetBytes(name, buffer, out int bytesInKey))
        {
            var keySpan = buffer.Slice(0, bytesInKey);
            bool added = _rocksNameDb.Get(keySpan) is null;
            if (added)
            {
                _rocksNameDb.Put(keySpan, key.Bytes.Span);
            }
            return added;
        }
        {
            var keyBytes = Encoding.UTF8.GetBytes(name);
            bool added = _rocksNameDb.Get(keyBytes) is null;
            if (added)
            {
                _rocksNameDb.Put(keyBytes, key.Bytes.ToArray());
            }
            return added;
        }
    }
#else
    public bool PutName(string name, in BlobKey key)
    {
        if (string.IsNullOrEmpty(name)) ThrowMustNotBeEmpty(nameof(name));
        if (!key.HasValue) ThrowMustNotBeEmpty(nameof(key));
        // todo? optimistic locking revision check
        // todo? lock on key to ensure below is atomic
        {
            var keyBytes = Encoding.UTF8.GetBytes(name);
            bool added = _rocksNameDb.Get(keyBytes) is null;
            if (added)
            {
                _rocksNameDb.Put(keyBytes, key.Bytes.ToArray());
            }
            return added;
        }
    }
#endif

}
