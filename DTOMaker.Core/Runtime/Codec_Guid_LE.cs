using System;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Guid_LE : Codec_Base<Guid>
#if NET7_0_OR_GREATER
    , ISpanCodec<Guid>
#endif
    {
        private Codec_Guid_LE() { }
        public static Codec_Guid_LE Instance { get; } = new Codec_Guid_LE();
        public override Guid OnRead(ReadOnlySpan<byte> source)
        {
#if NET8_0_OR_GREATER
            return new Guid(source, false);
#else
            return GuidHelper.ReadFromSpan(source, false);
#endif
        }

        public override void OnWrite(Span<byte> target, in Guid input)
        {
#if NET8_0_OR_GREATER
            input.TryWriteBytes(target, false, out int _);
#else
            GuidHelper.WriteToSpan(target, false, input);
#endif
        }
        public static Guid ReadFromSpan(ReadOnlySpan<byte> source)
        {
#if NET8_0_OR_GREATER
            return new Guid(source, false);
#else
            return GuidHelper.ReadFromSpan(source, false);
#endif
        }

        public static void WriteToSpan(Span<byte> target, in Guid input)
        {
#if NET8_0_OR_GREATER
            input.TryWriteBytes(target, false, out int _);
#else
            GuidHelper.WriteToSpan(target, false, input);
#endif
        }
    }
}
