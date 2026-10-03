namespace DataFac.Storage.Tests;

/// <summary>
/// Represents the kind of store used in the tests.
/// </summary>
public enum StoreKind
{
    /// <summary>
    /// An in-memory test store
    /// </summary>
    Testing,

    /// <summary>
    /// A RocksDB store
    /// </summary>
    RocksDb,

    /// <summary>
    /// A local file system store
    /// </summary>
    LocalFS,
}
