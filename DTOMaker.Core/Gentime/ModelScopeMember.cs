using System;
using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public abstract class ModelScopeMember : ModelScopeBase
    {
        public ModelScopeMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member) 
            : base(parent, language)
        {
            string memberType = _language.GetDataTypeToken(member.MemberTypeName);
            _variables.Add("MemberIsObsolete", member.IsObsolete);
            _variables.Add("MemberObsoleteMessage", member.ObsoleteMessage);
            _variables.Add("MemberObsoleteIsError", member.ObsoleteIsError);
            _variables.Add("MemberType", memberType);
            _variables.Add("MemberIsNullable", member.MemberIsNullable);
            _variables.Add("MemberIsValueType", member.MemberIsValueType);
            _variables.Add("MemberIsReferenceType", member.MemberIsReferenceType);
            _variables.Add("MemberIsArray", member.MemberIsArray);
            _variables.Add("MemberSequence", member.Sequence);
            _variables.Add("ScalarMemberSequence", member.Sequence);
            _variables.Add(member.MemberIsNullable ? "ScalarNullableMemberSequence" : "ScalarRequiredMemberSequence", member.Sequence);
            _variables.Add("VectorMemberSequence", member.Sequence);
            _variables.Add("MemberName", member.Name);
            _variables.Add("ScalarMemberName", member.Name);
            _variables.Add(member.MemberIsNullable ? "ScalarNullableMemberName" : "ScalarRequiredMemberName", member.Name);
            _variables.Add("VectorMemberName", member.Name);
            _variables.Add("MemberJsonName", member.Name.ToCamelCase());
            _variables.Add("MemberDefaultValue", _language.GetDefaultValue(member.MemberTypeName));
            _variables.Add("FieldOffset", member.FieldOffset);
            _variables.Add("FieldLength", member.FieldLength);
            _variables.Add("ArrayLength", member.ArrayLength);
            _variables.Add("MemberBELE", member.IsBigEndian ? "BE" : "LE");
            _variables.Add("IsBigEndian", member.IsBigEndian);
            // padded versions of above for docgen
            _variables.Add("MemberSequenceR4", member.Sequence.ToString().PadLeft(4));
            _variables.Add("FieldOffsetR4", member.FieldOffset.ToString().PadLeft(4));
            _variables.Add("FieldLengthR4", member.FieldLength.ToString().PadLeft(4));
            _variables.Add("ArrayLengthR4", member.ArrayLength == 0 ? "    " : member.ArrayLength.ToString().PadLeft(4));
            _variables.Add("MemberTypeL7", memberType.PadRight(7));
        }

        protected override (bool?, IModelScope[]) OnGetInnerScopes(string iteratorName)
        {
            return (null, Array.Empty<IModelScope>());
        }
    }
}
