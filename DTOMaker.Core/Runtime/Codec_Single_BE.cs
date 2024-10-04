using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Single_BE : Codec_Base<Single>
#if NET7_0_OR_GREATER
    , ISpanCodec<Single>
#endif
    {
        private Codec_Single_BE() { }
        public static Codec_Single_BE Instance { get; } = new Codec_Single_BE();
        public override Single OnRead(ReadOnlySpan<byte> source)
        {
#if NET6_0_OR_GREATER
            return BinaryPrimitives.ReadSingleBigEndian(source);
#else
            Int32 value = BinaryPrimitives.ReadInt32BigEndian(source);
            var overlap = new Convert32(value);
            return overlap.FloatValue;
#endif
        }

        public override void OnWrite(Span<byte> target, in Single input)
        {
#if NET6_0_OR_GREATER
            BinaryPrimitives.WriteSingleBigEndian(target, input);
#else
            var overlap = new Convert32(input);
            int value = overlap.Int32Value;
            BinaryPrimitives.WriteInt32BigEndian(target, value);
#endif
        }

        public static Single ReadFromSpan(ReadOnlySpan<byte> source)
        {
#if NET6_0_OR_GREATER
            return BinaryPrimitives.ReadSingleBigEndian(source);
#else
            Int32 value = BinaryPrimitives.ReadInt32BigEndian(source);
            var overlap = new Convert32(value);
            return overlap.FloatValue;
#endif
        }

        public static void WriteToSpan(Span<byte> target, in Single input)
        {
#if NET6_0_OR_GREATER
            BinaryPrimitives.WriteSingleBigEndian(target, input);
#else
            var overlap = new Convert32(input);
            int value = overlap.Int32Value;
            BinaryPrimitives.WriteInt32BigEndian(target, value);
#endif
        }
    }
}
