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
#if NET6_0_OR_GREATER
        double? Field2
        {
            get { return Field2_HasValue ? Field2_Value : null; }
            set
            {
                Field2_HasValue = value is not null;
                Field2_Value = value is null ? default : value.Value;
            }
        }
#endif

        // fixed byte arary
        [Member(3, arrayLength: 64)]
        ReadOnlyMemory<byte> Field3_Values { get; set; }

        [Member(4)]
        int Field3_Length { get; set; }

        // variable
#if NET6_0_OR_GREATER
        ReadOnlyMemory<byte>? Field3
        {
            get
            {
                var length = this.Field3_Length;
                return length switch
                {
                    < 0 => null,
                    0 => ReadOnlyMemory<byte>.Empty,
                    _ => this.Field3_Values.Slice(0, length)
                };
            }
            set
            {
                if (value is null)
                {
                    this.Field3_Length = -1;
                }
                else if (value.Value.Length == 0)
                {
                    this.Field3_Length = 0;
                }
                else
                {
                    var length = value.Value.Length;
                    this.Field3_Values = value.Value.Slice(0, length);
                    this.Field3_Length = length;
                }
            }
        }
#endif

        [Member(5)]
        ushort Enum16_Data { get; set; }

#if NET6_0_OR_GREATER
        Kind16 Enum16
        {
            get => (Kind16)this.Enum16_Data;
            set => this.Enum16_Data = (ushort)value;
        }
#endif

        [Member(6)]
        ReadOnlyMemory<Char> Field6 { get; set; }

        //string? Field6 { get; set; }
        //#if NET6_0_OR_GREATER
        //                return UTF8Encoding.UTF8.GetString(fullSlice.Span.Slice(0, count));
        //#else
        //                return Encoding.UTF8.GetString(fullSlice.ToArray(), 0, count);
        //#endif
        //#if NET6_0_OR_GREATER
        //        int bytesWritten = UTF8Encoding.UTF8.GetBytes(value.AsSpan(), fullSpan);
        //#else
        //                var encoded = Encoding.UTF8.GetBytes(value);
        //#endif
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
