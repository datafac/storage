using System;

namespace DTOMaker.Runtime
{
    public sealed class Codec_SByte_LE : Codec_Base<SByte>
#if NET7_0_OR_GREATER
    , ISpanCodec<SByte>
#endif
    {
        private Codec_SByte_LE() { }
        public static Codec_SByte_LE Instance { get; } = new Codec_SByte_LE();
        public override SByte OnRead(ReadOnlySpan<byte> source) => (SByte)source[0];
        public override void OnWrite(Span<byte> target, in SByte input) => target[0] = (byte)input;
        public static SByte ReadFromSpan(ReadOnlySpan<byte> source) => (SByte)source[0];
        public static void WriteToSpan(Span<byte> target, in SByte input) => target[0] = (byte)input;
    }
}
