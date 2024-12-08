using System;

namespace DTOMaker.Models
{
    // todo to MessagePack repo
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class EntityTagAttribute : Attribute
    {
        public readonly int EntityTag;
        public readonly int MemberTagOffset;

        public EntityTagAttribute(int entityTag, int memberTagOffset = 0)
        {
            EntityTag = entityTag;
            MemberTagOffset = memberTagOffset;
        }
    }
}
