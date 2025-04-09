using DTOMaker.Gentime;
using System;

namespace DTOMaker.MemBlocks
{
    internal sealed class MemBlocksModelScopeMember : ModelScopeMember
    {
        private readonly MemBlockMember _member;
        public bool IsFixedLength => _member.IsFixedLength;
        public int FieldLength => _member.FieldLength;

        public MemBlocksModelScopeMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember baseMember)
            : base(parent, factory, language, baseMember)
        {
            _member = baseMember as MemBlockMember ?? throw new ArgumentException("Expected type to be MemBlockMember", nameof(baseMember));
            var entity = _member.Entity as MemBlockEntity ?? throw new ArgumentException("Expected member.Entity to be a MemBlocksEntity");

            string memberType = _language.GetDataTypeToken(_member.MemberType);
            _tokens["FieldLength"] = _member.FieldLength;
            _tokens["ArrayLength"] = _member.ArrayCapacity;
            _tokens["MemberBELE"] = _member.IsBigEndian ? "BE" : "LE";
            _tokens["IsBigEndian"] = _member.IsBigEndian;
            // padded versions of above for docgen
            _tokens["MemberSequenceR4"] = _member.Sequence.ToString().PadLeft(4);
            _tokens["FieldOffsetR4"] = _member.FieldOffset.ToString().PadLeft(4);
            _tokens["FieldLengthR4"] = _member.FieldLength.ToString().PadLeft(4);
            _tokens["ArrayLengthR4"] = _member.ArrayCapacity == 0 ? "    " : _member.ArrayCapacity.ToString().PadLeft(4);
            _tokens["MemberTypeL7"] = memberType.PadRight(7);
            switch (_member.Kind)
            {
                case MemberKind.Native:
                    _tokens["ScalarFieldOffset"] = _member.FieldOffset;
                    break;
                case MemberKind.Vector:
                    _tokens["VectorFieldOffset"] = _member.FieldOffset;
                    break;
                case MemberKind.Entity:
                    _tokens[(_member.MemberIsNullable ? "Nullable" : "Required") + "EntityFieldOffset"] = _member.FieldOffset;
                    break;
                case MemberKind.Binary:
                    _tokens[(_member.MemberIsNullable ? "Nullable" : "Required") + (_member.IsFixedLength ? "FixLen" : "VarLen") + "BinaryMemberName"] = _member.Name;
                    _tokens[(_member.MemberIsNullable ? "Nullable" : "Required") + (_member.IsFixedLength ? "FixLen" : "VarLen") + "BinaryFieldOffset"] = _member.FieldOffset;
                    _tokens[(_member.MemberIsNullable ? "Nullable" : "Required") + (_member.IsFixedLength ? "FixLen" : "VarLen") + "BinaryFieldLength"] = _member.FieldLength;
                    break;
                case MemberKind.String:
                    _tokens[(_member.MemberIsNullable ? "Nullable" : "Required") + (_member.IsFixedLength ? "FixLen" : "VarLen") + "StringMemberName"] = _member.Name;
                    _tokens[(_member.MemberIsNullable ? "Nullable" : "Required") + (_member.IsFixedLength ? "FixLen" : "VarLen") + "StringFieldOffset"] = _member.FieldOffset;
                    _tokens[(_member.MemberIsNullable ? "Nullable" : "Required") + (_member.IsFixedLength ? "FixLen" : "VarLen") + "StringFieldLength"] = _member.FieldLength;
                    break;
                default:
                    throw new NotImplementedException($"Member.Kind: {_member.Kind}");
            }
        }
    }
}
