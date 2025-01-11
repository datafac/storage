using System;

namespace DTOMaker.Models.MemBlocks
{
    /// <summary>
    /// Defines the length (in bytes) of the memory block containing a UTF8-encoded string.
    /// The length must be a power of 2 up to 8K. The first byte, or first 2 bytes, of the block
    /// contain the actual number of bytes encoded, and the remainder contains the encoded
    /// characters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class StrLenAttribute : Attribute
    {
        public readonly int StringLength;

        public StrLenAttribute(int stringLength)
        {
            StringLength = stringLength;
        }
    }
}
