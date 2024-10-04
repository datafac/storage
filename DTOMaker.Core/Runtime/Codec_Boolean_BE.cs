using System;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Boolean_BE : Codec_Base<Boolean>
#if NET7_0_OR_GREATER
    , IDataCodec<Boolean>
#endif
    {
        private Codec_Boolean_BE() { }
        public static Codec_Boolean_BE Instance { get; } = new Codec_Boolean_BE();
        public override Boolean OnRead(ReadOnlySpan<byte> source) => source[0] != (byte)0;
        public override void OnWrite(Span<byte> target, in Boolean input) => target[0] = input ? (byte)1 : (byte)0;
#if NET7_0_OR_GREATER
        public static Boolean ReadFromSpan(ReadOnlySpan<byte> source) => source[0] != (byte)0;
        public static void WriteToSpan(Span<byte> target, in Boolean input) => target[0] = input ? (byte)1 : (byte)0;
#endif
    }
}
