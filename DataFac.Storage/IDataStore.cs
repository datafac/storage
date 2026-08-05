using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Storage;

public interface INameStore : IDisposable
{
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
}

public interface IBlobCache : IDisposable
{
    /// <summary>
    /// Clears any blobs cached in memory. Returns the count of blobs cleared.
    /// </summary>
    int Clear();

    /// <summary>
    /// Returns the blob for the given id if it exists, null otherwise.
    /// </summary>
    ValueTask<BlobData> GetBlob(BlobKey key);

    /// <summary>
    /// Saves the given data into the underlying store, writes its id to the
    /// given memory, and optionally waits for any store operation to complete.
    /// </summary>
    ValueTask PutBlob(BlobKey key, BlobData data, bool withSync = false);

    /// <summary>
    /// Removes the blob if it exists.
    /// </summary>
    ValueTask<BlobData> RemoveBlob(BlobKey key, bool withSync);

    /// <summary>
    /// Ensures all writes are complete.
    /// </summary>
    /// <returns></returns>
    ValueTask Sync();
}

public interface IBlobStore : IDisposable
{
    /// <summary>
    /// Returns the blob for the given id if it exists, null otherwise.
    /// </summary>
    ValueTask<BlobData> GetBlob(BlobKey key);

    /// <summary>
    /// Saves the given data into the underlying store, writes its id to the
    /// given memory, and optionally waits for any store operation to complete.
    /// </summary>
    ValueTask PutBlob(BlobKey key, BlobData data);

    IAsyncEnumerable<KeyValuePair<BlobKey, BlobData>> GetBlobs(CancellationToken cancellation);

    /// <summary>
    /// Removes the blob if it exists.
    /// </summary>
    ValueTask<BlobData> RemoveBlob(BlobKey key);
}
