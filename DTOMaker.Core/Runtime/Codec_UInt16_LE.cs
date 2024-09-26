using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_UInt16_LE : Codec_Base<UInt16>
    {
        private Codec_UInt16_LE() { }
        public static Codec_UInt16_LE Instance { get; } = new Codec_UInt16_LE();
        public override UInt16 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt16LittleEndian(source);
        public override void OnWrite(Span<byte> target, in UInt16 input) => BinaryPrimitives.WriteUInt16LittleEndian(target, input);
    }
}
