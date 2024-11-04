using System;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Flags : Codec_Base<Flags>
#if NET7_0_OR_GREATER
    , ISpanCodec<Flags>
#endif
    {
        private Codec_Flags() { }
        public static Codec_Flags Instance { get; } = new Codec_Flags();
        public override Flags OnRead(ReadOnlySpan<byte> source) => new Flags(source[0]);
        public override void OnWrite(Span<byte> target, in Flags input) => target[0] = input.AsByte();
        public static Flags ReadFromSpan(ReadOnlySpan<byte> source) => new Flags(source[0]);
        public static void WriteToSpan(Span<byte> target, in Flags input) => target[0] = input.AsByte();
    }
}
