using System;

namespace DTOMaker.Models
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class EntityAttribute : Attribute
    {
        public readonly int Tag;

        public EntityAttribute(int tag = 0)
        {
            Tag = tag;
        }
    }
}