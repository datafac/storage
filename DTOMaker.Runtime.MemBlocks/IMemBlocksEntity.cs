using DataFac.Storage;
using System;
using System.Threading.Tasks;

namespace DTOMaker.Runtime.MemBlocks
{
    public interface IMemBlocksEntity
    {
        ValueTask Pack(IDataStore dataStore);
        ReadOnlyMemory<byte> GetBuffer();
        void LoadBuffer(ReadOnlyMemory<byte> buffer);
        ValueTask Unpack(IDataStore dataStore);
    }
}
