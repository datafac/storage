using DTOMaker.Gentime;
using System;

namespace DTOMaker.MemBlocks
{
    internal sealed class MemBlocksModelScopeMember : ModelScopeMember
    {
        public MemBlocksModelScopeMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember baseMember)
            : base(parent, factory, language, baseMember)
        {
            MemBlockMember member = baseMember as MemBlockMember
                ?? throw new ArgumentException("Expected member to be a MemBlockMember", nameof(member));
            MemBlockEntity entity = member.Entity as MemBlockEntity
                ?? throw new ArgumentException("Expected member.Entity to be a MemBlocksEntity", nameof(member));

            string memberType = _language.GetDataTypeToken(member.MemberType);
            _tokens["FieldLength"] = member.FieldLength;
            _tokens["ArrayLength"] = member.ArrayCapacity;
            _tokens["MemberBELE"] = member.IsBigEndian ? "BE" : "LE";
            _tokens["IsBigEndian"] = member.IsBigEndian;
            // padded versions of above for docgen
            _tokens["MemberSequenceR4"] = member.Sequence.ToString().PadLeft(4);
            _tokens["FieldOffsetR4"] = member.FieldOffset.ToString().PadLeft(4);
            _tokens["FieldLengthR4"] = member.FieldLength.ToString().PadLeft(4);
            _tokens["ArrayLengthR4"] = member.ArrayCapacity == 0 ? "    " : member.ArrayCapacity.ToString().PadLeft(4);
            _tokens["MemberTypeL7"] = memberType.PadRight(7);
            switch (member.Kind)
            {
                case MemberKind.Scalar:
                    _tokens["ScalarFieldOffset"] = member.FieldOffset;
                    break;
                case MemberKind.Vector:
                    _tokens["VectorFieldOffset"] = member.FieldOffset;
                    break;
                case MemberKind.Entity:
                    _tokens[(member.MemberIsNullable ? "Nullable" : "Required") + "EntityFieldOffset"] = member.FieldOffset;
                    break;
                case MemberKind.Binary:
                    _tokens[(member.MemberIsNullable ? "Nullable" : "Required") + "BinaryFieldOffset"] = member.FieldOffset;
                    break;
                default:
                    throw new NotImplementedException($"Member.Kind: {member.Kind}");
            }
        }
    }
}
