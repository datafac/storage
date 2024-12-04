using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.Gentime
{
    public sealed class ModelScopeEntity : ModelScopeBase
    {
        private readonly TargetEntity _entity;

        private readonly ImmutableArray<ModelScopeEntity> _derivedEntities;
        private readonly ImmutableArray<ModelScopeMember> _members;

        public ModelScopeEntity(IModelScope parent, ILanguage language, TargetEntity entity)
            : base(parent, language)
        {
            _entity = entity;
            _derivedEntities = entity.Domain.Entities.Values
                .Where(e => e.IsChildOf(_entity))
                .OrderBy(e => e.Name)
                .Select(e => new ModelScopeEntity(parent, _language, e))
                .ToImmutableArray();
            _members = _entity.Members.Values
                .OrderBy(m => m.Sequence)
                .Select(m => new ModelScopeMember(this, _language, m))
                .ToImmutableArray();

            _variables["EntityName"] = entity.Name;
            _variables["EntityName2"] = entity.Name;
            _variables["BaseName"] = entity.Base?.Name ?? "EntityBase";
            _variables["HasDerivedEntities"] = _derivedEntities.Length > 0;
            _variables["BlockLength"] = entity.BlockLength;
            _variables["EntityTag"] = entity.Tag;

        }

        protected override (bool?, IModelScope[]) OnGetInnerScopes(string iteratorName)
        {
            switch (iteratorName.ToLowerInvariant())
            {
                case "derivedentities":
                    if (_derivedEntities.Length > 0)
                        return (true, _derivedEntities.ToArray());
                    else
                        return (false, new IModelScope[] { new ModelScope_Empty() });
                case "members":
                    if (_members.Length > 0)
                        return (true, _members.ToArray());
                    else
                        return (false, new IModelScope[] { new ModelScope_Empty() });
                default:
                    return (null, Array.Empty<IModelScope>());
            }
        }
    }
}
