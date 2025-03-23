using System.IO;

namespace DynaText
{
    internal interface IEmitText
    {
        bool Emit(TextWriter writer, int indent);
    }
}