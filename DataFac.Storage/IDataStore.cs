using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Store;

public interface IDataStore : IDisposable
{
    /// <summary>
    /// Returns the named id, or null if not found.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    BlobId? GetName(string key);

    /// <summary>
    /// Writes the named id to the store. Returns true if the name was added, 
    /// or false if the name was overwritten.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    bool PutName(string key, in BlobId id);

    /// <summary>
    /// Enumerates all the names in the store.
    /// </summary>
    /// <returns></returns>
    KeyValuePair<string, BlobId>[] GetNames();

    /// <summary>
    /// Removes the named id if it exists.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    void RemoveName(string key);

    /// <summary>
    /// Removes the named ids if they exist.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    void RemoveNames(IEnumerable<string> keys);

    ValueTask<BlobData?> GetBlob(BlobId id);
    ValueTask<BlobId> PutBlob(BlobData data, bool withSync);

    KeyValuePair<BlobId, BlobData>[] GetCachedBlobs();
    KeyValuePair<BlobId, BlobData>[] GetStoredBlobs();

    /// <summary>
    /// Removes the blob if it exists.
    /// </summary>
    /// <param name="id"></param>
    ValueTask<BlobData?> RemoveBlob(BlobId id, bool withSync);

    /// <summary>
    /// Removes the blob if it exists.
    /// </summary>
    /// <param name="id"></param>
    ValueTask RemoveBlobs(IEnumerable<BlobId> ids, bool withSync);

    /// <summary>
    /// Ensures all writes are complete.
    /// </summary>
    /// <returns></returns>
    ValueTask Sync();

    /// <summary>
    /// Clears any blobs cached in memory. Returns the count of blobs cleared.
    /// </summary>
    int ClearCache();

    /// <summary>
    /// Returns a copy of the current counters.
    /// </summary>
    /// <returns></returns>
    Counters GetCounters();

    /// <summary>
    /// Resets the current counters, and returns the previous counters.
    /// </summary>
    /// <returns></returns>
    void ResetCounters();
}
