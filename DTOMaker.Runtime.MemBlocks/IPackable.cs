using DataFac.Storage;
using System.Threading.Tasks;

namespace DTOMaker.Runtime.MemBlocks
{
    public interface IPackable
    {
        ValueTask Pack(IDataStore dataStore);
        ValueTask Unpack(IDataStore dataStore, int depth = 0);
        ValueTask UnpackAll(IDataStore dataStore);

    }
}
