using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.Gentime
{
    public sealed class ModelScope_Entity : IModelScope
    {
        private readonly ILanguage _language;
        private readonly TargetEntity _entity;

        public ImmutableDictionary<string, TokenValue?> Tokens { get; }

        public IEnumerable<KeyValuePair<string, object?>> TokenValues => Tokens.Select(t => new KeyValuePair<string, object?>(t.Key, t.Value?.Value));

        public ModelScope_Entity(ILanguage language, TargetEntity entity, ImmutableDictionary<string, TokenValue?> parentTokens)
        {
            _language = language;
            _entity = entity;
            Tokens = parentTokens
                .Add("EntityName", new TokenValue(entity.Name))
                .Add("BlockLength", new TokenValue(entity.BlockLength))
                ;
        }

        public (bool?, IModelScope[]) GetInnerScopes(string iteratorName)
        {
            switch (iteratorName.ToLowerInvariant())
            {
                case "members":
                    TargetMember[] members = _entity.Members.Values.ToArray();
                    if (members.Length > 0)
                        return (true, members.OrderBy(m => m.Sequence).Select(m => new ModelScope_Member(_language, m, Tokens)).ToArray());
                    else
                        return (false, new IModelScope[] { new ModelScope_Empty() });
                default:
                    return (null, Array.Empty<IModelScope>());
            }
        }
    }
}
