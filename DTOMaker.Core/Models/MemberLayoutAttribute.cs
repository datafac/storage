using System;

namespace DTOMaker.Models
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MemberLayoutAttribute : Attribute
    {
        public readonly int BlockOffset;
        public readonly bool IsBigEndian;

        public MemberLayoutAttribute(int blockOffset, bool isBigEndian = false)
        {
            BlockOffset = blockOffset;
            IsBigEndian = isBigEndian;
        }

    }
}
