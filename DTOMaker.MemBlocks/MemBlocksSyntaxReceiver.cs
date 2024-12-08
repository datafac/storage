using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.MemBlocks
{
    public readonly struct EntityLayoutAttribute { }
    public readonly struct MemberLayoutAttribute { }

    internal class MemBlocksSyntaxReceiver : SyntaxReceiverBase
    {
        private static TargetDomain DomainFactory(string name, Location location) => new MemBlockDomain(name, location);
        private static TargetEntity EntityFactory(TargetDomain domain, string name, Location location) => new MemBlockEntity(domain, name, location);
        private static TargetMember MemberFactory(TargetEntity entity, string name, Location location) => new MemBlockMember(entity, name, location);

        protected override void OnProcessEntityAttributes(TargetEntity baseEntity, Location location, ImmutableArray<AttributeData> entityAttributes)
        {
            if (baseEntity is MemBlockEntity entity
                && entityAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(EntityLayoutAttribute)) is AttributeData entityLayoutAttr)
            {
                // found entity layout attribute
                entity.HasEntityLayoutAttribute = true;
                var attributeArguments = entityLayoutAttr.ConstructorArguments;
                if (CheckAttributeArguments(nameof(EntityLayoutAttribute), attributeArguments, 2, entity, location))
                {
                    TryGetAttributeArgumentValue<int>(entity, location, attributeArguments, 0, (value) => { entity.LayoutMethod = (LayoutMethod)value; });
                    TryGetAttributeArgumentValue<int>(entity, location, attributeArguments, 1, (value) => { entity.BlockLength = value; });
                }
            }
        }

        protected override void OnProcessMemberAttributes(TargetMember baseMember, Location location, ImmutableArray<AttributeData> memberAttributes)
        {
            if (baseMember is MemBlockMember member)
            {
                if (memberAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(MemberLayoutAttribute)) is AttributeData memberOffsetAttr)
                {
                    member.HasMemberLayoutAttribute = true;
                    var attributeArguments = memberOffsetAttr.ConstructorArguments;
                    if (CheckAttributeArguments(nameof(MemberLayoutAttribute), attributeArguments, 3, member, location))
                    {
                        TryGetAttributeArgumentValue<int>(member, location, attributeArguments, 0, (value) => { member.FieldOffset = value; });
                        TryGetAttributeArgumentValue<bool>(member, location, attributeArguments, 1, (value) => { member.IsBigEndian = value; });
                        TryGetAttributeArgumentValue<int>(member, location, attributeArguments, 2, (value) => { member.ArrayLength = value; });
                    }
                }
            }
        }

        public MemBlocksSyntaxReceiver() : base(DomainFactory, EntityFactory, MemberFactory)
        {
        }
    }
}
