using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.MessagePack
{
    internal class MessagePackFactory : ITargetFactory
    {
        public TargetDomain CreateDomain(string name, Location location) => new MessagePackDomain(name, location);
        public TargetEntity CreateEntity(TargetDomain domain, TypeFullName tfn, Location location) => new MessagePackEntity(domain, tfn, location);
        public TargetMember CreateMember(TargetEntity entity, string name, Location location) => new MessagePackMember(entity, name, location);
        public TargetMember CloneMember(TargetEntity entity, TargetMember source) => new MessagePackMember(entity, (MessagePackMember)source);
    }
}
