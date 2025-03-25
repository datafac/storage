using System.IO;

namespace DTOMaker.Gentime
{
    public interface IEmitText
    {
        bool Emit(TextWriter writer, int indent);
    }
    public interface ILoadText
    {
        void LoadFrom(string text);
    }
    public interface IDynaText : IEmitText, ILoadText
    {
        DynaTextMap GetMap();
        void LoadFrom(DynaTextMap map);
    }
}