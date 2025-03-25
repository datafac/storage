using System.IO;

namespace DTOMaker.Gentime
{
    public interface IEmitText
    {
        bool Emit(TextWriter writer, int indent);
    }
}