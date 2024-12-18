using System;

namespace DTOMaker.Models.MemBlocks
{
    /// <summary>
    /// Defines the number of array elements, or string length (in bytes),
    /// which indirectly defines the total size of the memory used by the member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class LengthAttribute : Attribute
    {
        public readonly int ArrayLength;

        public LengthAttribute(int arrayLength)
        {
            ArrayLength = arrayLength;
        }
    }
}
