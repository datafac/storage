using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.Gentime
{
    internal sealed class ModelScope_Member : IModelScope
    {
        private readonly ILanguage _language;
        private readonly TargetMember _member;

        public ImmutableDictionary<string, TokenValue?> Tokens { get; }

        public IEnumerable<KeyValuePair<string, object?>> TokenValues => Tokens.Select(t => new KeyValuePair<string, object?>(t.Key, t.Value?.Value));

        public ModelScope_Member(ILanguage language, TargetMember member, ImmutableDictionary<string, TokenValue?> parentTokens)
        {
            _language = language;
            _member = member;
            //bool nullable = false; // member.DataType?.Nullable ?? true;
            var builder = parentTokens.ToBuilder();
            builder.Add("MemberType", new TokenValue(_language.GetDataTypeToken(member.MemberType)));
            //builder.Add("IsNullable", new TokenValue(nullable));
            builder.Add("MemberSequence", new TokenValue(member.Sequence));
            builder.Add("MemberName", new TokenValue(member.Name));
            builder.Add("MemberJsonName", new TokenValue(member.Name.ToCamelCase()));
            builder.Add("MemberDefaultValue", new TokenValue(_language.GetDefaultValue(member.MemberType)));
            builder.Add("MemberBELE", new TokenValue(member.IsBigEndian ? "BE" : "LE"));
            builder.Add("FieldOffset", new TokenValue(member.FieldOffset));
            builder.Add("FieldLength", new TokenValue(member.FieldLength));
            Tokens = builder.ToImmutable();
        }

        public (bool?, IModelScope[]) GetInnerScopes(string iteratorName)
        {
            return (null, Array.Empty<IModelScope>());
        }
    }
}
