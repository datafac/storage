using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.CSPoco
{
    internal class CSPocoFactory : ITargetFactory
    {
        public TargetDomain CreateDomain(string name, Location location) => new CSPocoDomain(name, location);
        public TargetEntity CreateEntity(TargetDomain domain, TypeFullName tfn, Location location) => new CSPocoEntity(domain, tfn, location);
        public TargetMember CreateMember(TargetEntity entity, string name, Location location) => new CSPocoMember(entity, name, location);
        public TargetMember CloneMember(TargetEntity entity, TargetMember source) => new CSPocoMember(entity, (CSPocoMember)source);
    }
}
