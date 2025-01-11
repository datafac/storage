using System;

namespace DTOMaker.Models.MemBlocks
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class IdAttribute : Attribute
    {
        public readonly string EntityId;
        public IdAttribute(string entityId)
        {
            EntityId = entityId;
        }
    }
}
