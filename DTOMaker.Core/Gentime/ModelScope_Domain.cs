using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DTOMaker.Gentime
{
    public sealed class ModelScope_Domain : IModelScope
    {
        private readonly TargetDomain _domain;
        private readonly ILanguage _language;
        private readonly Dictionary<string, object?> _variables = new Dictionary<string, object?>();
        public IDictionary<string, object?> Variables => _variables;

        public ModelScope_Domain(ILanguage language, TargetDomain domain, IEnumerable<KeyValuePair<string, object?>> tokens)
        {
            _language = language;
            _domain = domain;

            foreach (var token in tokens)
            {
                _variables[token.Key] = token.Value;
            }
            _variables["DomainName"] = domain.Name;
        }

        public (bool?, IModelScope[]) GetInnerScopes(string iteratorName)
        {
            switch (iteratorName.ToLowerInvariant())
            {
                case "entities":
                    TargetEntity[] entities = _domain.Entities.Values.ToArray();
                    if (entities.Length > 0)
                        return (true, entities.OrderBy(e => e.Name).Select(e => new ModelScope_Entity(_language, e, _variables)).ToArray());
                    else
                        return (false, new IModelScope[] { new ModelScope_Empty() });
                default:
                    return (null, Array.Empty<IModelScope>());
            }
        }
    }
}
