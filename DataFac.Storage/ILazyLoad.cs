using System.Threading.Tasks;

namespace Inventory.Store;

public interface ILazyLoad
{
    ValueTask LazyLoad(int depth, IDataStore store);
}
