using System;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Boolean_BE : Codec_Base<Boolean>
    {
        private Codec_Boolean_BE() { }
        public static Codec_Boolean_BE Instance { get; } = new Codec_Boolean_BE();
        public override Boolean OnRead(ReadOnlySpan<byte> source) => source[0] != (byte)0;
        public override void OnWrite(Span<byte> target, in Boolean input) => target[0] = input ? (byte)1 : (byte)0;
    }
    public sealed class Codec_SByte_BE : Codec_Base<SByte>
    {
        private Codec_SByte_BE() { }
        public static Codec_SByte_BE Instance { get; } = new Codec_SByte_BE();
        public override SByte OnRead(ReadOnlySpan<byte> source) => (SByte)source[0];
        public override void OnWrite(Span<byte> target, in SByte input) => target[0] = (byte)input;
    }
    public sealed class Codec_SByte_LE : Codec_Base<SByte>
    {
        private Codec_SByte_LE() { }
        public static Codec_SByte_LE Instance { get; } = new Codec_SByte_LE();
        public override SByte OnRead(ReadOnlySpan<byte> source) => (SByte)source[0];
        public override void OnWrite(Span<byte> target, in SByte input) => target[0] = (byte)input;
    }
    public sealed class Codec_Byte_BE : Codec_Base<Byte>
    {
        private Codec_Byte_BE() { }
        public static Codec_Byte_BE Instance { get; } = new Codec_Byte_BE();
        public override Byte OnRead(ReadOnlySpan<byte> source) => (Byte)source[0];
        public override void OnWrite(Span<byte> target, in Byte input) => target[0] = (byte)input;
    }
    public sealed class Codec_Byte_LE : Codec_Base<Byte>
    {
        private Codec_Byte_LE() { }
        public static Codec_Byte_LE Instance { get; } = new Codec_Byte_LE();
        public override Byte OnRead(ReadOnlySpan<byte> source) => (Byte)source[0];
        public override void OnWrite(Span<byte> target, in Byte input) => target[0] = (byte)input;
    }
}
