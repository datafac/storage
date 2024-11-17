using System;

namespace DTOMaker.Models
{
    /// <summary>
    /// Defines the memory offset within the memory block of the underlying field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MemberLayoutAttribute : Attribute
    {
        public readonly int FieldOffset;
        public readonly bool IsBigEndian;
        public readonly int ArrayLength;

        public MemberLayoutAttribute(int fieldOffset = 0, bool isBigEndian = false, int arrayLength = 0)
        {
            FieldOffset = fieldOffset;
            IsBigEndian = isBigEndian;
            ArrayLength = arrayLength;
        }
    }
}
