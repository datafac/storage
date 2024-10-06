using System;
using System.Buffers.Binary;

namespace DTOMaker.Runtime
{
    public sealed class Codec_Decimal_BE : Codec_Base<Decimal>
#if NET7_0_OR_GREATER
    , ISpanCodec<Decimal>
#endif
    {
        private Codec_Decimal_BE() { }
        public static Codec_Decimal_BE Instance { get; } = new Codec_Decimal_BE();
        public override Decimal OnRead(ReadOnlySpan<byte> source)
        {
#if NET6_0_OR_GREATER
            Span<int> data = stackalloc int[4];
            data[0] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(0));
            data[1] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(4));
            data[2] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(8));
            data[3] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(12));
            return new Decimal(data);
#else
            int[] data = new int[4];
            data[0] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(0));
            data[1] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(4));
            data[2] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(8));
            data[3] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(12));
            return new Decimal(data);
#endif
        }

        public override void OnWrite(Span<byte> target, in Decimal input)
        {
#if NET6_0_OR_GREATER
            Span<int> data = stackalloc int[4];
            Decimal.TryGetBits(input, data, out int _);
#else
            int[] data = Decimal.GetBits(input);
#endif
            BinaryPrimitives.WriteInt32BigEndian(target.Slice(0), data[0]);
            BinaryPrimitives.WriteInt32BigEndian(target.Slice(4), data[1]);
            BinaryPrimitives.WriteInt32BigEndian(target.Slice(8), data[2]);
            BinaryPrimitives.WriteInt32BigEndian(target.Slice(12), data[3]);
        }

        public static Decimal ReadFromSpan(ReadOnlySpan<byte> source)
        {
#if NET6_0_OR_GREATER
            Span<int> data = stackalloc int[4];
            data[0] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(0));
            data[1] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(4));
            data[2] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(8));
            data[3] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(12));
            return new Decimal(data);
#else
            int[] data = new int[4];
            data[0] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(0));
            data[1] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(4));
            data[2] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(8));
            data[3] = BinaryPrimitives.ReadInt32BigEndian(source.Slice(12));
            return new Decimal(data);
#endif
        }

        public static void WriteToSpan(Span<byte> target, in Decimal input)
        {
#if NET6_0_OR_GREATER
            Span<int> data = stackalloc int[4];
            Decimal.TryGetBits(input, data, out int _);
#else
            int[] data = Decimal.GetBits(input);
#endif
            BinaryPrimitives.WriteInt32BigEndian(target.Slice(0), data[0]);
            BinaryPrimitives.WriteInt32BigEndian(target.Slice(4), data[1]);
            BinaryPrimitives.WriteInt32BigEndian(target.Slice(8), data[2]);
            BinaryPrimitives.WriteInt32BigEndian(target.Slice(12), data[3]);
        }
    }
    public sealed class Codec_Decimal_LE : Codec_Base<Decimal>
#if NET7_0_OR_GREATER
    , ISpanCodec<Decimal>
#endif
    {
        private Codec_Decimal_LE() { }
        public static Codec_Decimal_LE Instance { get; } = new Codec_Decimal_LE();
        public override Decimal OnRead(ReadOnlySpan<byte> source)
        {
#if NET6_0_OR_GREATER
            Span<int> data = stackalloc int[4];
            data[0] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(0));
            data[1] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(4));
            data[2] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(8));
            data[3] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(12));
            return new Decimal(data);
#else
            int[] data = new int[4];
            data[0] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(0));
            data[1] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(4));
            data[2] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(8));
            data[3] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(12));
            return new Decimal(data);
#endif
        }

        public override void OnWrite(Span<byte> target, in Decimal input)
        {
#if NET6_0_OR_GREATER
            Span<int> data = stackalloc int[4];
            Decimal.TryGetBits(input, data, out int _);
#else
            int[] data = Decimal.GetBits(input);
#endif
            BinaryPrimitives.WriteInt32LittleEndian(target.Slice(0), data[0]);
            BinaryPrimitives.WriteInt32LittleEndian(target.Slice(4), data[1]);
            BinaryPrimitives.WriteInt32LittleEndian(target.Slice(8), data[2]);
            BinaryPrimitives.WriteInt32LittleEndian(target.Slice(12), data[3]);
        }

        public static Decimal ReadFromSpan(ReadOnlySpan<byte> source)
        {
#if NET6_0_OR_GREATER
            Span<int> data = stackalloc int[4];
            data[0] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(0));
            data[1] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(4));
            data[2] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(8));
            data[3] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(12));
            return new Decimal(data);
#else
            int[] data = new int[4];
            data[0] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(0));
            data[1] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(4));
            data[2] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(8));
            data[3] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(12));
            return new Decimal(data);
#endif
        }

        public static void WriteToSpan(Span<byte> target, in Decimal input)
        {
#if NET6_0_OR_GREATER
            Span<int> data = stackalloc int[4];
            Decimal.TryGetBits(input, data, out int _);
#else
            int[] data = Decimal.GetBits(input);
#endif
            BinaryPrimitives.WriteInt32LittleEndian(target.Slice(0), data[0]);
            BinaryPrimitives.WriteInt32LittleEndian(target.Slice(4), data[1]);
            BinaryPrimitives.WriteInt32LittleEndian(target.Slice(8), data[2]);
            BinaryPrimitives.WriteInt32LittleEndian(target.Slice(12), data[3]);
        }
    }
}
