using System;

namespace DTOMaker.Core.Codecs
{
    public interface IFieldCodec
    {
        object? ReadObject(ReadOnlySpan<byte> source);
        void WriteObject(Span<byte> target, object? input);
    }
}
