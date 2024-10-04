using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Int16_LE : Codec_Base<Int16>
#if NET7_0_OR_GREATER
    , IDataCodec<Int16>
#endif
    {
        private Codec_Int16_LE() { }
        public static Codec_Int16_LE Instance { get; } = new Codec_Int16_LE();
        public override Int16 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt16LittleEndian(source);
        public override void OnWrite(Span<byte> target, in Int16 input) => BinaryPrimitives.WriteInt16LittleEndian(target, input);
#if NET7_0_OR_GREATER
        public static Int16 ReadFromSpan(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt16LittleEndian(source);
        public static void WriteToSpan(Span<byte> target, in Int16 input) => BinaryPrimitives.WriteInt16LittleEndian(target, input);
#endif
    }
}
