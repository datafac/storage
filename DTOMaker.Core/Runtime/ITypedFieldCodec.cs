using System;

namespace DTOMaker.Runtime
{
    public interface ITypedFieldCodec<TField> : IFieldCodec
    {
        TField ReadFrom(ReadOnlySpan<byte> source);
        void WriteTo(Span<byte> target, in TField input);
    }
}
