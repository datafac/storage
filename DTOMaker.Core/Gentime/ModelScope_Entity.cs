using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DTOMaker.Gentime
{
    public sealed class ModelScope_Entity : IModelScope
    {
        private readonly TargetEntity _entity;
        private readonly ILanguage _language;
        private readonly Dictionary<string, object?> _variables = new Dictionary<string, object?>();
        public IDictionary<string, object?> Variables => _variables;

        public ModelScope_Entity(ILanguage language, TargetEntity entity, IEnumerable<KeyValuePair<string, object?>> tokens)
        {
            _language = language;
            _entity = entity;
            
            foreach (var token in tokens)
            {
                _variables[token.Key] = token.Value;
            }
            _variables["EntityName"] = entity.Name;
            _variables["BlockLength"] = entity.BlockLength;
        }

        public (bool?, IModelScope[]) GetInnerScopes(string iteratorName)
        {
            switch (iteratorName.ToLowerInvariant())
            {
                case "members":
                    TargetMember[] members = _entity.Members.Values.ToArray();
                    if (members.Length > 0)
                        return (true, members.OrderBy(m => m.Sequence).Select(m => new ModelScope_Member(_language, m, _variables)).ToArray());
                    else
                        return (false, new IModelScope[] { new ModelScope_Empty() });
                default:
                    return (null, Array.Empty<IModelScope>());
            }
        }
    }
}
