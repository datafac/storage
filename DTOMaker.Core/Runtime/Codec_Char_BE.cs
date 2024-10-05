using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Char_BE : Codec_Base<Char>
#if NET7_0_OR_GREATER
    , ISpanCodec<Char>
#endif
    {
        private Codec_Char_BE() { }
        public static Codec_Char_BE Instance { get; } = new Codec_Char_BE();
        public override Char OnRead(ReadOnlySpan<byte> source) => (Char)BinaryPrimitives.ReadUInt16BigEndian(source);
        public override void OnWrite(Span<byte> target, in Char input) => BinaryPrimitives.WriteUInt16BigEndian(target, (UInt16)input);
        public static Char ReadFromSpan(ReadOnlySpan<byte> source) => (Char)BinaryPrimitives.ReadUInt16BigEndian(source);
        public static void WriteToSpan(Span<byte> target, in Char input) => BinaryPrimitives.WriteUInt16BigEndian(target, (UInt16)input);
    }
}
