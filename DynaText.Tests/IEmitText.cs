using System.IO;

namespace DTOMaker.Gentime.Tests
{
    public interface IEmitText
    {
        bool Emit(TextWriter writer, int indent);
    }
}