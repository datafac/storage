using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Guid_BE : Codec_Base<Guid>
#if NET7_0_OR_GREATER
    , ISpanCodec<Guid>
#endif
    {
        private Codec_Guid_BE() { }
        public static Codec_Guid_BE Instance { get; } = new Codec_Guid_BE();
        public override Guid OnRead(ReadOnlySpan<byte> source)
        {
#if NET8_0_OR_GREATER
            return new Guid(source, true);
#else
            return GuidHelper.ReadFromSpan(source, true);
#endif
        }

        public override void OnWrite(Span<byte> target, in Guid input)
        {
#if NET8_0_OR_GREATER
            input.TryWriteBytes(target, true, out int _);
#else
            GuidHelper.WriteToSpan(target, true, input);
#endif
        }
        public static Guid ReadFromSpan(ReadOnlySpan<byte> source)
        {
#if NET8_0_OR_GREATER
            return new Guid(source, true);
#else
            return GuidHelper.ReadFromSpan(source, true);
#endif
        }

        public static void WriteToSpan(Span<byte> target, in Guid input)
        {
#if NET8_0_OR_GREATER
            input.TryWriteBytes(target, true, out int _);
#else
            GuidHelper.WriteToSpan(target, true, input);
#endif
        }
    }
}
