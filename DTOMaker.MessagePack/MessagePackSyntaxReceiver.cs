using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.MessagePack
{
    public readonly struct EntityTagAttribute { }

    internal class MessagePackSyntaxReceiver : SyntaxReceiverBase
    {
        private static TargetDomain DomainFactory(string name, Location location) => new MessagePackDomain(name, location);
        private static TargetEntity EntityFactory(TargetDomain domain, string name, Location location) => new MessagePackEntity(domain, name, location);
        private static TargetMember MemberFactory(TargetEntity entity, string name, Location location) => new MessagePackMember(entity, name, location);

        protected override void OnProcessEntityAttributes(TargetEntity baseEntity, Location location, ImmutableArray<AttributeData> entityAttributes)
        {
            if (baseEntity is MessagePackEntity entity
                && entityAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(EntityTagAttribute)) is AttributeData entityAttr)
            {
                // found entity tag attribute
                var attributeArguments = entityAttr.ConstructorArguments;
                if (CheckAttributeArguments(nameof(EntityTagAttribute), attributeArguments, 2, entity, location))
                {
                    TryGetAttributeArgumentValue<int>(entity, location, attributeArguments, 0, (value) => { entity.EntityTag = value; });
                    TryGetAttributeArgumentValue<int>(entity, location, attributeArguments, 1, (value) => { entity.MemberTagOffset = value; });
                }
            }
        }

        protected override void OnProcessMemberAttributes(TargetMember member, Location location, ImmutableArray<AttributeData> memberAttributes)
        {
            // not needed yet
        }

        public MessagePackSyntaxReceiver() : base(DomainFactory, EntityFactory, MemberFactory)
        {
        }
    }
}
