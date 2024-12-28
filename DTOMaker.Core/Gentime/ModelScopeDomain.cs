using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace DTOMaker.Gentime
{
    public abstract class ModelScopeDomain : ModelScopeBase
    {
        private readonly TargetDomain _domain;

        public ModelScopeDomain(IModelScope parent, IScopeFactory factory, ILanguage language, TargetDomain domain) 
            : base(parent, factory, language)
        {
            _domain = domain;
            _variables["DomainName"] = domain.Name;
        }

        protected override (bool?, IModelScope[]) OnGetInnerScopes(string iteratorName)
        {
            switch (iteratorName.ToLowerInvariant())
            {
                case "entities":
                    var entities = _domain.Entities.Values
                        .OrderBy(e => e.EntityName.FullName)
                        .Select(e => _factory.CreateEntity(this, _factory, _language, e))
                        .ToArray();
                    if (entities.Length > 0)
                        return (true, entities);
                    else
                        return (false, new IModelScope[] { ModelScopeEmpty.Instance });
                default:
                    return (null, Array.Empty<IModelScope>());
            }
        }
    }
}
