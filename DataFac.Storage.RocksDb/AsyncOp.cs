using DataFac.Compression;
using System;
using System.Threading.Tasks;

namespace DataFac.Storage.RocksDbStore;

internal readonly struct AsyncOp
{
    public readonly AsyncOpKind Kind;
    public readonly BlobKey Key;
    public readonly BlobData Data;
    public readonly TaskCompletionSource<BlobData>? Completion;

    private AsyncOp(AsyncOpKind kind, BlobKey key, BlobData data, TaskCompletionSource<BlobData>? completion)
    {
        Kind = kind;
        Key = key;
        Data = data;
        Completion = completion;
    }

    public static AsyncOp Sync(TaskCompletionSource<BlobData> completion) => new AsyncOp(AsyncOpKind.Sync, default, default, completion);
    public static AsyncOp Del(BlobKey key, TaskCompletionSource<BlobData>? completion) => new AsyncOp(AsyncOpKind.Del, key, default, completion);
    public static AsyncOp Put(BlobKey key, BlobData data, TaskCompletionSource<BlobData>? completion) => new AsyncOp(AsyncOpKind.Put, key, data, completion);
    public static AsyncOp Get(BlobKey key, TaskCompletionSource<BlobData> completion) => new AsyncOp(AsyncOpKind.Get, key, default, completion);
}
