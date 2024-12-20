using System;

namespace DTOMaker.Models.MemBlocks
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class EntityLayoutAttribute : Attribute
    {
        public readonly LayoutMethod LayoutMethod;
        public readonly int BlockLength;
        public EntityLayoutAttribute(string entityId, LayoutMethod layoutMethod, int blockLength = 0)
        {
            BlockLength = blockLength;
            LayoutMethod = layoutMethod;
        }
    }
}
