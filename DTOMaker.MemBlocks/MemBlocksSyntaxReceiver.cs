using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.MemBlocks
{
    internal class MemBlocksSyntaxReceiver : SyntaxReceiverBase
    {
        private static TargetDomain DomainFactory(string name, Location location) => new MemBlockDomain(name, location);
        private static TargetEntity EntityFactory(TargetDomain domain, string name, Location location) => new MemBlockEntity(domain, name, location);
        private static TargetMember MemberFactory(TargetEntity entity, string name, Location location) => new MemBlockMember(entity, name, location);

        protected override void OnProcessEntityAttributes(TargetEntity baseEntity, Location location, ImmutableArray<AttributeData> entityAttributes)
        {
            if (baseEntity is MemBlockEntity entity
                && entityAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(EntityLayoutAttribute)) is AttributeData entityAttr)
            {
                // found entity layout attribute
                // todo
                //var attributeArguments = entityAttr.ConstructorArguments;
                //if (CheckAttributeArguments(nameof(EntityLayoutAttribute), attributeArguments, 2, entity, location))
                //{
                //    TryGetAttributeArgumentValue<int>(entity, location, attributeArguments, 0, (value) => { entity.EntityTag = value; });
                //    TryGetAttributeArgumentValue<int>(entity, location, attributeArguments, 1, (value) => { entity.MemberTagOffset = value; });
                //}
            }
        }

        public MemBlocksSyntaxReceiver() : base(DomainFactory, EntityFactory, MemberFactory)
        {
        }
    }
}
