using DataFac.Storage;
using System;
using System.Threading.Tasks;

namespace DTOMaker.Runtime.MemBlocks
{
    public interface IMemBlocksEntity
    {
        ValueTask Pack(IDataStore dataStore);
        ReadOnlyMemory<byte> GetBuffer();
        ValueTask Unpack(IDataStore dataStore, int depth = 0);
        ValueTask UnpackAll(IDataStore dataStore);
    }
}
