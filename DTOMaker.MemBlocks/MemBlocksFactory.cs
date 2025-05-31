using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.MemBlocks
{
    internal class MemBlocksFactory : ITargetFactory
    {
        public TargetDomain CreateDomain(string name, Location location) => new MemBlockDomain(name, location);
        public TargetEntity CreateEntity(TargetDomain domain, TypeFullName tfn, Location location) => new MemBlockEntity(domain, tfn, location);
        public TargetMember CreateMember(TargetEntity entity, string name, Location location) => new MemBlockMember(entity, name, location);
        public TargetMember CloneMember(TargetEntity entity, TargetMember source) => new MemBlockMember(entity, (MemBlockMember)source);
    }
}
