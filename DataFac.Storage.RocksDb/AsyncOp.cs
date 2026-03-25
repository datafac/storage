using DataFac.Memory;
using System.Buffers;
using System.Threading.Tasks;

namespace DataFac.Storage.RocksDbStore;

internal readonly struct AsyncOp
{
    public readonly AsyncOpKind Kind;
    public readonly BlobIdV1 Id;
    public readonly Octets? Data;
    public readonly TaskCompletionSource<Octets?>? Completion;

    private AsyncOp(AsyncOpKind kind, BlobIdV1 id, Octets? data, TaskCompletionSource<Octets?>? completion)
    {
        Kind = kind;
        Id = id;
        Data = data;
        Completion = completion;
    }

    public static AsyncOp Sync(TaskCompletionSource<Octets?> completion) => new AsyncOp(AsyncOpKind.Sync, default, default, completion);
    public static AsyncOp Del(BlobIdV1 id, TaskCompletionSource<Octets?>? completion) => new AsyncOp(AsyncOpKind.Del, id, default, completion);
    public static AsyncOp Put(BlobIdV1 id, Octets data, TaskCompletionSource<Octets?>? completion) => new AsyncOp(AsyncOpKind.Put, id, data, completion);
    public static AsyncOp Get(BlobIdV1 id, TaskCompletionSource<Octets?> completion) => new AsyncOp(AsyncOpKind.Get, id, default, completion);
}
