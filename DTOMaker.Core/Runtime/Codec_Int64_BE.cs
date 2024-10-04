using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Int64_BE : Codec_Base<Int64>
#if NET7_0_OR_GREATER
    , ISpanCodec<Int64>
#endif
    {
        private Codec_Int64_BE() { }
        public static Codec_Int64_BE Instance { get; } = new Codec_Int64_BE();
        public override Int64 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt64BigEndian(source);
        public override void OnWrite(Span<byte> target, in Int64 input) => BinaryPrimitives.WriteInt64BigEndian(target, input);
        public static Int64 ReadFromSpan(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt64BigEndian(source);
        public static void WriteToSpan(Span<byte> target, in Int64 input) => BinaryPrimitives.WriteInt64BigEndian(target, input);
    }
}
