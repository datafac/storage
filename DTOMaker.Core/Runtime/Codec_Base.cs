using System;

namespace DTOMaker.Runtime
{
    public abstract class Codec_Base<TField> : ITypedFieldCodec<TField>
        where TField : struct
    {
        public abstract TField OnRead(ReadOnlySpan<byte> source);
        public abstract void OnWrite(Span<byte> target, in TField input);

        public TField ReadFrom(ReadOnlySpan<byte> source)
        {
            return OnRead(source);
        }

        public void WriteTo(Span<byte> target, in TField input)
        {
            OnWrite(target, in input);
        }

        object? IFieldCodec.ReadObject(ReadOnlySpan<byte> source)
        {
            return OnRead(source);
        }

        void IFieldCodec.WriteObject(Span<byte> target, object? input)
        {
            OnWrite(target, input is TField value ? value : default);
        }

    }
}
