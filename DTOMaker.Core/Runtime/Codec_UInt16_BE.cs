using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_UInt16_BE : Codec_Base<UInt16>
    {
        private Codec_UInt16_BE() { }
        public static Codec_UInt16_BE Instance { get; } = new Codec_UInt16_BE();
        public override UInt16 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt16BigEndian(source);
        public override void OnWrite(Span<byte> target, in UInt16 input) => BinaryPrimitives.WriteUInt16BigEndian(target, input);
    }
}
