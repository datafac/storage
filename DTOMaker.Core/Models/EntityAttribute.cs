using System;

namespace DTOMaker.Models
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class EntityAttribute : Attribute
    {
        public readonly int BlockSize;
        public EntityAttribute(int blockSize)
        {
            if (blockSize < 0) throw new ArgumentOutOfRangeException(nameof(blockSize), blockSize, "Must be >= 0");
            BlockSize = blockSize;
        }
    }
}