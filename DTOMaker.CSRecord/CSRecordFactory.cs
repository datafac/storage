using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.CSRecord
{
    internal class CSRecordFactory : ITargetFactory
    {
        public TargetDomain CreateDomain(string name, Location location) => new CSRecordDomain(name, location);
        public TargetEntity CreateEntity(TargetDomain domain, TypeFullName tfn, Location location) => new CSRecordEntity(domain, tfn, location);
        public TargetMember CreateMember(TargetEntity entity, string name, Location location) => new CSRecordMember(entity, name, location);
        public TargetMember CloneMember(TargetEntity entity, TargetMember source) => new CSRecordMember(entity, (CSRecordMember)source);
    }
}
