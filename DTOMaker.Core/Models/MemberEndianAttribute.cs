using System;

namespace DTOMaker.Models
{
    /// <summary>
    /// Defines the endian-ness of the underlying field. This is only meaningful
    /// for multi-byte fields.
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
}
