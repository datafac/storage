namespace DataFac.Storage.RocksDbStore;

internal enum AsyncOpKind
{
    Sync,
    Get,
    Put,
    Del,
}
