using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.MessagePack
{
    public readonly struct MemberKeyOffsetAttribute { }

    internal class MessagePackSyntaxReceiver : SyntaxReceiverBase
    {
        protected override void OnProcessEntityAttributes(TargetEntity baseEntity, Location location, ImmutableArray<AttributeData> entityAttributes)
        {
            if (baseEntity is MessagePackEntity entity
                && entityAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(MemberKeyOffsetAttribute)) is AttributeData entityAttr)
            {
                // found entity key attribute
                var attributeArguments = entityAttr.ConstructorArguments;
                if (CheckAttributeArguments(nameof(MemberKeyOffsetAttribute), attributeArguments, 1, entity, location))
                {
                    TryGetAttributeArgumentValue<int>(entity, location, attributeArguments, 0, (value) => { entity.MemberKeyOffset = value; });
                }
            }
        }
        protected override void OnProcessMemberAttributes(TargetMember member, Location location, ImmutableArray<AttributeData> memberAttributes) { }
        public MessagePackSyntaxReceiver() : base(new MessagePackFactory()) { }
    }
}
