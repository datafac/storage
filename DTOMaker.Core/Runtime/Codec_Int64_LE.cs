using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Int64_LE : Codec_Base<Int64>
    {
        private Codec_Int64_LE() { }
        public static Codec_Int64_LE Instance { get; } = new Codec_Int64_LE();
        public override Int64 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt64LittleEndian(source);
        public override void OnWrite(Span<byte> target, in Int64 input) => BinaryPrimitives.WriteInt64LittleEndian(target, input);
    }
}
