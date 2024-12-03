using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.MessagePack
{
    internal sealed class MessagePackEntity : TargetEntity
    {
        public MessagePackEntity(TargetDomain domain, string name, Location location) : base(domain, name, location) { }
    }
}
