using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.MessagePack
{
    internal sealed class MessagePackEntity : TargetEntity
    {
        public int MemberKeyOffset { get; set; }
        public MessagePackEntity(TargetDomain domain, TypeFullName entityName, Location location) 
            : base(domain, entityName, location) { }
    }
}
