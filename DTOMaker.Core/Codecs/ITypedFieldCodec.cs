using System;

namespace DTOMaker.Core.Codecs
{
    public interface ITypedFieldCodec<TField> : IFieldCodec
    {
        TField ReadFrom(ReadOnlySpan<byte> source);
        void WriteTo(Span<byte> target, in TField input);
    }
}
