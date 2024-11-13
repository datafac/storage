using DTOMaker.Models;
using System;
using System.Text;
namespace MyOrg.Models
{
    public enum Kind16 : ushort
    {
        Undefined,
        Kind1,
        MaxKind = ushort.MaxValue,
    }
    [Entity]
    [EntityLayout(LayoutMethod.SequentialV1)]
    public interface IMyDTO
    {
        [Member(1)] double Field2_Value { get; set; }
        [Member(2)] bool Field2_HasValue { get; set; }
        double? Field2 { get; set; }

        // fixed byte arary
        [Member(3)]
        [MemberLayout(arrayLength: 64)]
        ReadOnlyMemory<byte> Field3_Values { get; set; }

        [Member(4)]
        int Field3_Length { get; set; }

        // variable
        ReadOnlyMemory<byte>? Field3 { get; set; }

        [Member(5)]
        ushort Enum16_Data { get; set; }
        Kind16 Enum16 { get; set; }
    }

    public static class MyDTOExtensions
    {
        public static double? GetField2(this IMyDTO self)
        {
            return self.Field2_HasValue ? self.Field2_Value : null;
        }

        public static void SetField2(this IMyDTO self, double? value)
        {
            self.Field2_HasValue = value is not null;
            self.Field2_Value = value is null ? default : value.Value;
        }
    }
}
