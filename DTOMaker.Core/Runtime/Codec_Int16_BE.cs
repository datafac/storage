using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Int16_BE : Codec_Base<Int16>
#if NET7_0_OR_GREATER
    , ISpanCodec<Int16>
#endif
    {
        private Codec_Int16_BE() { }
        public static Codec_Int16_BE Instance { get; } = new Codec_Int16_BE();
        public override Int16 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt16BigEndian(source);
        public override void OnWrite(Span<byte> target, in Int16 input) => BinaryPrimitives.WriteInt16BigEndian(target, input);
        public static Int16 ReadFromSpan(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt16BigEndian(source);
        public static void WriteToSpan(Span<byte> target, in Int16 input) => BinaryPrimitives.WriteInt16BigEndian(target, input);
    }
}
