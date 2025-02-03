using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public abstract class ModelScopeBase : IModelScope
    {
        protected readonly IModelScope _parent;
        protected readonly IScopeFactory _factory;
        protected readonly ILanguage _language;
        protected readonly Dictionary<string, object?> _tokens = new Dictionary<string, object?>();
        public IReadOnlyDictionary<string, object?> Tokens => _tokens;

        protected ModelScopeBase(IModelScope parent, IScopeFactory factory, ILanguage language)
        {
            _parent = parent;
            _factory = factory;
            _language = language;
            foreach (var token in parent.Tokens)
            {
                _tokens[token.Key] = token.Value;
            }
        }
    }
}
