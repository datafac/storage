using DataFac.Storage;
using System.Buffers;
using System.Threading.Tasks;

namespace DTOMaker.Runtime.MemBlocks
{
    public interface IMemBlocksEntity
    {
        ValueTask Pack(IDataStore dataStore);
        ReadOnlySequence<byte> GetBuffers();
        ValueTask Unpack(IDataStore dataStore, int depth = 0);
        ValueTask UnpackAll(IDataStore dataStore);
    }
}
