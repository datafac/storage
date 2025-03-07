using DataFac.Storage;
using System.Threading.Tasks;

namespace DTOMaker.Runtime.MemBlocks
{
    public interface IPackable
    {
        ValueTask Pack(IDataStore dataStore);
    }
}
