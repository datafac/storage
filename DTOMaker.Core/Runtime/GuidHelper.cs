using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace DTOMaker.Runtime
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public readonly struct GuidHelper
    {
        [FieldOffset(0)]
        public readonly Guid GuidValue;

        [FieldOffset(0)] public readonly Int32 Part_A;
        [FieldOffset(4)] public readonly Int16 Part_B;
        [FieldOffset(6)] public readonly Int16 Part_C;
        [FieldOffset(8)] public readonly Byte Part_D;
        [FieldOffset(9)] public readonly Byte Part_E;
        [FieldOffset(10)] public readonly Byte Part_F;
        [FieldOffset(11)] public readonly Byte Part_G;
        [FieldOffset(12)] public readonly Byte Part_H;
        [FieldOffset(13)] public readonly Byte Part_I;
        [FieldOffset(14)] public readonly Byte Part_J;
        [FieldOffset(15)] public readonly Byte Part_K;

        public GuidHelper(Guid guidValue) : this()
        {
            GuidValue = guidValue;
        }

        private GuidHelper(Int32 part_A, Int16 part_B, Int16 part_C, byte part_D, byte part_E, byte part_F, byte part_G, byte part_H, byte part_I, byte part_J, byte part_K)
        {
            Part_A = part_A;
            Part_B = part_B;
            Part_C = part_C;
            Part_D = part_D;
            Part_E = part_E;
            Part_F = part_F;
            Part_G = part_G;
            Part_H = part_H;
            Part_I = part_I;
            Part_J = part_J;
            Part_K = part_K;
        }

        private GuidHelper(ReadOnlySpan<byte> source, bool bigEndian)
        {
            if (bigEndian)
            {
                Part_A = BinaryPrimitives.ReadInt32BigEndian(source.Slice(0));
                Part_B = BinaryPrimitives.ReadInt16BigEndian(source.Slice(4));
                Part_C = BinaryPrimitives.ReadInt16BigEndian(source.Slice(6));
            }
            else
            {
                Part_A = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(0));
                Part_B = BinaryPrimitives.ReadInt16LittleEndian(source.Slice(4));
                Part_C = BinaryPrimitives.ReadInt16LittleEndian(source.Slice(6));
            }
            Part_D = source[8];
            Part_E = source[9];
            Part_F = source[10];
            Part_G = source[11];
            Part_H = source[12];
            Part_I = source[13];
            Part_J = source[14];
            Part_K = source[15];
        }

        public override string ToString()
        {
            return $"{Part_A:X8}-{Part_B:X4}-{Part_C:X4}-{Part_D:X2}-{Part_E:X2}-{Part_F:X2}-{Part_G:X2}-{Part_H:X2}-{Part_I:X2}-{Part_J:X2}-{Part_K:X2}";
        }

        public static Guid ReadFromSpan(ReadOnlySpan<byte> source, bool bigEndian)
        {
            return new GuidHelper(source, bigEndian).GuidValue;
        }

        public static void WriteToSpan(Span<byte> target, bool bigEndian, Guid value)
        {
            new GuidHelper(value).WriteTo(target, bigEndian);
        }

        private void WriteTo(Span<byte> target, bool bigEndian)
        {
            if (bigEndian)
            {
                BinaryPrimitives.WriteInt32BigEndian(target.Slice(0), Part_A);
                BinaryPrimitives.WriteInt16BigEndian(target.Slice(4), Part_B);
                BinaryPrimitives.WriteInt16BigEndian(target.Slice(6), Part_C);
            }
            else
            {
                BinaryPrimitives.WriteInt32LittleEndian(target.Slice(0), Part_A);
                BinaryPrimitives.WriteInt16LittleEndian(target.Slice(4), Part_B);
                BinaryPrimitives.WriteInt16LittleEndian(target.Slice(6), Part_C);
            }
            target[8] = Part_D;
            target[9] = Part_E;
            target[10] = Part_F;
            target[11] = Part_G;
            target[12] = Part_H;
            target[13] = Part_I;
            target[14] = Part_J;
            target[15] = Part_K;
        }
    }
}
