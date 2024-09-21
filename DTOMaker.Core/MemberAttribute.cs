using System;

namespace DTOMaker.Core
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MemberAttribute : Attribute
    {
        public readonly int BlockOffset;
        public readonly int FieldLength;
        public readonly bool IsBigEndian;

        public MemberAttribute(int blockOffset, int fieldLength, bool isBigEndian)
        {
            BlockOffset = blockOffset;
            FieldLength = fieldLength;
            IsBigEndian = isBigEndian;
        }

        public MemberAttribute(int blockOffset, int fieldLength)
            : this(blockOffset, fieldLength, false)
        {
        }

    }
}
