using System.IO;

namespace DynaText
{
    // todo we need a better name for these
    public interface IEmitText
    {
        bool Emit(TextWriter writer, int indent);
    }
    public interface ILoadText
    {
        void LoadFrom(string text);
    }
    public interface IDynaText
    {
        DynaTextMap GetMap();
        void LoadFrom(DynaTextMap map);
    }
}