using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.MessagePack
{
    internal sealed class MessagePackEntity : TargetEntity
    {
        public int EntityKey { get; set; }
        public int MemberKeyOffset { get; set; }
        public MessagePackEntity(TargetDomain domain, string nameSpace, string name, Location location) : base(domain, nameSpace, name, location) { }
    }
}
