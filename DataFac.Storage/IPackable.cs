using System.Threading.Tasks;

namespace DataFac.Storage;

public interface IPackable
{
    ValueTask Pack(IDataStore dataStore);
    ValueTask Unpack(IDataStore dataStore, int depth = 0);
    ValueTask UnpackAll(IDataStore dataStore);
}
