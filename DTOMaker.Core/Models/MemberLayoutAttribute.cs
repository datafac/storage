using System;

namespace DTOMaker.Models
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MemberLayoutAttribute : Attribute
    {
        public readonly int BlockOffset;
        public readonly int FieldLength;
        public readonly bool IsBigEndian;

        public MemberLayoutAttribute(int blockOffset, int fieldLength, bool isBigEndian = false)
        {
            BlockOffset = blockOffset;
            FieldLength = fieldLength;
            IsBigEndian = isBigEndian;
        }

    }
}
