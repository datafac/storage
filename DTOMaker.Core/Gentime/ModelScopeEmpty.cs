using System;
using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public sealed class ModelScopeEmpty : IModelScope
    {
        private static readonly ModelScopeEmpty _instance = new ModelScopeEmpty();
        public static ModelScopeEmpty Instance => _instance;

        public IDictionary<string, object?> Variables { get; } = new Dictionary<string, object?>();

        private ModelScopeEmpty() { }

        public (bool?, IModelScope[]) GetInnerScopes(string iteratorName) => (null, Array.Empty<IModelScope>());
    }
}
