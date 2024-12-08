using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.Gentime
{
    public abstract class ModelScopeDomain : ModelScopeBase
    {
        private readonly TargetDomain _domain;

        private readonly ImmutableArray<ModelScopeEntity> _entities;

        public ModelScopeDomain(IModelScope parent, IScopeFactory factory, ILanguage language, TargetDomain domain) 
            : base(parent, language)
        {
            _domain = domain;

            _variables["DomainName"] = domain.Name;

            _entities = _domain.Entities.Values
                .OrderBy(e => e.Name)
                .Select(e => factory.CreateEntity(this, factory, language, e))
                .ToImmutableArray();
        }

        protected override (bool?, IModelScope[]) OnGetInnerScopes(string iteratorName)
        {
            switch (iteratorName.ToLowerInvariant())
            {
                case "entities":
                    TargetEntity[] entities = _domain.Entities.Values.ToArray();
                    if (entities.Length > 0)
                        return (true, _entities.ToArray());
                    else
                        return (false, new IModelScope[] { ModelScopeEmpty.Instance });
                default:
                    return (null, Array.Empty<IModelScope>());
            }
        }
    }
}
