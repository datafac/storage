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
            _variables["FieldOffset"] = member.FieldOffset;
            _variables["FieldLength"] = member.FieldLength;
            _variables["ArrayLength"] = member.ArrayCapacity;
            _variables["MemberBELE"] = member.IsBigEndian ? "BE" : "LE";
            _variables["IsBigEndian"] = member.IsBigEndian;
            // padded versions of above for docgen
            _variables["MemberSequenceR4"] = member.Sequence.ToString().PadLeft(4);
            _variables["FieldOffsetR4"] = member.FieldOffset.ToString().PadLeft(4);
            _variables["FieldLengthR4"] = member.FieldLength.ToString().PadLeft(4);
            _variables["ArrayLengthR4"] = member.ArrayCapacity == 0 ? "    " : member.ArrayCapacity.ToString().PadLeft(4);
            _variables["MemberTypeL7"] = memberType.PadRight(7);
        }
    }
}
