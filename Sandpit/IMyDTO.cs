using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DTOMaker.Models;
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
        [Member(1)]
        IList<Int16?>? Field1 { get; set; }

        [Member(2)] double Field2_Value { get; set; }
        [Member(3)] bool Field2_HasValue { get; set; }
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

        // Octets
        [Member(4)]
        ReadOnlyMemory<byte> Field4 { get; set; }

        [Member(5)]
        ReadOnlyMemory<byte>? Field5 { get; set; }

        [Member(6)]
        string? Field6 { get; set; }
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
