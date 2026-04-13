using DataFac.Compression;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace DataFac.Storage;

public interface IDataStore : IDisposable
{
    // todo decouple BlobIdV1 formatting from the store. This will allow V2 and other formats to be used without needing to change the store.

    /// <summary>
    /// Returns the named key.
    /// </summary>
    BlobKey GetName(string name);

    /// <summary>
    /// Writes the named key to the store. Returns true if the name was added, 
    /// or false if the name was overwritten.
    /// </summary>
    bool PutName(string name, in BlobKey key);

    /// <summary>
    /// Enumerates all the names in the store.
    /// </summary>
    IEnumerable<KeyValuePair<string, BlobKey>> GetNames();

    /// <summary>
    /// Removes the named id if it exists.
    /// </summary>
    void RemoveName(string name);

    /// <summary>
    /// Returns the blob for the given id if it exists, null otherwise.
    /// </summary>
    ValueTask<BlobData> GetBlob(BlobKey key);

    /// <summary>
    /// Saves the given data into the underlying store, writes its id to the
    /// given memory, and optionally waits for any store operation to complete.
    /// </summary>
    ValueTask PutBlob(BlobKey key, BlobData data, bool withSync = false);

    IEnumerable<KeyValuePair<BlobKey, BlobData>> GetCachedBlobs();
    IEnumerable<KeyValuePair<BlobKey, BlobData>> GetStoredBlobs();

    /// <summary>
    /// Removes the blob if it exists.
    /// </summary>
    ValueTask<BlobData> RemoveBlob(BlobKey key, bool withSync);

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
