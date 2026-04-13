using DataFac.Compression;
using System;
using System.Threading.Tasks;

namespace DataFac.Storage.RocksDbStore;

internal readonly struct AsyncOp
{
    public readonly AsyncOpKind Kind;
    public readonly BlobIdV1 Id;
    public readonly ReadOnlyMemory<byte> Data;
    public readonly TaskCompletionSource<BlobResult>? Completion;

    private AsyncOp(AsyncOpKind kind, BlobIdV1 id, ReadOnlyMemory<byte> data, TaskCompletionSource<BlobResult>? completion)
    {
        Kind = kind;
        Id = id;
        Data = data;
        Completion = completion;
    }

    public static AsyncOp Sync(TaskCompletionSource<BlobResult> completion) => new AsyncOp(AsyncOpKind.Sync, default, default, completion);
    public static AsyncOp Del(BlobIdV1 id, TaskCompletionSource<BlobResult>? completion) => new AsyncOp(AsyncOpKind.Del, id, default, completion);
    public static AsyncOp Put(BlobIdV1 id, ReadOnlyMemory<byte> data, TaskCompletionSource<BlobResult>? completion) => new AsyncOp(AsyncOpKind.Put, id, data, completion);
    public static AsyncOp Get(BlobIdV1 id, TaskCompletionSource<BlobResult> completion) => new AsyncOp(AsyncOpKind.Get, id, default, completion);
}
