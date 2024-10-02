using System;

namespace DTOMaker.Models
{

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