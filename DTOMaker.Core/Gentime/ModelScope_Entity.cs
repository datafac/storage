using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.Gentime
{
    public sealed class ModelScope_Entity : IModelScope
    {
        private readonly ModelScope_Domain _domain;
        private readonly TargetEntity _entity;
        private readonly ILanguage _language;

        private readonly ImmutableArray<TargetEntity> _derivedEntities;

        private readonly Dictionary<string, object?> _variables = new Dictionary<string, object?>();
        public IDictionary<string, object?> Variables => _variables;

        public ModelScope_Entity(ModelScope_Domain domain, ILanguage language, TargetEntity entity)
        {
            _domain = domain;
            _language = language;
            _entity = entity;
            _derivedEntities = _domain.Entities.Where(e => e.IsChildOf(_entity)).ToImmutableArray();

            foreach (var token in _domain.Variables)
            {
                _variables[token.Key] = token.Value;
            }
            _variables["EntityName"] = entity.Name;
            _variables["EntityName2"] = entity.Name;
            _variables["BaseName"] = entity.Base?.Name ?? "EntityBase";
            _variables["HasDerivedEntities"] = _derivedEntities.Length > 0;
            _variables["BlockLength"] = entity.BlockLength;
            _variables["EntityTag"] = entity.Tag;

        }

        public (bool?, IModelScope[]) GetInnerScopes(string iteratorName)
        {
            switch (iteratorName.ToLowerInvariant())
            {
                case "derivedentities":
                    if (_derivedEntities.Length > 0)
                        return (true, _derivedEntities.OrderBy(e => e.Name).Select(e => new ModelScope_Entity(_domain, _language, e)).ToArray());
                    else
                        return (false, new IModelScope[] { new ModelScope_Empty() });
                case "members":
                    TargetMember[] members = _entity.Members.Values.ToArray();
                    if (members.Length > 0)
                        return (true, members.OrderBy(m => m.Sequence).Select(m => new ModelScope_Member(this, _language, m)).ToArray());
                    else
                        return (false, new IModelScope[] { new ModelScope_Empty() });
                default:
                    return (null, Array.Empty<IModelScope>());
            }
        }
    }
}
