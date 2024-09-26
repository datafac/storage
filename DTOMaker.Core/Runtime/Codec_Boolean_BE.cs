using System;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Boolean_BE : Codec_Base<Boolean>
    {
        private Codec_Boolean_BE() { }
        public static Codec_Boolean_BE Instance { get; } = new Codec_Boolean_BE();
        public override Boolean OnRead(ReadOnlySpan<byte> source) => source[0] != (byte)0;
        public override void OnWrite(Span<byte> target, in Boolean input) => target[0] = input ? (byte)1 : (byte)0;
    }
}
