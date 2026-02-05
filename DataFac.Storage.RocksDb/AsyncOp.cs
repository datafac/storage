using System.Buffers;
using System.Threading.Tasks;

namespace DataFac.Storage.RocksDbStore;

internal readonly struct AsyncOp
{
    public readonly AsyncOpKind Kind;
    public readonly BlobIdV1 Id;
    public readonly ReadOnlySequence<byte> Data;
    public readonly TaskCompletionSource<ReadOnlySequence<byte>?>? Completion;

    private AsyncOp(AsyncOpKind kind, BlobIdV1 id, ReadOnlySequence<byte> data, TaskCompletionSource<ReadOnlySequence<byte>?>? completion)
    {
        Kind = kind;
        Id = id;
        Data = data;
        Completion = completion;
    }

    public static AsyncOp Sync(TaskCompletionSource<ReadOnlySequence<byte>?> completion) => new AsyncOp(AsyncOpKind.Sync, default, default, completion);
    public static AsyncOp Del(BlobIdV1 id, TaskCompletionSource<ReadOnlySequence<byte>?>? completion) => new AsyncOp(AsyncOpKind.Del, id, default, completion);
    public static AsyncOp Put(BlobIdV1 id, ReadOnlySequence<byte> data, TaskCompletionSource<ReadOnlySequence<byte>?>? completion) => new AsyncOp(AsyncOpKind.Put, id, data, completion);
    public static AsyncOp Get(BlobIdV1 id, TaskCompletionSource<ReadOnlySequence<byte>?> completion) => new AsyncOp(AsyncOpKind.Get, id, default, completion);
}
