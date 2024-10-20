using System;

namespace DTOMaker.Models
{
    /// <summary>
    /// Defines the endianness (big or little) of the property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MemberEndianAttribute : Attribute
    {
        public readonly bool IsBigEndian;

        public MemberEndianAttribute(bool isBigEndian)
        {
            IsBigEndian = isBigEndian;
        }

    }

    /// <summary>
    /// Defines the memory offsets of the fields holding the property
    /// value. FieldOffset is the offset of the data value. FlagsOffset is
    /// the offset of a flags byte.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MemberLayoutAttribute : Attribute
    {
        public readonly int FieldOffset;
        public readonly int FlagsOffset;

        public MemberLayoutAttribute(int fieldOffset, int flagsOffset)
        {
            FieldOffset = fieldOffset;
            FlagsOffset = flagsOffset;
        }
    }
}
