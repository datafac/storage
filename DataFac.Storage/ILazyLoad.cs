using System.Threading.Tasks;

namespace DataFac.Storage;

public interface ILazyLoad
{
    ValueTask LazyLoad(int depth, IDataStore store);
}
