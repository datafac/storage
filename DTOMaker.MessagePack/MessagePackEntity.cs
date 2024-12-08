using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.MessagePack
{
    internal sealed class MessagePackEntity : TargetEntity
    {
        // todo set these somehow
        public int EntityTag { get; set; }
        public int MemberTagOffset { get; set; }
        public MessagePackEntity(TargetDomain domain, string name, Location location) : base(domain, name, location) { }
    }
}
