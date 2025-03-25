using System.IO;

namespace DTOMaker.Gentime
{
    public interface IDynaText : IEmitText, ILoadText
    {
        DynaMap GetMap();
        void LoadFrom(DynaMap map);
    }
}