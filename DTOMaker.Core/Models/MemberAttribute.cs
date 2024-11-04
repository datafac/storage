using System;

namespace DTOMaker.Models
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MemberAttribute : Attribute
    {
        public readonly int Sequence;
        public readonly int ArrayLength;

        public MemberAttribute(int sequence, int arrayLength = 0)
        {
            Sequence = sequence;
            ArrayLength = arrayLength;
        }

    }
}
