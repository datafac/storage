using System;

namespace DTOMaker.Models
{
    // todo move to MemBlocks repo
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class EntityLayoutAttribute : Attribute
    {
        public readonly LayoutMethod LayoutMethod;
        public readonly int BlockLength;
        public EntityLayoutAttribute(LayoutMethod layoutMethod, int blockLength = 0)
        {
            BlockLength = blockLength;
            LayoutMethod = layoutMethod;
        }
    }
}