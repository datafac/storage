using System;

namespace DTOMaker.Models
{
    /// <summary>
    /// Defines the memory offsets of the fields holding the property
    /// value. FieldOffset is the offset of the data value. IsBigEndian
    /// defines the endianness (big or little) of the data value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MemberLayoutAttribute : Attribute
    {
        public readonly int FieldOffset;
        public readonly bool IsBigEndian;

        public MemberLayoutAttribute(int fieldOffset, bool isBigEndian = false)
        {
            FieldOffset = fieldOffset;
            IsBigEndian = isBigEndian;
        }
    }
}
