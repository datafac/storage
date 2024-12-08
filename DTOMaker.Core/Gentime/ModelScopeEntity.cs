using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.Gentime
{
    public interface IScopeFactory
    {
        ModelScopeEntity CreateEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity);
        ModelScopeMember CreateMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member);
    }
    public abstract class ModelScopeEntity : ModelScopeBase
    {
        private readonly ImmutableArray<ModelScopeEntity> _derivedEntities;
        private readonly Lazy<ImmutableArray<ModelScopeMember>> _membersqqq;

        public ModelScopeEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
            : base(parent, language)
        {
            _derivedEntities = entity.Domain.Entities.Values
                .Where(e => e.IsChildOf(entity))
                .OrderBy(e => e.Name)
                .Select(e => factory.CreateEntity(parent, factory, language, e))
                .ToImmutableArray();

            _variables["EntityName"] = entity.Name;
            _variables["EntityName2"] = entity.Name;
            _variables["BaseName"] = entity.Base?.Name ?? "EntityBase";
            _variables["HasDerivedEntities"] = _derivedEntities.Length > 0;
            _variables["BlockLength"] = entity.BlockLength;

            _membersqqq = new Lazy<ImmutableArray<ModelScopeMember>>(
                () => entity.Members.Values
                    .OrderBy(m => m.Sequence)
                    .Select(m => factory.CreateMember(this, factory, language, m))
                    .ToImmutableArray());
        }

        protected override (bool?, IModelScope[]) OnGetInnerScopes(string iteratorName)
        {
            switch (iteratorName.ToLowerInvariant())
            {
                case "derivedentities":
                    if (_derivedEntities.Length > 0)
                        return (true, _derivedEntities.ToArray());
                    else
                        return (false, new IModelScope[] { ModelScopeEmpty.Instance });
                case "members":
                    var members = _membersqqq.Value;
                    if (members.Length > 0)
                        return (true, members.ToArray());
                    else
                        return (false, new IModelScope[] { ModelScopeEmpty.Instance });
                default:
                    return (null, Array.Empty<IModelScope>());
            }
        }
    }
}
