using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Int32_BE : Codec_Base<Int32>
    {
        private Codec_Int32_BE() { }
        public static Codec_Int32_BE Instance { get; } = new Codec_Int32_BE();
        public override Int32 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt32BigEndian(source);
        public override void OnWrite(Span<byte> target, in Int32 input) => BinaryPrimitives.WriteInt32BigEndian(target, input);
    }
    public sealed class Codec_Int32_LE : Codec_Base<Int32>
    {
        private Codec_Int32_LE() { }
        public static Codec_Int32_LE Instance { get; } = new Codec_Int32_LE();
        public override Int32 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt32LittleEndian(source);
        public override void OnWrite(Span<byte> target, in Int32 input) => BinaryPrimitives.WriteInt32LittleEndian(target, input);
    }
    public sealed class Codec_Int16_BE : Codec_Base<Int16>
    {
        private Codec_Int16_BE() { }
        public static Codec_Int16_BE Instance { get; } = new Codec_Int16_BE();
        public override Int16 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt16BigEndian(source);
        public override void OnWrite(Span<byte> target, in Int16 input) => BinaryPrimitives.WriteInt16BigEndian(target, input);
    }
    public sealed class Codec_Int16_LE : Codec_Base<Int16>
    {
        private Codec_Int16_LE() { }
        public static Codec_Int16_LE Instance { get; } = new Codec_Int16_LE();
        public override Int16 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt16LittleEndian(source);
        public override void OnWrite(Span<byte> target, in Int16 input) => BinaryPrimitives.WriteInt16LittleEndian(target, input);
    }
    public sealed class Codec_UInt32_BE : Codec_Base<UInt32>
    {
        private Codec_UInt32_BE() { }
        public static Codec_UInt32_BE Instance { get; } = new Codec_UInt32_BE();
        public override UInt32 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt32BigEndian(source);
        public override void OnWrite(Span<byte> target, in UInt32 input) => BinaryPrimitives.WriteUInt32BigEndian(target, input);
    }
    public sealed class Codec_UInt32_LE : Codec_Base<UInt32>
    {
        private Codec_UInt32_LE() { }
        public static Codec_UInt32_LE Instance { get; } = new Codec_UInt32_LE();
        public override UInt32 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt32LittleEndian(source);
        public override void OnWrite(Span<byte> target, in UInt32 input) => BinaryPrimitives.WriteUInt32LittleEndian(target, input);
    }
    public sealed class Codec_UInt16_BE : Codec_Base<UInt16>
    {
        private Codec_UInt16_BE() { }
        public static Codec_UInt16_BE Instance { get; } = new Codec_UInt16_BE();
        public override UInt16 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt16BigEndian(source);
        public override void OnWrite(Span<byte> target, in UInt16 input) => BinaryPrimitives.WriteUInt16BigEndian(target, input);
    }
    public sealed class Codec_UInt16_LE : Codec_Base<UInt16>
    {
        private Codec_UInt16_LE() { }
        public static Codec_UInt16_LE Instance { get; } = new Codec_UInt16_LE();
        public override UInt16 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt16LittleEndian(source);
        public override void OnWrite(Span<byte> target, in UInt16 input) => BinaryPrimitives.WriteUInt16LittleEndian(target, input);
    }
}
