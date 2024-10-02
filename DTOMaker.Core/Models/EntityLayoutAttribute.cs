using System;

namespace DTOMaker.Models
{

    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class EntityLayoutAttribute : Attribute
    {
        public readonly MemberLayoutMethod LayoutMethod;
        public readonly int? BlockLength;
        public EntityLayoutAttribute(MemberLayoutMethod layoutMethod, int? blockLength = null)
        {
            BlockLength = blockLength;
            LayoutMethod = layoutMethod;
        }
    }
}