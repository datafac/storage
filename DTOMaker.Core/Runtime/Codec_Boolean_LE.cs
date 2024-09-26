using System;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Boolean_LE : Codec_Base<Boolean>
    {
        private Codec_Boolean_LE() { }
        public static Codec_Boolean_LE Instance { get; } = new Codec_Boolean_LE();
        public override Boolean OnRead(ReadOnlySpan<byte> source) => source[0] != (byte)0;
        public override void OnWrite(Span<byte> target, in Boolean input) => target[0] = input ? (byte)1 : (byte)0;
    }
}
