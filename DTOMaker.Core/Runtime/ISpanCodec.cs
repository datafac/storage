using System;

namespace DTOMaker.Runtime
{
#if NET7_0_OR_GREATER
    public interface ISpanCodec<TField>
    {
        static abstract TField ReadFromSpan(ReadOnlySpan<byte> source);
        static abstract void WriteToSpan(Span<byte> target, in TField input);
    }
#endif
}
