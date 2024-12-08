using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public abstract class ModelScopeBase : IModelScope
    {
        protected readonly ILanguage _language;
        protected readonly Dictionary<string, object?> _variables = new Dictionary<string, object?>();
        public IDictionary<string, object?> Variables => _variables;

        protected ModelScopeBase(IModelScope parent, ILanguage language)
        {
            _language = language;
            foreach (var token in parent.Variables)
            {
                _variables[token.Key] = token.Value;
            }
        }

        protected abstract (bool?, IModelScope[]) OnGetInnerScopes(string iteratorName);
        public (bool?, IModelScope[]) GetInnerScopes(string iteratorName) => OnGetInnerScopes(iteratorName);
    }
}
