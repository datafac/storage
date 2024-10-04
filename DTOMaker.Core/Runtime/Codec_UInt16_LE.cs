using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_UInt16_LE : Codec_Base<UInt16>
#if NET7_0_OR_GREATER
    , IDataCodec<UInt16>
#endif
    {
        private Codec_UInt16_LE() { }
        public static Codec_UInt16_LE Instance { get; } = new Codec_UInt16_LE();
        public override UInt16 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt16LittleEndian(source);
        public override void OnWrite(Span<byte> target, in UInt16 input) => BinaryPrimitives.WriteUInt16LittleEndian(target, input);
#if NET7_0_OR_GREATER
        public static UInt16 ReadFromSpan(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt16LittleEndian(source);
        public static void WriteToSpan(Span<byte> target, in UInt16 input) => BinaryPrimitives.WriteUInt16LittleEndian(target, input);
#endif
    }
}
