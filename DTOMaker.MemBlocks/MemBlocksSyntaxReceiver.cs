using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.MemBlocks
{
    public readonly struct LayoutAttribute { }
    public readonly struct OffsetAttribute { }
    public readonly struct EndianAttribute { }
    public readonly struct FixedLengthAttribute { }
    public readonly struct CapacityAttribute { }

    internal class MemBlocksSyntaxReceiver : SyntaxReceiverBase
    {
        protected override void OnProcessEntityAttributes(TargetEntity baseEntity, Location location, ImmutableArray<AttributeData> entityAttributes)
        {
            if (baseEntity is MemBlockEntity entity)
            {
                if (entityAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(LayoutAttribute)) is AttributeData entityLayoutAttr)
                {
                    // found layout attribute
                    var attributeArguments = entityLayoutAttr.ConstructorArguments;
                    if (CheckAttributeArguments(nameof(LayoutAttribute), attributeArguments, 2, entity, location))
                    {
                        TryGetAttributeArgumentValue<int>(entity, location, attributeArguments, 0, (value) => { entity.LayoutMethod = (LayoutMethod)value; });
                        TryGetAttributeArgumentValue<int>(entity, location, attributeArguments, 1, (value) => { entity.BlockLength = value; });
                    }
                }
                else
                {
                    // no layout attr
                }
            }
        }

        protected override void OnProcessMemberAttributes(TargetMember baseMember, Location location, ImmutableArray<AttributeData> memberAttributes)
        {
            if (baseMember is MemBlockMember member)
            {
                if (memberAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(OffsetAttribute)) is AttributeData offsetAttr)
                {
                    member.HasOffsetAttribute = true;
                    var attributeArguments = offsetAttr.ConstructorArguments;
                    if (CheckAttributeArguments(nameof(OffsetAttribute), attributeArguments, 1, member, location))
                    {
                        TryGetAttributeArgumentValue<int>(member, location, attributeArguments, 0, (value) => { member.FieldOffset = value; });
                    }
                }
                if (memberAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(FixedLengthAttribute)) is AttributeData strLenAttr)
                {
                    var attributeArguments = strLenAttr.ConstructorArguments;
                    if (CheckAttributeArguments(nameof(FixedLengthAttribute), attributeArguments, 1, member, location))
                    {
                        TryGetAttributeArgumentValue<int>(member, location, attributeArguments, 0, (value) => { member.FixedLength = value; });
                    }
                }
                if (memberAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(CapacityAttribute)) is AttributeData capacityAttr)
                {
                    var attributeArguments = capacityAttr.ConstructorArguments;
                    if (CheckAttributeArguments(nameof(CapacityAttribute), attributeArguments, 1, member, location))
                    {
                        TryGetAttributeArgumentValue<int>(member, location, attributeArguments, 0, (value) => { member.ArrayCapacity = value; });
                    }
                }
                if (memberAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(EndianAttribute)) is AttributeData memberEndianAttr)
                {
                    var attributeArguments = memberEndianAttr.ConstructorArguments;
                    if (CheckAttributeArguments(nameof(EndianAttribute), attributeArguments, 1, member, location))
                    {
                        TryGetAttributeArgumentValue<bool>(member, location, attributeArguments, 0, (value) => { member.IsBigEndian = value; });
                    }
                }
            }
        }

        public MemBlocksSyntaxReceiver() : base(new MemBlocksFactory())
        {
        }
    }
}
