using System;

namespace DTOMaker.Gentime
{
    public abstract class ModelScopeMember : ModelScopeBase
    {
        public ModelScopeMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member) 
            : base(parent, factory, language)
        {
            _variables["MemberIsObsolete"] = member.IsObsolete;
            _variables["MemberObsoleteMessage"] = member.ObsoleteMessage;
            _variables["MemberObsoleteIsError"] = member.ObsoleteIsError;
            _variables["MemberType"] = _language.GetDataTypeToken(member.MemberType);
            _variables["MemberIsNullable"] = member.MemberIsNullable;
            _variables["MemberIsValueType"] = member.MemberIsValueType;
            _variables["MemberIsReferenceType"] = member.MemberIsReferenceType;
            _variables["MemberIsVector"] = member.MemberIsVector;
            _variables["MemberSequence"] = member.Sequence;
            _variables["ScalarMemberSequence"] = member.Sequence;
            if (member.MemberIsNullable)
                _variables["ScalarNullableMemberSequence"] = member.Sequence;
            else
                _variables["ScalarRequiredMemberSequence"] = member.Sequence;
            _variables["VectorMemberSequence"] = member.Sequence;
            _variables["MemberName"] = member.Name;
            _variables["ScalarMemberName"] = member.Name;
            if (member.MemberIsNullable)
                _variables["ScalarNullableMemberName"] = member.Name;
            else
                _variables["ScalarRequiredMemberName"] = member.Name;
            _variables["VectorMemberName"] = member.Name;
            _variables["MemberJsonName"] = member.Name.ToCamelCase();
            _variables["MemberDefaultValue"] = _language.GetDefaultValue(member.MemberType);
            _variables["MemberIsEntity"] = member.MemberIsEntity;
            if (member.MemberIsEntity)
            {
                if (member.MemberIsNullable)
                    _variables["NullableEntityMemberName"] = member.Name;
                else
                    _variables["RequiredEntityMemberName"] = member.Name;
            }
        }

        protected override (bool?, IModelScope[]) OnGetInnerScopes(string iteratorName)
        {
            return (null, Array.Empty<IModelScope>());
        }
    }
}
