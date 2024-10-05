using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
#if NET8_0_OR_GREATER
    public sealed class Codec_UInt128_LE : ISpanCodec<UInt128>
    {
        public static UInt128 ReadFromSpan(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt128LittleEndian(source);
        public static void WriteToSpan(Span<byte> target, in UInt128 input) => BinaryPrimitives.WriteUInt128LittleEndian(target, input);
    }
#endif

}
