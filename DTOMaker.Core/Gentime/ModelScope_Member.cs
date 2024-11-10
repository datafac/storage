using System;
using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    internal sealed class ModelScope_Member : IModelScope
    {
        private readonly TargetMember _member;
        private readonly ILanguage _language;
        private readonly Dictionary<string, object?> _variables = new Dictionary<string, object?>();
        public IDictionary<string, object?> Variables => _variables;

        public ModelScope_Member(ILanguage language, TargetMember member, IEnumerable<KeyValuePair<string, object?>> tokens)
        {
            _language = language;
            _member = member;

            foreach (var token in tokens)
            {
                _variables[token.Key] = token.Value;
            }
            _variables.Add("MemberIsObsolete", member.IsObsolete);
            _variables.Add("MemberObsoleteMessage", member.ObsoleteMessage);
            _variables.Add("MemberObsoleteIsError", member.ObsoleteIsError);
            _variables.Add("MemberType", _language.GetDataTypeToken(member.MemberTypeName));
            _variables.Add("MemberIsNullable", member.MemberIsNullable);
            _variables.Add("MemberIsValueType", member.MemberIsValueType);
            _variables.Add("MemberIsReferenceType", member.MemberIsReferenceType);
            _variables.Add("MemberIsArray", member.MemberIsArray);
            _variables.Add("MemberSequence", member.Sequence);
            _variables.Add("ScalarMemberSequence", member.Sequence);
            _variables.Add("VectorMemberSequence", member.Sequence);
            _variables.Add("MemberName", member.Name);
            _variables.Add("ScalarMemberName", member.Name);
            _variables.Add("VectorMemberName", member.Name);
            _variables.Add("MemberJsonName", member.Name.ToCamelCase());
            _variables.Add("MemberDefaultValue", _language.GetDefaultValue(member.MemberTypeName));
            _variables.Add("FieldOffset", member.FieldOffset);
            _variables.Add("FieldLength", member.FieldLength);
            _variables.Add("ArrayLength", member.ArrayLength);
            _variables.Add("MemberBELE", member.IsBigEndian ? "BE" : "LE");
            _variables.Add("IsBigEndian", member.IsBigEndian);
        }

        public (bool?, IModelScope[]) GetInnerScopes(string iteratorName)
        {
            return (null, Array.Empty<IModelScope>());
        }
    }
}
