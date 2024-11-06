using System;

namespace DTOMaker.Models
{
    /// <summary>
    /// Defines the memory offset within the memory block of the underlying field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MemberOffsetAttribute : Attribute
    {
        public readonly int FieldOffset;

        public MemberOffsetAttribute(int fieldOffset)
        {
            FieldOffset = fieldOffset;
        }
    }
}
