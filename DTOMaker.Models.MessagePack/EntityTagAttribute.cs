using System;

namespace DTOMaker.Models.MessagePack
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class EntityKeyAttribute : Attribute
    {
        public readonly int EntityKey;
        public readonly int MemberKeyOffset;

        public EntityKeyAttribute(int entityKey, int memberKeyOffset = 0)
        {
            EntityKey = entityKey;
            MemberKeyOffset = memberKeyOffset;
        }
    }
}
