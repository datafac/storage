using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Int32_LE : Codec_Base<Int32>
    {
        private Codec_Int32_LE() { }
        public static Codec_Int32_LE Instance { get; } = new Codec_Int32_LE();
        public override Int32 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt32LittleEndian(source);
        public override void OnWrite(Span<byte> target, in Int32 input) => BinaryPrimitives.WriteInt32LittleEndian(target, input);
    }
}
