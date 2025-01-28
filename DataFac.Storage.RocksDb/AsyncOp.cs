using System.Threading.Tasks;

namespace Inventory.Store.RocksDbStore;

internal readonly struct AsyncOp
{
    public readonly AsyncOpKind Kind;
    public readonly BlobId Id;
    public readonly BlobData Data;
    public readonly TaskCompletionSource<BlobData?>? Completion;

    public AsyncOp(AsyncOpKind kind, BlobId id, BlobData data, TaskCompletionSource<BlobData?>? completion)
    {
        Kind = kind;
        Id = id;
        Data = data;
        Completion = completion;
    }

    /// <summary>
    /// Async SYNC
    /// </summary>
    /// <param name="completion"></param>
    public AsyncOp(TaskCompletionSource<BlobData?> completion)
    {
        Kind = AsyncOpKind.Sync;
        Completion = completion;
    }

    /// <summary>
    /// Async GET
    /// </summary>
    /// <param name="id"></param>
    /// <param name="completion"></param>
    public AsyncOp(BlobId id, TaskCompletionSource<BlobData?> completion)
    {
        Kind = AsyncOpKind.Get;
        Id = id;
        Completion = completion;
    }

    /// <summary>
    /// Async PUT
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    public AsyncOp(BlobId id, BlobData data, TaskCompletionSource<BlobData?>? completion)
    {
        Kind = AsyncOpKind.Putqqq;
        Id = id;
        Data = data;
        Completion = completion;
    }
}
