using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
#if NET6_0_OR_GREATER
    public sealed class Codec_Half_BE : Codec_Base<Half>
#if NET7_0_OR_GREATER
    , ISpanCodec<Half>
#endif
    {
        private Codec_Half_BE() { }
        public static Codec_Half_BE Instance { get; } = new Codec_Half_BE();
        public override Half OnRead(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadHalfBigEndian(source);
        }

        public override void OnWrite(Span<byte> target, in Half input)
        {
            BinaryPrimitives.WriteHalfBigEndian(target, input);
        }
        public static Half ReadFromSpan(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadHalfBigEndian(source);
        }

        public static void WriteToSpan(Span<byte> target, in Half input)
        {
            BinaryPrimitives.WriteHalfBigEndian(target, input);
        }
    }
#endif
}
