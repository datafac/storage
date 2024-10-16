using System;
using System.Collections.Immutable;

namespace DTOMaker.Gentime
{
    internal sealed class ModelScope_Member : IModelScope
    {
        private readonly ILanguage _language;
        private readonly TargetMember _member;

        public ImmutableDictionary<string, object?> Tokens { get; }

        public ModelScope_Member(ILanguage language, TargetMember member, ImmutableDictionary<string, object?> parentTokens)
        {
            _language = language;
            _member = member;
            //bool nullable = false; // member.DataType?.Nullable ?? true;
            var builder = parentTokens.ToBuilder();
            builder.Add("MemberType", _language.GetDataTypeToken(member.MemberType));
            //builder.Add("IsNullable", nullable));
            builder.Add("MemberSequence", member.Sequence);
            builder.Add("MemberName", member.Name);
            builder.Add("MemberJsonName", member.Name.ToCamelCase());
            builder.Add("MemberDefaultValue", _language.GetDefaultValue(member.MemberType));
            builder.Add("MemberBELE", member.IsBigEndian ? "BE" : "LE");
            builder.Add("FieldOffset", member.FieldOffset);
            builder.Add("FieldLength", member.FieldLength);
            Tokens = builder.ToImmutable();
        }

        public (bool?, IModelScope[]) GetInnerScopes(string iteratorName)
        {
            return (null, Array.Empty<IModelScope>());
        }
    }
}
