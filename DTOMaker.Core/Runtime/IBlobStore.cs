using System.Threading.Tasks;
using System.Threading;

namespace DTOMaker.Runtime
{
    public interface IBlobStore
    {
        /// <summary>
        /// Saves a blob and returns its id.
        /// </summary>
        ValueTask<BlobId> SaveAsync(Octets content, CancellationToken token);

        /// <summary>
        /// Gets the blob for the given id, or throws an exception.
        /// </summary>
        ValueTask<Octets> LoadAsync(BlobId id, CancellationToken token);

        /// <summary>
        /// Gets the blob for the given id, or null if not found.
        /// </summary>
        ValueTask<(bool, Octets?)> TryLoadAsync(BlobId id, CancellationToken token);
    }
}
