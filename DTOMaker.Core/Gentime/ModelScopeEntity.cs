using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace DTOMaker.Gentime
{
    public abstract class ModelScopeEntity : ModelScopeBase
    {
        protected readonly TargetEntity _entity;

        public ModelScopeEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
            : base(parent, factory, language)
        {
            _entity = entity;
            _variables["NameSpace"] = entity.EntityName.NameSpace;
            _variables["EntityName"] = entity.EntityName.ShortName;
            _variables["EntityName2"] = entity.EntityName.ShortName;
            _variables["BaseName"] = entity.Base?.EntityName.ShortName ?? EntityFQN.DefaultBase.ShortName;
            _variables["BaseFullName"] = entity.Base?.EntityName.FullName ?? EntityFQN.DefaultBase.FullName;
            _variables["ClassHeight"] = entity.GetClassHeight();

            _variables["DerivedEntityCount"] = _entity.DerivedEntities.Length;
        }

        private static bool IsDerivedFrom(TargetEntity candidate, TargetEntity parent)
        {
            if (ReferenceEquals(candidate, parent)) return false;
            if (candidate.Base is null) return false;
            if (candidate.Base.EntityName.Equals(parent.EntityName)) return true;
            return IsDerivedFrom(candidate.Base, parent);
        }

        protected override (bool?, IModelScope[]) OnGetInnerScopes(string iteratorName)
        {
            switch (iteratorName.ToLowerInvariant())
            {
                case "members":
                    var members = _entity.Members.Values
                            .OrderBy(m => m.Sequence)
                            .Select(m => _factory.CreateMember(this, _factory, _language, m))
                            .ToArray();
                    if (members.Length > 0)
                        return (true, members);
                    else
                        return (false, new IModelScope[] { ModelScopeEmpty.Instance });
                case "derivedentities":
                    var derivedEntities = _entity.DerivedEntities
                        .OrderBy(e => e.EntityName.FullName)
                        .Select(e => _factory.CreateEntity(this, _factory, _language, e))
                        .ToArray();
                    if (derivedEntities.Length > 0)
                        return (true, derivedEntities);
                    else
                        return (false, new IModelScope[] { ModelScopeEmpty.Instance });
                default:
                    return (null, Array.Empty<IModelScope>());
            }
        }
    }
}
