using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.MessagePack
{
    public readonly struct EntityKeyAttribute { }

    internal class MessagePackSyntaxReceiver : SyntaxReceiverBase
    {
        private static TargetDomain DomainFactory(string name, Location location) => new MessagePackDomain(name, location);
        private static TargetEntity EntityFactory(TargetDomain domain, string nameSpace, string name, Location location) => new MessagePackEntity(domain, nameSpace, name, location);
        private static TargetMember MemberFactory(TargetEntity entity, string name, Location location) => new MessagePackMember(entity, name, location);

        protected override void OnProcessEntityAttributes(TargetEntity baseEntity, Location location, ImmutableArray<AttributeData> entityAttributes)
        {
            if (baseEntity is MessagePackEntity entity
                && entityAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(EntityKeyAttribute)) is AttributeData entityAttr)
            {
                // found entity key attribute
                var attributeArguments = entityAttr.ConstructorArguments;
                if (CheckAttributeArguments(nameof(EntityKeyAttribute), attributeArguments, 2, entity, location))
                {
                    TryGetAttributeArgumentValue<int>(entity, location, attributeArguments, 0, (value) => { entity.EntityKey = value; });
                    TryGetAttributeArgumentValue<int>(entity, location, attributeArguments, 1, (value) => { entity.MemberKeyOffset = value; });
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
