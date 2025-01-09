using System;

namespace DTOMaker.Gentime
{
    public abstract class ModelScopeMember : ModelScopeBase
    {
        public ModelScopeMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member) 
            : base(parent, factory, language)
        {
            string memberType = _language.GetDataTypeToken(member.MemberTypeName);
            _variables["MemberIsObsolete"] = member.IsObsolete;
            _variables["MemberObsoleteMessage"] = member.ObsoleteMessage;
            _variables["MemberObsoleteIsError"] = member.ObsoleteIsError;
            _variables["MemberType"] = memberType;
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
            _variables["MemberNameSpace"] = member.MemberName.NameSpace;
            _variables["MemberName"] = member.MemberName.ShortName;
            _variables["ScalarMemberName"] = member.MemberName.ShortName;
            if (member.MemberIsNullable)
                _variables["ScalarNullableMemberName"] = member.MemberName.ShortName;
            else
                _variables["ScalarRequiredMemberName"] = member.MemberName.ShortName;
            _variables["VectorMemberName"] = member.MemberName.ShortName;
            _variables["MemberJsonName"] = member.MemberName.ShortName.ToCamelCase();
            _variables["MemberDefaultValue"] = _language.GetDefaultValue(member.MemberTypeName);
            _variables["MemberIsEntity"] = member.MemberIsEntity;
        }

        protected override (bool?, IModelScope[]) OnGetInnerScopes(string iteratorName)
        {
            return (null, Array.Empty<IModelScope>());
        }
    }
}
