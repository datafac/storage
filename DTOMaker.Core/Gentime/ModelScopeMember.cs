using System;

namespace DTOMaker.Gentime
{
    public abstract class ModelScopeMember : ModelScopeBase
    {
        private readonly TargetMember _member;
        public ModelScopeMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member) 
            : base(parent, factory, language)
        {
            _member = member;
            _tokens["MemberIsObsolete"] = member.IsObsolete;
            _tokens["MemberObsoleteMessage"] = member.ObsoleteMessage;
            _tokens["MemberObsoleteIsError"] = member.ObsoleteIsError;
            _tokens["MemberType"] = _language.GetDataTypeToken(member.MemberType);
            _tokens["MemberTypeName"] = member.MemberType.ShortName;
            _tokens["MemberTypeNameSpace"] = member.MemberType.NameSpace;
            _tokens["MemberIsNullable"] = member.MemberIsNullable;
            _tokens["MemberIsValueType"] = member.MemberIsValueType;
            _tokens["MemberIsReferenceType"] = member.MemberIsReferenceType;
            _tokens["MemberIsVector"] = member.MemberIsVector;
            _tokens["MemberSequence"] = member.Sequence;
            _tokens["ScalarMemberSequence"] = member.Sequence;
            if (member.MemberIsNullable)
                _tokens["ScalarNullableMemberSequence"] = member.Sequence;
            else
                _tokens["ScalarRequiredMemberSequence"] = member.Sequence;
            _tokens["VectorMemberSequence"] = member.Sequence;
            _tokens["MemberName"] = member.Name;
            _tokens["ScalarMemberName"] = member.Name;
            if (member.MemberIsNullable)
                _tokens["ScalarNullableMemberName"] = member.Name;
            else
                _tokens["ScalarRequiredMemberName"] = member.Name;
            _tokens["VectorMemberName"] = member.Name;
            _tokens["MemberJsonName"] = member.Name.ToCamelCase();
            _tokens["MemberDefaultValue"] = _language.GetDefaultValue(member.MemberType);
            _tokens["MemberIsEntity"] = member.MemberIsEntity;
            if (member.MemberIsEntity)
            {
                if (member.MemberIsNullable)
                    _tokens["NullableEntityMemberName"] = member.Name;
                else
                    _tokens["RequiredEntityMemberName"] = member.Name;
            }
        }

        public bool IsEntity => _member.MemberIsEntity;
        public bool IsNullable => _member.MemberIsNullable;
        public bool IsVector => _member.MemberIsVector;
        public bool IsObsolete => _member.IsObsolete;
        public int FieldLength => _member.FieldLength;

        protected override (bool?, IModelScope[]) OnGetInnerScopes(string iteratorName)
        {
            return (null, Array.Empty<IModelScope>());
        }
    }
}
