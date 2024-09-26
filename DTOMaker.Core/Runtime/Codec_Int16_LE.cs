using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Int16_LE : Codec_Base<Int16>
    {
        private Codec_Int16_LE() { }
        public static Codec_Int16_LE Instance { get; } = new Codec_Int16_LE();
        public override Int16 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt16LittleEndian(source);
        public override void OnWrite(Span<byte> target, in Int16 input) => BinaryPrimitives.WriteInt16LittleEndian(target, input);
    }
}
