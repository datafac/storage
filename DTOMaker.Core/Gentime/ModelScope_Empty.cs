using System;
using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public sealed class ModelScope_Empty : IModelScope
    {
        public IDictionary<string, object?> Variables { get; } = new Dictionary<string, object?>();

        public (bool?, IModelScope[]) GetInnerScopes(string iteratorName) => (null, Array.Empty<IModelScope>());
    }
}
