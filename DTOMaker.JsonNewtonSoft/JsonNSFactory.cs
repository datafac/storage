using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.JsonNewtonSoft
{
    internal class JsonNSFactory : ITargetFactory
    {
        public TargetDomain CreateDomain(string name, Location location) => new JsonNSDomain(name, location);
        public TargetEntity CreateEntity(TargetDomain domain, TypeFullName tfn, Location location) => new JsonNSEntity(domain, tfn, location);
        public TargetMember CreateMember(TargetEntity entity, string name, Location location) => new JsonNSMember(entity, name, location);
        public TargetMember CloneMember(TargetEntity entity, TargetMember source) => new JsonNSMember(entity, (JsonNSMember)source);
    }
}
