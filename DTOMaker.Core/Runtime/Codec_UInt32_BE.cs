using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_UInt32_BE : Codec_Base<UInt32>
#if NET7_0_OR_GREATER
    , ISpanCodec<UInt32>
#endif
    {
        private Codec_UInt32_BE() { }
        public static Codec_UInt32_BE Instance { get; } = new Codec_UInt32_BE();
        public override UInt32 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt32BigEndian(source);
        public override void OnWrite(Span<byte> target, in UInt32 input) => BinaryPrimitives.WriteUInt32BigEndian(target, input);
        public static UInt32 ReadFromSpan(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt32BigEndian(source);
        public static void WriteToSpan(Span<byte> target, in UInt32 input) => BinaryPrimitives.WriteUInt32BigEndian(target, input);
    }
}
