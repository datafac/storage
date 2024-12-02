using System;

namespace DTOMaker.Models
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class EntityAttribute : Attribute
    {
        public readonly int IntTag;

        public EntityAttribute(int intTag = 0)
        {
            IntTag = intTag;
        }
    }
}