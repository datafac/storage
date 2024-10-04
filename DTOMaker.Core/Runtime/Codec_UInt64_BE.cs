using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_UInt64_BE : Codec_Base<UInt64>
#if NET7_0_OR_GREATER
    , ISpanCodec<UInt64>
#endif
    {
        private Codec_UInt64_BE() { }
        public static Codec_UInt64_BE Instance { get; } = new Codec_UInt64_BE();
        public override UInt64 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt64BigEndian(source);
        public override void OnWrite(Span<byte> target, in UInt64 input) => BinaryPrimitives.WriteUInt64BigEndian(target, input);
        public static UInt64 ReadFromSpan(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt64BigEndian(source);
        public static void WriteToSpan(Span<byte> target, in UInt64 input) => BinaryPrimitives.WriteUInt64BigEndian(target, input);
    }
}
