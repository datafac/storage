using System;

namespace DTOMaker.Models
{
    /// <summary>
    /// Defines the memory offsets of the fields holding the property
    /// value. FieldOffset is the offset of the data value. FlagsOffset is
    /// the offset of a flags byte. IsBigEndian defines the endianness (big 
    /// or little) of the data value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MemberLayoutAttribute : Attribute
    {
        public readonly int FieldOffset;
        public readonly int FlagsOffset;
        public readonly bool IsBigEndian;

        public MemberLayoutAttribute(int fieldOffset, int flagsOffset, bool isBigEndian = false)
        {
            FieldOffset = fieldOffset;
            FlagsOffset = flagsOffset;
            IsBigEndian = isBigEndian;
        }
    }
}
