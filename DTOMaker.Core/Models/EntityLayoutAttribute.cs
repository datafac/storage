using System;

namespace DTOMaker.Models
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class EntityLayoutAttribute : Attribute
    {
        public readonly int BlockSize;
        public EntityLayoutAttribute(int blockSize)
        {
            BlockSize = blockSize;
        }
    }
}