using System;

namespace DTOMaker.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
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