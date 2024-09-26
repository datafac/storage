using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Int64_BE : Codec_Base<Int64>
    {
        private Codec_Int64_BE() { }
        public static Codec_Int64_BE Instance { get; } = new Codec_Int64_BE();
        public override Int64 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt64BigEndian(source);
        public override void OnWrite(Span<byte> target, in Int64 input) => BinaryPrimitives.WriteInt64BigEndian(target, input);
    }
    public sealed class Codec_Int64_LE : Codec_Base<Int64>
    {
        private Codec_Int64_LE() { }
        public static Codec_Int64_LE Instance { get; } = new Codec_Int64_LE();
        public override Int64 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt64LittleEndian(source);
        public override void OnWrite(Span<byte> target, in Int64 input) => BinaryPrimitives.WriteInt64LittleEndian(target, input);
    }
    public sealed class Codec_UInt64_BE : Codec_Base<UInt64>
    {
        private Codec_UInt64_BE() { }
        public static Codec_UInt64_BE Instance { get; } = new Codec_UInt64_BE();
        public override UInt64 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt64BigEndian(source);
        public override void OnWrite(Span<byte> target, in UInt64 input) => BinaryPrimitives.WriteUInt64BigEndian(target, input);
    }
    public sealed class Codec_UInt64_LE : Codec_Base<UInt64>
    {
        private Codec_UInt64_LE() { }
        public static Codec_UInt64_LE Instance { get; } = new Codec_UInt64_LE();
        public override UInt64 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt64LittleEndian(source);
        public override void OnWrite(Span<byte> target, in UInt64 input) => BinaryPrimitives.WriteUInt64LittleEndian(target, input);
    }
}
